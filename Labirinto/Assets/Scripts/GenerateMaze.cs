using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze: MonoBehaviour
{
    [Header("Room Prefab")]
    [SerializeField] private GameObject roomPrefab;

    [Header("Maze Dimensions")]
    [SerializeField] private int numX = 50; // Número de salas ao longo do eixo X
    [SerializeField] private int numY = 50; // Número de salas ao longo do eixo Y

    // Array 2D para armazenar referências a todas as salas
    private Room[,] rooms;

    // Pilha para retrocesso durante o algoritmo de Busca em Profundidade (DFS)
    private Stack<Room> stack = new Stack<Room>();

    // Flag para indicar se a geração do labirinto está em andamento
    private bool generating = false;

    // Dimensões de cada sala
    private float roomWidth;
    private float roomHeight;

    /// <summary>
    /// Inicializa o labirinto criando instâncias das salas e configurando a grade.
    /// </summary>
    private void Start()
    {
        if (roomPrefab == null)
        {
            Debug.LogError("Room Prefab não está atribuído em GenerateMaze.");
            return;
        }

        // Determina o tamanho da sala com base nos limites do SpriteRenderer do prefab
        CalculateRoomSize();

        // Inicializa o array de salas
        rooms = new Room[numX, numY];

        // Instancia as salas e as posiciona na grade
        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                Vector3 position = new Vector3(x * roomWidth, y * roomHeight, 0f);
                GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity, this.transform);
                roomObj.name = $"Room_{x}_{y}";
                Room room = roomObj.GetComponent<Room>();

                if (room == null)
                {
                    Debug.LogError($"Room Prefab em ({x}, {y}) não possui um componente Room.");
                    continue;
                }

                room.Index = new Vector2Int(x, y);
                rooms[x, y] = room;
            }
        }

        // Inicia a geração do labirinto
        StartCoroutine(GenerateMazeCoroutine());
    }

    /// <summary>
    /// Calcula a largura e altura de uma sala com base nos SpriteRenderers do prefab.
    /// </summary>
    private void CalculateRoomSize()
    {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError("Room Prefab não possui SpriteRenderers.");
            return;
        }

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);
            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;

        Debug.Log($"Tamanho da Sala - Largura: {roomWidth}, Altura: {roomHeight}");
    }

    /// <summary>
    /// Coroutine para gerar o labirinto utilizando o algoritmo de Busca em Profundidade (DFS).
    /// </summary>
    private IEnumerator GenerateMazeCoroutine()
    {
        if (generating)
            yield break;

        generating = true;

        // Reseta todas as salas antes da geração
        ResetRooms();

        // Escolhe a sala inicial (0, 0)
        Room startRoom = rooms[0, 0];
        startRoom.Visited = true;
        stack.Push(startRoom);
        Debug.Log($"Iniciando a geração do labirinto a partir da sala ({startRoom.Index.x}, {startRoom.Index.y})");

        while (stack.Count > 0)
        {
            Room currentRoom = stack.Peek();
            List<Room.Directions> unvisitedDirections = GetUnvisitedDirections(currentRoom);

            if (unvisitedDirections.Count > 0)
            {
                // Escolhe uma direção aleatória
                int randIndex = UnityEngine.Random.Range(0, unvisitedDirections.Count);
                Room.Directions chosenDir = unvisitedDirections[randIndex];

                // Determina as coordenadas do vizinho com base na direção escolhida
                Vector2Int neighborPos = GetNeighborPosition(currentRoom.Index, chosenDir);

                if (IsWithinBounds(neighborPos))
                {
                    Room neighborRoom = rooms[neighborPos.x, neighborPos.y];

                    if (neighborRoom != null && !neighborRoom.Visited)
                    {
                        // Remove as paredes entre a salaAtual e a salaVizinha
                        currentRoom.RemoveWall(chosenDir);
                        neighborRoom.RemoveWall(Room.GetOppositeDirection(chosenDir));

                        // Marca a sala vizinha como visitada e a adiciona à pilha
                        neighborRoom.Visited = true;
                        stack.Push(neighborRoom);

                        Debug.Log($"Conectou Sala ({currentRoom.Index.x}, {currentRoom.Index.y}) à Sala ({neighborRoom.Index.x}, {neighborRoom.Index.y}) via {chosenDir}");
                    }
                }
            }
            else
            {
                // Retrocede se não houver vizinhos não visitados
                Room backtrackedRoom = stack.Pop();
                Debug.Log($"Retrocedendo para a Sala ({backtrackedRoom.Index.x}, {backtrackedRoom.Index.y})");
            }

            // Aguarda o próximo quadro para evitar congelamento
            yield return null;
        }

        generating = false;
        Debug.Log("Geração do labirinto concluída!");

        // Após a geração, oculta o primeiro bloco base
        HideBaseBlock();
    }

    /// <summary>
    /// Oculta o primeiro bloco base após a geração do labirinto.
    /// </summary>
    public void HideBaseBlock()
    {
        // Encontra todos os objetos com a tag "BaseBlock"
        GameObject[] baseBlocks = GameObject.FindGameObjectsWithTag("BaseBlock");

        // Garante que pelo menos um bloco base seja encontrado
        if (baseBlocks.Length > 0)
        {
            // Oculta apenas o primeiro bloco base
            baseBlocks[0].SetActive(false);  // Oculta o primeiro bloco base após a geração
            Debug.Log($"Primeiro bloco base '{baseBlocks[0].name}' foi ocultado.");
        }
        else
        {
            Debug.LogWarning("Nenhum bloco base encontrado com a tag 'BaseBlock'.");
        }
    }

    /// <summary>
    /// Reseta todas as salas para seu estado inicial.
    /// </summary>
    private void ResetRooms()
    {
        foreach (Room room in rooms)
        {
            if (room != null)
            {
                room.Visited = false;
                // Reativa todas as paredes
                foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
                {
                    if (dir == Room.Directions.NONE)
                        continue;

                    room.ActivateWall(dir);
                }
            }
        }

        // Limpa a pilha
        stack.Clear();
        Debug.Log("Todas as salas foram resetadas.");
    }

    /// <summary>
    /// Obtém todas as direções não visitadas a partir da sala atual.
    /// </summary>
    /// <param name="room">Sala atual.</param>
    /// <returns>Lista de direções não visitadas.</returns>
    private List<Room.Directions> GetUnvisitedDirections(Room room)
    {
        List<Room.Directions> directions = new List<Room.Directions>();

        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
        {
            if (dir == Room.Directions.NONE)
                continue;

            Vector2Int neighborPos = GetNeighborPosition(room.Index, dir);

            if (IsWithinBounds(neighborPos))
            {
                Room neighbor = rooms[neighborPos.x, neighborPos.y];
                if (neighbor != null && !neighbor.Visited)
                {
                    directions.Add(dir);
                }
            }
        }

        return directions;
    }

    /// <summary>
    /// Calcula a posição do vizinho com base na posição atual e na direção.
    /// </summary>
    /// <param name="current">Posição atual da sala.</param>
    /// <param name="dir">Direção para o vizinho.</param>
    /// <returns>Posição do vizinho.</returns>
    private Vector2Int GetNeighborPosition(Vector2Int current, Room.Directions dir)
    {
        switch (dir)
        {
            case Room.Directions.TOP:
                return new Vector2Int(current.x, current.y + 1);
            case Room.Directions.RIGHT:
                return new Vector2Int(current.x + 1, current.y);
            case Room.Directions.BOTTOM:
                return new Vector2Int(current.x, current.y - 1);
            case Room.Directions.LEFT:
                return new Vector2Int(current.x - 1, current.y);
            default:
                return current;
        }
    }

    /// <summary>
    /// Verifica se a posição fornecida está dentro dos limites do labirinto.
    /// </summary>
    /// <param name="pos">Posição a ser verificada.</param>
    /// <returns>Verdadeiro se estiver dentro dos limites, caso contrário, falso.</returns>
    private bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < numX && pos.y >= 0 && pos.y < numY;
    }
}
