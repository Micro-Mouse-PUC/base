// Room.cs
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum Directions
    {
        TOP,
        RIGHT,
        BOTTOM,
        LEFT,
        NONE,
    }

    [Header("Wall Objects")]
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject leftWall;

    // Index of the room in the maze grid
    public Vector2Int Index { get; set; }

    // Flag to indicate if the room has been visited during maze generation
    public bool Visited { get; set; } = false;

    // List of accessible neighboring rooms
    public List<Room> Neighbors { get; private set; } = new List<Room>();

    private Dictionary<Directions, GameObject> walls = new Dictionary<Directions, GameObject>();

    private void Awake()
    {
        // Initialize walls dictionary
        walls[Directions.TOP] = topWall;
        walls[Directions.RIGHT] = rightWall;
        walls[Directions.BOTTOM] = bottomWall;
        walls[Directions.LEFT] = leftWall;

        // Activate all walls initially
        foreach (Directions dir in System.Enum.GetValues(typeof(Directions)))
        {
            if (dir == Directions.NONE)
                continue;

            if (walls.ContainsKey(dir) && walls[dir] != null)
            {
                ActivateWall(dir);
            }
            else
            {
                Debug.LogError($"Room '{gameObject.name}' is missing the wall for direction {dir}.");
            }
        }
    }

    /// <summary>
    /// Activates a wall in the specified direction.
    /// </summary>
    /// <param name="dir">Direction of the wall to activate.</param>
    public void ActivateWall(Directions dir)
    {
        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            walls[dir].SetActive(true);
            Collider2D collider = walls[dir].GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// Removes a wall in the specified direction and adds the neighbor to the list.
    /// </summary>
    /// <param name="dir">Direction of the wall to remove.</param>
    /// <param name="neighborRoom">The neighboring room being connected.</param>
    public void RemoveWall(Directions dir, Room neighborRoom)
    {
        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            walls[dir].SetActive(false);
            Collider2D collider = walls[dir].GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Add neighbor to the accessible neighbors list
            Neighbors.Add(neighborRoom);
        }
    }

    /// <summary>
    /// Gets the opposite direction.
    /// </summary>
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
