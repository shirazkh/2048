using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Represents a single tile: knows its state, current cell, and handles its own animations.
public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }  // logical value + colors (from ScriptableObject)
    public TileCell cell { get; private set; }    // current grid cell this tile occupies
    public bool locked { get; set; }              // prevents multiple merges in the same turn

    private Image background;                     // tile background (colored per state)
    private TextMeshProUGUI text;                 // numeric label

    private void Awake()
    {
        background = GetComponent<Image>();                       // cache UI refs
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state)
    {
        this.state = state;                                       // update logical state

        background.color = state.backgroundColor;                 // apply theme
        text.color = state.textColor;
        text.text = state.number.ToString();                      // update label
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;                                // detach from previous cell
        }

        this.cell = cell;                                         
        this.cell.tile = this;

        transform.position = cell.transform.position;             // snap to cell position 
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;                                // free previous cell
        }

        this.cell = cell;                                         // occupy new cell
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));  // slide animation (non-merge)
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;                                // free previous cell
        }

        this.cell = null;                                         // 'this' will be destroyed after anim
        cell.tile.locked = true;                                  // target tile merges only once this turn

        StartCoroutine(Animate(cell.transform.position, true));   // animate into target, then destroy
    }

    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.1f;                                    

        Vector3 from = transform.position;

        
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;                                    // wait one frame
        }

        transform.position = to;                                  // ensure exact final position

        if (merging) {
            Destroy(gameObject);                                  // merged tile is removed 
        }
    }

}
