using UnityEngine;
public class Node
{
    public bool isWalkable; // Indica si el nodo es transitable.
    public Vector3 worldPosition; // Posición del nodo en el mundo.
    public int gridX, gridY; // Coordenadas en la cuadrícula.

    public int gCost; // Costo desde el nodo inicial hasta este nodo.
    public int hCost; // Costo heurístico estimado hasta el nodo final.
    public int fCost => gCost + hCost; // Costo total.

    public Node parent; // Nodo padre para reconstruir el camino.

    public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
