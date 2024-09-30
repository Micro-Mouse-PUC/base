using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField]
    private GameObject roomPrefab;

    // The grid to hold all room instances
    private Room[,] rooms;

    [SerializeField]
    private int numX = 10; // Number of rooms along the X-axis
    [SerializeField]
    private int numY = 10; // Number of rooms along the Y-axis

    // Dimensions of each room
    private float roomWidth;
    private float roomHeight;

    // Stack for backtracking during maze generation
    private Stack<Room> stack = new Stack<Room>();

    // Flag to indicate if maze generation is in progress
    private bool generating = false;

    /// <summary>
    /// Entrance and Exit configurations
    /// </summary>
    private Vector2Int entrance = new Vector2Int(0, 0); // (x, y) for entrance
    private Room.Directions entranceDirection = Room.Directions.BOTTOM; // Direction to remove for entrance

    private Vector2Int exit = new Vector2Int(9, 9); // (x, y) for exit (adjust based on numX and numY)
    private Room.Directions exitDirection = Room.Directions.RIGHT; // Direction to remove for exit

    /// <summary>
    /// Calculates the size of a room based on its sprite renderers.
    /// </summary>
    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError("Room prefab has no SpriteRenderers.");
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
    }

    /// <summary>
    /// Positions and sizes the camera to fit the entire maze.
    /// </summary>
    private void SetCamera()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found.");
            return;
        }

        Camera.main.transform.position = new Vector3(
            numX * roomWidth / 2 - roomWidth / 2,
            numY * roomHeight / 2 - roomHeight / 2,
            -10.0f); // Standard Z position for 2D cameras

        // Adjust orthographic size to fit the maze
        float orthographicSizeX = (numX * roomWidth) / (2f * Camera.main.aspect);
        float orthographicSizeY = (numY * roomHeight) / 2f;
        Camera.main.orthographicSize = Mathf.Max(orthographicSizeX, orthographicSizeY) * 1.1f; // Added padding
    }

    /// <summary>
    /// Initializes the maze grid by instantiating room prefabs.
    /// </summary>
    private void Start()
    {
        GetRoomSize();

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                Vector3 position = new Vector3(i * roomWidth, j * roomHeight, 0.0f);
                GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity, transform);
                roomObj.name = $"Room_{i}_{j}";
                Room room = roomObj.GetComponent<Room>();

                if (room == null)
                {
                    Debug.LogError($"Room prefab at position ({i}, {j}) does not have a Room component.");
                    continue;
                }

                room.Index = new Vector2Int(i, j);
                rooms[i, j] = room;
            }
        }

        SetCamera();
    }

    /// <summary>
    /// Removes a wall from a specified room and its neighboring room.
    /// </summary>
    /// <param name="x">X-coordinate of the room.</param>
    /// <param name="y">Y-coordinate of the room.</param>
    /// <param name="dir">Direction of the wall to remove.</param>
    /// <param name="isBoundary">Indicates if the wall removal is for the maze boundary (entrance/exit).</param>
    private void RemoveRoomWall(int x, int y, Room.Directions dir, bool isBoundary = false)
    {
        // Check if we're removing a boundary wall (excluding entrance and exit)
        if (isBoundary)
        {
            bool isEntrance = (x == entrance.x && y == entrance.y && dir == entranceDirection);
            bool isExit = (x == exit.x && y == exit.y && dir == exitDirection);

            if (!isEntrance && !isExit)
            {
                Debug.LogWarning($"Attempted to remove a boundary wall not designated as entrance or exit at ({x},{y}) {dir}");
                return;
            }
        }
        else
        {
            // Prevent the removal of boundary walls (outermost edges of the maze)
            if ((x == 0 && dir == Room.Directions.LEFT) ||
                (x == numX - 1 && dir == Room.Directions.RIGHT) ||
                (y == 0 && dir == Room.Directions.BOTTOM) ||
                (y == numY - 1 && dir == Room.Directions.TOP))
            {
                Debug.LogWarning($"Attempted to remove an outer boundary wall at ({x},{y}) {dir}");
                return;
            }

            // Additional check for corners to ensure walls stay closed
            if ((x == 0 && y == 0 && (dir == Room.Directions.LEFT || dir == Room.Directions.BOTTOM)) ||
                (x == numX - 1 && y == 0 && (dir == Room.Directions.RIGHT || dir == Room.Directions.BOTTOM)) ||
                (x == 0 && y == numY - 1 && (dir == Room.Directions.LEFT || dir == Room.Directions.TOP)) ||
                (x == numX - 1 && y == numY - 1 && (dir == Room.Directions.RIGHT || dir == Room.Directions.TOP)))
            {
                Debug.LogWarning($"Attempted to remove a corner boundary wall at ({x},{y}) {dir}");
                return;
            }
        }

        // Proceed to remove the wall normally
        rooms[x, y].SetDirFlag(dir, false);
        Debug.Log($"Removed wall {dir} from room ({x},{y})");

        // Determine the opposite direction and the neighboring room's coordinates
        Room.Directions oppositeDir = Room.GetOppositeDirection(dir);
        int neighborX = x, neighborY = y;

        switch (dir)
        {
            case Room.Directions.TOP:
                neighborY += 1;
                break;
            case Room.Directions.RIGHT:
                neighborX += 1;
                break;
            case Room.Directions.BOTTOM:
                neighborY -= 1;
                break;
            case Room.Directions.LEFT:
                neighborX -= 1;
                break;
            default:
                Debug.LogWarning($"Invalid direction {dir} provided for wall removal.");
                return;
        }

        // Remove the opposite wall from the neighboring room if within bounds
        if (neighborX >= 0 && neighborX < numX && neighborY >= 0 && neighborY < numY)
        {
            rooms[neighborX, neighborY].SetDirFlag(oppositeDir, false);
            Debug.Log($"Removed wall {oppositeDir} from neighboring room ({neighborX},{neighborY})");
        }
    }



    /// <summary>
    /// Retrieves all unvisited neighboring rooms of a given room.
    /// </summary>
    /// <param name="cx">Current room's X-coordinate.</param>
    /// <param name="cy">Current room's Y-coordinate.</param>
    /// <returns>List of tuples containing the direction and the neighboring room.</returns>
    private List<Tuple<Room.Directions, Room>> GetNeighboursNotVisited(int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbours = new List<Tuple<Room.Directions, Room>>();

        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
        {
            if (dir == Room.Directions.NONE)
                continue;

            int nx = cx, ny = cy;
            switch (dir)
            {
                case Room.Directions.TOP:
                    ny += 1;
                    break;
                case Room.Directions.RIGHT:
                    nx += 1;
                    break;
                case Room.Directions.BOTTOM:
                    ny -= 1;
                    break;
                case Room.Directions.LEFT:
                    nx -= 1;
                    break;
            }

            // Check bounds
            if (nx >= 0 && nx < numX && ny >= 0 && ny < numY)
            {
                Room neighbour = rooms[nx, ny];
                if (!neighbour.visited)
                {
                    neighbours.Add(new Tuple<Room.Directions, Room>(dir, neighbour));
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Performs a single step in the maze generation algorithm.
    /// </summary>
    /// <returns>True if maze generation is complete; otherwise, false.</returns>
    private bool GenerateStep()
    {
        if (stack.Count == 0)
            return true; // Generation complete

        Room currentRoom = stack.Peek();
        List<Tuple<Room.Directions, Room>> neighbours = GetNeighboursNotVisited(currentRoom.Index.x, currentRoom.Index.y);

        if (neighbours.Count > 0)
        {
            // Choose a random unvisited neighbor
            int randIndex = UnityEngine.Random.Range(0, neighbours.Count);
            var chosen = neighbours[randIndex];
            Room.Directions direction = chosen.Item1;
            Room neighbour = chosen.Item2;

            // Remove the wall between the current room and the chosen neighbor
            RemoveRoomWall(currentRoom.Index.x, currentRoom.Index.y, direction);
            Debug.Log($"Connecting room ({currentRoom.Index.x},{currentRoom.Index.y}) to room ({neighbour.Index.x},{neighbour.Index.y}) via {direction}");

            // Mark the neighbor as visited and push it to the stack
            neighbour.visited = true;
            stack.Push(neighbour);
        }
        else
        {
            // Backtrack if no unvisited neighbors
            Room backtrackedRoom = stack.Pop();
            Debug.Log($"Backtracking to room ({backtrackedRoom.Index.x},{backtrackedRoom.Index.y})");
        }

        return false; // Generation not yet complete
    }

    /// <summary>
    /// Initiates the maze generation process.
    /// </summary>
    public void CreateMaze()
    {
        if (generating)
            return; // Prevent multiple simultaneous generations

        ResetMaze();

        // Define entrance and exit
        //RemoveRoomWall(entrance.x, entrance.y, entranceDirection, true); // Entrance
        //#RemoveRoomWall(exit.x, exit.y, exitDirection, true); // Exit

        // Start from the entrance
        Room startingRoom = rooms[entrance.x, entrance.y];
        startingRoom.visited = true; // Mark as visited
        stack.Push(startingRoom);
        Debug.Log($"Starting maze generation from room ({entrance.x},{entrance.y})");

        // Start the coroutine for step-by-step generation
        StartCoroutine(Coroutine_Generate());
    }

    /// <summary>
    /// Coroutine that handles the maze generation over time.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator Coroutine_Generate()
    {
        generating = true;
        bool generationComplete = false;

        while (!generationComplete)
        {
            generationComplete = GenerateStep();
            yield return new WaitForSeconds(0.02f); // Adjust speed as needed
        }

        generating = false;
        Debug.Log("Maze generation complete!");
    }

    /// <summary>
    /// Resets the maze to its initial state.
    /// </summary>
    private void ResetMaze()
    {
        foreach (Room room in rooms)
        {
            room.visited = false;
            room.SetDirFlag(Room.Directions.TOP, true);
            room.SetDirFlag(Room.Directions.RIGHT, true);
            room.SetDirFlag(Room.Directions.BOTTOM, true);
            room.SetDirFlag(Room.Directions.LEFT, true);
        }

        stack.Clear();
        Debug.Log("Maze has been reset.");
    }

    /// <summary>
    /// Listens for user input to start maze generation.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !generating)
        {
            CreateMaze();
        }
    }
}
