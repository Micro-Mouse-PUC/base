using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cat : MonoBehaviour
{
    // Reference to the player
    private Transform player;

    // Speed at which the cat moves
    public float speed = 2f;

    // Reference to Rigidbody2D
    private Rigidbody2D rb;

    // Current path as a list of rooms
    private List<Room> currentPath = new List<Room>();

    // Current target waypoint index in the path
    private int targetWaypoint = 0;

    // Reference to the maze's GenerateMaze script to access rooms
    private GenerateMaze mazeGenerator;

    // Reference to PathVisualizer
    private PathVisualizer pathVisualizer;

    // Reference to Pathfinding (Singleton)
    private Pathfinding pathfinder;

    private void Start()
    {
        // Initialize Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing from the cat!");
        }

        // Find the player in the scene (ensure the player has the tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Ensure the player has the tag 'Player'.");
        }

        // Access the Pathfinding Singleton
        pathfinder = Pathfinding.Instance;
        if (pathfinder == null)
        {
            Debug.LogError("Pathfinding instance not found! Ensure a GameObject with the Pathfinding script is present in the scene.");
        }

        // Find the GenerateMaze component
        mazeGenerator = FindObjectOfType<GenerateMaze>();
        if (mazeGenerator == null)
        {
            Debug.LogError("GenerateMaze component not found in the scene!");
        }

        // Find the PathVisualizer component
        pathVisualizer = FindObjectOfType<PathVisualizer>();
        if (pathVisualizer == null)
        {
            Debug.LogError("PathVisualizer component not found in the scene!");
        }

        // Start pathfinding coroutine
        StartCoroutine(UpdatePathRoutine());
    }

    private void FixedUpdate()
    {
        MoveAlongPath();
    }

    /// <summary>
    /// Moves the cat along the current path towards the player.
    /// </summary>
    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.Log("No current path for the cat.");
            return;
        }

        if (targetWaypoint >= currentPath.Count)
        {
            Debug.Log("Cat has reached the end of the path.");
            return;
        }

        // Get the target room's position
        Vector3 targetPosition = currentPath[targetWaypoint].transform.position;
        Debug.Log($"Cat moving towards waypoint {targetWaypoint} at position {targetPosition}.");

        // Calculate direction towards the target
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Set velocity for smooth movement
        rb.velocity = direction * speed;

        // Check if the cat has reached the target position
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < 0.1f)
        {
            Debug.Log($"Cat reached waypoint {targetWaypoint}.");
            targetWaypoint++;
        }
    }

    /// <summary>
    /// Coroutine to periodically update the cat's path to the player.
    /// </summary>
    private IEnumerator UpdatePathRoutine()
    {
        while (true)
        {
            if (pathfinder != null && mazeGenerator != null && player != null && pathVisualizer != null)
            {
                // Get current room of the cat
                Room currentRoom = GetCurrentRoom();
                // Get player's current room
                Room playerRoom = GetRoomFromPosition(player.position);

                if (currentRoom != null && playerRoom != null)
                {
                    // Find path using A*
                    List<Room> path = pathfinder.FindPath(currentRoom, playerRoom);
                    if (path != null)
                    {
                        currentPath = path;
                        targetWaypoint = 1; // Start moving towards the first waypoint after current room
                        Debug.Log("Path updated for the cat.");

                        // Update the PathVisualizer with the new path
                        pathVisualizer.SetPath(currentPath);
                    }
                    else
                    {
                        Debug.LogWarning("No path found for the cat to reach the player.");
                    }
                }
                else
                {
                    Debug.LogWarning("Current room or Player's room is null.");
                }
            }

            // Update path every 0.5 seconds for better responsiveness
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Determines the current room the cat is in based on its position.
    /// </summary>
    /// <returns>The current Room.</returns>
    private Room GetCurrentRoom()
    {
        foreach (Room room in mazeGenerator.GetRooms())
        {
            if (IsWithinRoom(transform.position, room))
            {
                return room;
            }
        }
        return null;
    }

    /// <summary>
    /// Determines the room based on a position.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <returns>The corresponding Room.</returns>
    private Room GetRoomFromPosition(Vector3 position)
    {
        foreach (Room room in mazeGenerator.GetRooms())
        {
            if (IsWithinRoom(position, room))
            {
                return room;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a position is within a room's boundaries.
    /// </summary>
    private bool IsWithinRoom(Vector3 position, Room room)
    {
        // Assuming rooms are aligned on integer grid and have uniform size
        float halfWidth = mazeGenerator.GetRoomWidth() / 2f;
        float halfHeight = mazeGenerator.GetRoomHeight() / 2f;

        Vector3 roomCenter = room.transform.position;
        return Mathf.Abs(position.x - roomCenter.x) <= halfWidth &&
               Mathf.Abs(position.y - roomCenter.y) <= halfHeight;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Log the event for debugging
            Debug.Log("Player caught by the cat! Returning to Menu.");

            // Optionally, add a delay or animation here

            // Load the Menu scene
            SceneManager.LoadScene("MainMenu"); // Ensure "Menu" matches the exact name of your Menu scene
        }
    }

    /// <summary>
    /// Optional: Visualize the path using Gizmos for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        Gizmos.color = Color.red;
        foreach (Room room in currentPath)
        {
            if (room != null)
                Gizmos.DrawSphere(room.transform.position, 0.2f);
        }
    }
}
