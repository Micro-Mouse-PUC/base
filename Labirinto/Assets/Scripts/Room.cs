using System;
using System.Collections.Generic;
using UnityEngine;

public class Room: MonoBehaviour
{
    public enum Directions
    {
        TOP,
        RIGHT,
        BOTTOM,
        LEFT,
        NONE,
    }

    [Header("Objetos das Paredes")]
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject leftWall;

    // Nova referência para o bloco base que precisa ser ocultado após a geração
    [Header("Bloco Base")]
    [SerializeField] private GameObject baseBlock;

    // Dicionário para mapear direções aos respectivos GameObjects das paredes
    private Dictionary<Directions, GameObject> walls = new Dictionary<Directions, GameObject>();

    // Índice de posição da sala na grade do labirinto
    public Vector2Int Index { get; set; }

    // Flag para indicar se a sala foi visitada durante a geração do labirinto
    public bool Visited { get; set; } = false;


    private void Awake()
    {
        // Inicializa o dicionário de paredes
        walls[Directions.TOP] = topWall;
        walls[Directions.RIGHT] = rightWall;
        walls[Directions.BOTTOM] = bottomWall;
        walls[Directions.LEFT] = leftWall;

        // Inicializa todas as paredes como ativas
        foreach (Directions dir in Enum.GetValues(typeof(Directions)))
        {
            if (dir == Directions.NONE)
                continue;

            if (walls.ContainsKey(dir) && walls[dir] != null)
            {
                ActivateWall(dir);
            }
            else
            {
                Debug.LogError($"Sala '{gameObject.name}' está faltando a atribuição da parede {dir}.");
            }

            // Inicializa o bloco base, se necessário
            if (baseBlock != null)
            {
                baseBlock.SetActive(true);  // Inicialmente, o bloco base está visível
            }
        }
    }

    /// <summary>
    /// Ativa uma parede na direção especificada e habilita seu colisor.
    /// </summary>
    /// <param name="dir">Direção da parede a ser ativada.</param>
    public void ActivateWall(Directions dir)
    {
        if (dir == Directions.NONE)
            return;

        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            // Ativa o GameObject da parede (torna-a visível)
            walls[dir].SetActive(true);

            // Habilita o colisor para bloquear a passagem
            Collider2D collider = walls[dir].GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;  // Habilita o colisor
            }

            Debug.Log($"Sala ({Index.x}, {Index.y}) - Parede {dir} ativada e seu colisor habilitado.");
        }
        else
        {
            Debug.LogWarning($"Sala ({Index.x}, {Index.y}) - Parede {dir} está ausente ou não foi atribuída.");
        }
    }

    /// <summary>
    /// Remove uma parede na direção especificada desativando seu GameObject e colisor.
    /// </summary>
    /// <param name="dir">Direção da parede a ser removida.</param>
    public void RemoveWall(Directions dir)
    {
        if (dir == Directions.NONE)
            return;

        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            // Desativa o GameObject da parede (torna-a invisível)
            walls[dir].SetActive(false);

            // Desabilita o colisor para permitir a passagem
            Collider2D collider = walls[dir].GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;  // Desabilita o colisor
            }

            Debug.Log($"Sala ({Index.x}, {Index.y}) - Parede {dir} removida e seu colisor desabilitado.");
        }
        else
        {
            Debug.LogWarning($"Sala ({Index.x}, {Index.y}) - Não foi possível remover a parede {dir} pois não está atribuída.");
        }
    }

    /// <summary>
    /// Verifica se uma parede está ativa na direção especificada.
    /// </summary>
    /// <param name="dir">Direção da parede.</param>
    /// <returns>Verdadeiro se ativa, caso contrário, falso.</returns>
    public bool IsWallActive(Directions dir)
    {
        if (dir == Directions.NONE)
            return false;

        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            return walls[dir].activeSelf;
        }

        Debug.LogWarning($"Sala ({Index.x}, {Index.y}) - Parede {dir} está ausente ou não foi atribuída.");
        return false;
    }

    /// <summary>
    /// Obtém a direção oposta à direção original.
    /// </summary>
    /// <param name="dir">Direção original.</param>
    /// <returns>Direção oposta.</returns>
    public static Directions GetOppositeDirection(Directions dir)
    {
        switch (dir)
        {
            case Directions.TOP:
                return Directions.BOTTOM;
            case Directions.RIGHT:
                return Directions.LEFT;
            case Directions.BOTTOM:
                return Directions.TOP;
            case Directions.LEFT:
                return Directions.RIGHT;
            default:
                return Directions.NONE;
        }
    }
}
