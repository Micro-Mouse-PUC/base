using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // Singleton instance
    public static Pathfinding Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
            Debug.Log("Pathfinding Singleton Initialized.");
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            Debug.LogWarning("Duplicate Pathfinding Singleton Destroyed.");
            return;
        }
    }

    /// <summary>
    /// Finds the shortest path between start and end rooms using the A* algorithm.
    /// </summary>
    public List<Room> FindPath(Room start, Room end)
    {
        if (start == null || end == null)
        {
            Debug.LogError("Start or End room is null.");
            return null;
        }

        List<Room> openSet = new List<Room> { start };
        HashSet<Room> closedSet = new HashSet<Room>();
        Dictionary<Room, Room> cameFrom = new Dictionary<Room, Room>();

        Dictionary<Room, float> gScore = new Dictionary<Room, float> { { start, 0 } };
        Dictionary<Room, float> fScore = new Dictionary<Room, float> { { start, Heuristic(start, end) } };

        while (openSet.Count > 0)
        {
            // Get the room in openSet with the lowest fScore
            Room current = openSet[0];
            foreach (Room room in openSet)
            {
                if (fScore.ContainsKey(room) && fScore[room] < fScore[current])
                {
                    current = room;
                }
            }

            Debug.Log($"Evaluating Room: {current.gameObject.name}");

            if (current == end)
            {
                Debug.Log("Path found!");
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Room neighbor in current.Neighbors)
            {
                Debug.Log($"Checking neighbor: {neighbor.gameObject.name} of Room: {current.gameObject.name}");

                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + 1; // Assuming uniform cost

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (gScore.ContainsKey(neighbor) && tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                // This path is the best until now. Record it.
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
            }
        }

        // No path found
        Debug.LogWarning("No path found.");
        return null;
    }

    /// <summary>
    /// Heuristic function for A* (Manhattan distance).
    /// </summary>
    private float Heuristic(Room a, Room b)
    {
        return Mathf.Abs(a.Index.x - b.Index.x) + Mathf.Abs(a.Index.y - b.Index.y);
    }

    /// <summary>
    /// Reconstructs the path from start to end using the cameFrom map.
    /// </summary>
    private List<Room> ReconstructPath(Dictionary<Room, Room> cameFrom, Room current)
    {
        List<Room> totalPath = new List<Room> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();

        // Log the path
        string pathLog = "Path: ";
        foreach (Room room in totalPath)
        {
            pathLog += $"{room.gameObject.name} -> ";
        }
        Debug.Log(pathLog.TrimEnd(' ', '-', '>'));

        return totalPath;
    }
}
