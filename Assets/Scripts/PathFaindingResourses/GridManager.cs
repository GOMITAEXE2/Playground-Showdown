using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public float nodeSize = 1f;
    public LayerMask obstacleLayer;

    private Node[,] grid;

    private void Awake()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];
        Vector3 bottomLeft = transform.position - Vector3.right * gridSize.x / 2 * nodeSize - Vector3.forward * gridSize.y / 2 * nodeSize;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeSize + nodeSize / 2) + Vector3.forward * (y * nodeSize + nodeSize / 2);
                bool isWalkable = !Physics.CheckSphere(worldPoint, nodeSize / 2, obstacleLayer);
                grid[x, y] = new Node(isWalkable, worldPoint, x, y);
            }
        }
    }

    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x - transform.position.x + gridSize.x / 2 * nodeSize) / (gridSize.x * nodeSize));
        float percentY = Mathf.Clamp01((worldPosition.z - transform.position.z + gridSize.y / 2 * nodeSize) / (gridSize.y * nodeSize));

        int x = Mathf.FloorToInt((gridSize.x) * percentX);
        int y = Mathf.FloorToInt((gridSize.y) * percentY);

        return grid[x, y];
    }

    public Node[,] GetGrid()
    {
        return grid;
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (var node in grid)
            {
                Gizmos.color = node.isWalkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }
}
