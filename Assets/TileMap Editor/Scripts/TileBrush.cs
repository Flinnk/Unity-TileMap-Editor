using UnityEngine;

//Class which holds the information of the brush you will draw with
public class TileBrush : MonoBehaviour {

    //Size of the brush in unity units
	public Vector2 BrushSize { get; set; }
    //Reference to the id of the current draw tile
	public int TileID { get; set; }
    //Renderer of the current sprite to draw
	public SpriteRenderer Renderer2D { get; set; }

    //Initilization
    TileBrush()    {
        BrushSize = Vector2.zero;
        TileID = 0;
    }

    //Scene window visualizer
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, BrushSize);
	}

    //Change to a new sprite to draw
	public void UpdateBrush(Sprite sprite){
		Renderer2D.sprite = sprite;
	}
}
