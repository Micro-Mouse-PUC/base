// GenerateMaze.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    [Header("Room Prefab")]
    [SerializeField] private GameObject roomPrefab;

    [Header("Maze Dimensions")]
    [SerializeField] private int numX = 10; // Number of rooms along the X-axis
    [SerializeField] private int numY = 10; // Number of rooms along the Y-axis

    [Header("Enemy and Goal Prefabs")]
    [SerializeField] private GameObject catPrefab;     // Prefab for the cat enemy
    [SerializeField] private GameObject cheesePrefab;  // Prefab for the cheese goal

    // 2D array to store references to all rooms
    private Room[,] rooms;

    // Stack for backtracking during Depth-First Search (DFS)
    private Stack<Room> stack = new Stack<Room>();

    // Flag to indicate if maze generation is in progress
    private bool generating = false;

    // Dimensions of each room
    private float roomWidth;
    private float roomHeight;

    /// <summary>
    /// Initializes the maze by creating room instances and setting up the grid.
    /// </summary>
    private void Start()
    {
        if (roomPrefab == null)
        {
            Debug.LogError("Room Prefab is not assigned in GenerateMaze.");
            return;
        }

        // Determine room size based on the SpriteRenderer bounds of the prefab
        CalculateRoomSize();

        // Initialize the rooms array
        rooms = new Room[numX, numY];

        // Instantiate rooms and position them in the grid
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
                    Debug.LogError($"Room Prefab at ({x}, {y}) does not have a Room component.");
                    continue;
                }

                room.Index = new Vector2Int(x, y);
                rooms[x, y] = room;
            }
        }

        // Start maze generation
        StartCoroutine(GenerateMazeCoroutine());
    }

    /// <summary>
    /// Calculates the width and height of a room based on the prefab's SpriteRenderers.
    /// </summary>
    private void CalculateRoomSize()
    {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError("Room Prefab does not have any SpriteRenderers.");
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

        Debug.Log($"Room Size - Width: {roomWidth}, Height: {roomHeight}");
    }

    /// <summary>
    /// Coroutine to generate the maze using Depth-First Search (DFS) algorithm.
    /// </summary>
    private IEnumerator GenerateMazeCoroutine()
    {
        if (generating)
            yield break;

        generating = true;

        // Reset all rooms before generation
        ResetRooms();

        // Choose the starting room (0, 0)
        Room startRoom = rooms[0, 0];
        startRoom.Visited = true;
        stack.Push(startRoom);
        Debug.Log($"Starting maze generation from Room ({startRoom.Index.x}, {startRoom.Index.y})");

        while (stack.Count > 0)
        {
            Room currentRoom = stack.Peek();
            List<Room.Directions> unvisitedDirections = GetUnvisitedDirections(currentRoom);

            if (unvisitedDirections.Count > 0)
            {
                // Choose a random direction
                int randIndex = UnityEngine.Random.Range(0, unvisitedDirections.Count);
                Room.Directions chosenDir = unvisitedDirections[randIndex];

                // Determine the neighbor's position based on the chosen direction
                Vector2Int neighborPos = GetNeighborPosition(currentRoom.Index, chosenDir);

                if (IsWithinBounds(neighborPos))
                {
                    Room neighborRoom = rooms[neighborPos.x, neighborPos.y];

                    if (neighborRoom != null && !neighborRoom.Visited)
                    {
                        // Remove walls between current and neighbor rooms
                        currentRoom.RemoveWall(chosenDir, neighborRoom);
                        neighborRoom.RemoveWall(Room.GetOppositeDirection(chosenDir), currentRoom);

                        // Mark the neighbor as visited and push it to the stack
                        neighborRoom.Visited = true;
                        stack.Push(neighborRoom);

                        Debug.Log($"Connected Room ({currentRoom.Index.x}, {currentRoom.Index.y}) to Room ({neighborRoom.Index.x}, {neighborRoom.Index.y}) via {chosenDir}");
                    }
                }
            }
            else
            {
                // Backtrack if no unvisited neighbors
                Room backtrackedRoom = stack.Pop();
                Debug.Log($"Backtracking to Room ({backtrackedRoom.Index.x}, {backtrackedRoom.Index.y})");
            }

            // Wait for the next frame to avoid freezing
            yield return null;
        }

        generating = false;
        Debug.Log("Maze generation completed!");

        // Place the cat and cheese after the maze is generated
        PlaceCatAndCheese();
    }

    /// <summary>
    /// Places the cat (enemy) and cheese (goal) randomly in the maze.
    /// </summary>
    private void PlaceCatAndCheese()
    {
        // Collect all possible rooms excluding the starting room (0,0)
        List<Room> possibleRooms = new List<Room>();
        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                if (x == 0 && y == 0)
                    continue; // Exclude the starting room
                possibleRooms.Add(rooms[x, y]);
            }
        }

        if (possibleRooms.Count < 2)
        {
            Debug.LogError("Not enough rooms to place both the cat and the cheese.");
            return;
        }

        // Shuffle the list to randomize room selection
        for (int i = 0; i < possibleRooms.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, possibleRooms.Count);
            Room temp = possibleRooms[i];
            possibleRooms[i] = possibleRooms[rnd];
            possibleRooms[rnd] = temp;
        }

        // Select the first room for the cat and the second for the cheese
        Room catRoom = possibleRooms[0];
        Room cheeseRoom = possibleRooms[1];

        // Instantiate the cat
        if (catPrefab != null)
        {
            Vector3 catPosition = catRoom.transform.position;
            Instantiate(catPrefab, catPosition, Quaternion.identity, catRoom.transform);
            Debug.Log($"Cat placed at Room ({catRoom.Index.x}, {catRoom.Index.y})");
        }
        else
        {
            Debug.LogError("Cat Prefab is not assigned in GenerateMaze.");
        }

        // Instantiate the cheese
        if (cheesePrefab != null)
        {
            Vector3 cheesePosition = cheeseRoom.transform.position;
            Instantiate(cheesePrefab, cheesePosition, Quaternion.identity, cheeseRoom.transform);
            Debug.Log($"Cheese placed at Room ({cheeseRoom.Index.x}, {cheeseRoom.Index.y})");
        }
        else
        {
            Debug.LogError("Cheese Prefab is not assigned in GenerateMaze.");
        }
    }

    /// <summary>
    /// Resets all rooms to their initial state.
    /// </summary>
    private void ResetRooms()
    {
        foreach (Room room in rooms)
        {
            if (room != null)
            {
                room.Visited = false;
                // Reactivate all walls
                foreach (Room.Directions dir in System.Enum.GetValues(typeof(Room.Directions)))
                {
                    if (dir == Room.Directions.NONE)
                        continue;

                    room.ActivateWall(dir);
                }
            }
        }

        // Clear the stack
        stack.Clear();
        Debug.Log("All rooms have been reset.");
    }

    /// <summary>
    /// Gets all unvisited directions from the current room.
    /// </summary>
    /// <param name="room">Current room.</param>
    /// <returns>List of unvisited directions.</returns>
    private List<Room.Directions> GetUnvisitedDirections(Room room)
    {
        List<Room.Directions> directions = new List<Room.Directions>();

        foreach (Room.Directions dir in System.Enum.GetValues(typeof(Room.Directions)))
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
    /// Calculates the neighbor's position based on the current position and direction.
    /// </summary>
    /// <param name="current">Current room position.</param>
    /// <param name="dir">Direction to the neighbor.</param>
    /// <returns>Neighbor room position.</returns>
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
    /// Checks if the given position is within the maze bounds.
    /// </summary>
    /// <param name="pos">Position to check.</param>
    /// <returns>True if within bounds, else false.</returns>
    private bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < numX && pos.y >= 0 && pos.y < numY;
    }

    /// <summary>
    /// Returns all rooms in the maze.
    /// </summary>
    public IEnumerable<Room> GetRooms()
    {
        foreach (Room room in rooms)
        {
            if (room != null)
                yield return room;
        }
    }

    /// <summary>
    /// Gets the width of a room.
    /// </summary>
    public float GetRoomWidth()
    {
        return roomWidth;
    }

    /// <summary>
    /// Gets the height of a room.
    /// </summary>
    public float GetRoomHeight()
    {
        return roomHeight;
    }
}
