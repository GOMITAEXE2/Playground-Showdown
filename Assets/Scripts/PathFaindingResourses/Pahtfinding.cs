using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Transform character; // El personaje que se moverá.

    public float moveSpeed = 5f; // Velocidad del movimiento.
    private GridManager gridManager;

    private List<Node> path;
    private int currentNodeIndex = 0;
    private Vector3 lastEndPointPosition; // Almacena la última posición del objetivo.

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        lastEndPointPosition = endPoint.position;
        FindPath(startPoint.position, lastEndPointPosition);
    }

    private void Update()
    {
        // Si la posición del objetivo ha cambiado, recalcular la ruta.
        if (endPoint.position != lastEndPointPosition)
        {
            lastEndPointPosition = endPoint.position;
            FindPath(character.position, lastEndPointPosition);
        }

        if (path != null && currentNodeIndex < path.Count)
        {
            MoveCharacter();
        }
    }

    private void MoveCharacter()
    {
        // Obtiene la posición del nodo actual en el camino.
        Vector3 targetPosition = path[currentNodeIndex].worldPosition;

        // Mueve el personaje hacia el nodo actual.
        character.position = Vector3.MoveTowards(character.position, targetPosition, moveSpeed * Time.deltaTime);

        // Si llega al nodo actual, pasa al siguiente.
        if (Vector3.Distance(character.position, targetPosition) < 0.1f)
        {
            currentNodeIndex++;
        }
    }

    private void FindPath(Vector3 startPos, Vector3 endPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPosition(startPos);
        Node endNode = gridManager.GetNodeFromWorldPosition(endPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridManager.gridSize.x && checkY >= 0 && checkY < gridManager.gridSize.y)
                {
                    neighbors.Add(gridManager.GetGrid()[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        currentNodeIndex = 0; // Reinicia el índice de los nodos.
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            foreach (Node node in path)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (gridManager.nodeSize - 0.1f));
            }
        }
    }
}
