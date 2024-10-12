using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    // Public variables to customize the visualization
    public Color pathColor = Color.green;
    public float lineWidth = 0.2f;

    // The LineRenderer component
    private LineRenderer lineRenderer;

    // The current path to visualize
    private List<Room> path = new List<Room>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component missing from PathVisualizer.");
        }

        // Configure LineRenderer properties
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Updates the path to be visualized.
    /// </summary>
    /// <param name="newPath">List of rooms representing the path.</param>
    public void SetPath(List<Room> newPath)
    {
        path = newPath;
        UpdateLineRenderer();
    }

    /// <summary>
    /// Updates the LineRenderer with the current path.
    /// </summary>
    private void UpdateLineRenderer()
    {
        if (path == null || path.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].transform.position);
        }
    }
}
