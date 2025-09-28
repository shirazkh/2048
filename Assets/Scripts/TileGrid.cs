using UnityEngine;

// holds rows & cells, resolves coordinates and neighbors.
public class TileGrid : MonoBehaviour
{
    public TileRow[] rows { get; private set; }   // rows[y] gives access to cells in row y
    public TileCell[] cells { get; private set; } // flat list of all cells (row-major)

    public int Size => cells.Length;              // total number of cells (Width * Height)
    public int Height => rows.Length;             // number of rows (Y)
    public int Width => Size / Height;            // number of columns (X)

    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();   // collect row components
        cells = GetComponentsInChildren<TileCell>();  // collect all cell components

        // assign (x,y) coordinates to each cell (row-major order)
        for (int i = 0; i < cells.Length; i++) {
            cells[i].coordinates = new Vector2Int(i % Width, i / Width);
        }
    }

    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y); // convenience overload
    }

    public TileCell GetCell(int x, int y)
    {
        // bounds check; return null if outside the grid
        if (x >= 0 && x < Width && y >= 0 && y < Height) {
            return rows[y].cells[x]; // access via row, then column
        } else {
            return null;
        }
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y; // note: Y is inverted so "up" means y-1 here

        return GetCell(coordinates);
    }

    public TileCell GetRandomEmptyCell()
    {
        // pick a random start and scan forward until an empty cell is found
        int index = Random.Range(0, cells.Length);
        int startingIndex = index;

        while (cells[index].Occupied)
        {
            index++;

            if (index >= cells.Length) {
                index = 0; // wrap around
            }

            // all cells are occupied
            if (index == startingIndex) {
                return null;
            }
        }

        return cells[index];
    }

}
