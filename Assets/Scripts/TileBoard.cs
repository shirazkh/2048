using UnityEngine;
using System.Collections.Generic;

public class TileBoard : MonoBehaviour
{
    private TileGrid grid;
    private List<Tile> tiles;

    public Tile tilePrefab;
    public TileState[] tileStates;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }
    private void Start()
    {
        CreateTile();
        CreateTile();
    }
    
    private void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        
    }

}
