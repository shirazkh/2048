using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;      // prefab for new tiles (2 by default)
    [SerializeField] private TileState[] tileStates; // ordered states

    private TileGrid grid;                         // grid layout & cell access
    private List<Tile> tiles;                      // runtime tiles on board
    private bool waiting;                          // gate: block input during settle/animations

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>(); // find grid in children
        tiles = new List<Tile>(16);                // 4x4 board capacity
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells) {
            cell.tile = null;                      // detach all tiles from cells
        }

        foreach (var tile in tiles) {
            Destroy(tile.gameObject);              // remove tile objects 
        }

        tiles.Clear();                             // reset runtime list
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform); // spawn under grid
        tile.SetState(tileStates[0]);                        // base state (2)
        tile.Spawn(grid.GetRandomEmptyCell());               // place in empty cell
        tiles.Add(tile);                                     // track it
    }

    private void Update()
    {
        if (waiting) return;                      // ignore input while board settles

        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            Move(Vector2Int.up, 0, 1, 1, 1);
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            Move(Vector2Int.left, 1, 1, 0, 1);
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            Move(Vector2Int.down, 0, 1, grid.Height - 2, -1);
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(Vector2Int.right, grid.Width - 2, -1, 0, 1);
        }
    }

    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;                      // tracks if any tile moved/merged

        // scan board in an order that pushes tiles toward 'direction'
        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.Occupied) {
                    changed |= MoveTile(cell.tile, direction); // accumulate changes
                }
            }
        }

        if (changed) {
            StartCoroutine(WaitForChanges());     // settle => spawn new tile => check end
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;                                  // furthest empty cell we can slide to
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        // walk forward until blocked or out of bounds
        while (adjacent != null)
        {
            if (adjacent.Occupied)
            {
                // merge only if same state and target not yet merged this turn
                if (CanMerge(tile, adjacent.tile))
                {
                    MergeTiles(tile, adjacent.tile);              // merge into target
                    return true;
                }

                break;                                            // different tile blocks movement
            }

            newCell = adjacent;                                   // remember last empty
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);                                 // slide animation
            return true;
        }

        return false;                                             // no move possible
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.state == b.state && !b.locked;                   // each tile merges once per turn
    }

    private void MergeTiles(Tile a, Tile b)
    {
        tiles.Remove(a);                                          // 'a' will be destroyed after anim
        a.Merge(b.cell);                                          // animate into 'b' position

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];

        b.SetState(newState);                                     // upgrade merged tile
        GameManager.Instance.IncreaseScore(newState.number);      // award points = new tile value
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i]) {
                return i;                                         // find current state's index
            }
        }

        return -1;                                                // should not happen if states are valid
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;                                           // gate input during settle

        yield return new WaitForSeconds(0.1f);                    // small delay to finish anims

        waiting = false;

        foreach (var tile in tiles) {
            tile.locked = false;                                  // unlock for next turn's merges
        }

        if (tiles.Count != grid.Size) {
            CreateTile();                                         // spawn one new tile after a valid move
        }

        if (CheckForGameOver()) {
            GameManager.Instance.GameOver();                      // no space and no merges => end game
        }
    }

    public bool CheckForGameOver()
    {
        if (tiles.Count != grid.Size) {
            return false;                                         // space exists => not over
        }

        // if any neighbor can merge, game continues
        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.tile)) {
                return false;
            }

            if (down != null && CanMerge(tile, down.tile)) {
                return false;
            }

            if (left != null && CanMerge(tile, left.tile)) {
                return false;
            }

            if (right != null && CanMerge(tile, right.tile)) {
                return false;
            }
        }

        return true;                                              // full board and no merges => game over
    }

}
