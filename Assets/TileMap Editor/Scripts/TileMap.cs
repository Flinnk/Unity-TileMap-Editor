using UnityEngine;

/**
 * Basic class which holds the basic information about the tile map and renders gizmo information about the grid
 **/
public class TileMap : MonoBehaviour{

    //Contains the rows and colummns of the grid
	public Vector2 GridSize { get; set; }
    //Reference to the texture which contains the tiles to draw
    public Texture2D SourceTexture { get; set; }
    //Size of a single tile in the source texture. All tiles should have the same dimensions
    public Vector2 SingleTileSize { get; set; }
    //Holds all the sprites extracted from the texture in the sprite editor
    public Object[] SpriteReferences { get; set; }
    //Size of the grid converted to unity units
    public Vector2 UnityGridSize  { get; set; }
    //GameObject which will hold all the drawn tiles. Doesn´t have any functionality
    public GameObject Tiles { get; set; }
    //Pixel to units conversion
    public int PixelsToUnits { get; set; }
    //Current tile ID
    public int TileID { get; set; }
    //Current tile Sprite to draw
    public Sprite CurrentTileBrush
    {
        get { 
            return SpriteReferences [TileID] as Sprite;
        }
    }

    //Initialization of properties
    TileMap() {
        GridSize = new Vector2(0, 0);
        SingleTileSize = new Vector2();
        UnityGridSize = new Vector2();
        PixelsToUnits = 100;
        TileID = 0;
    }

   

    //Draws Grid Gizmo information on the Scene window 
    void OnDrawGizmosSelected(){

		if (SourceTexture != null) {
            DrawGrid();
            DrawBorder();
        }
	}

    private void DrawGrid() {
        Gizmos.color = Color.gray;

        //Declare basic values
        int row = 0;
        int maxColumns = (int)GridSize.x;
        float total = GridSize.x * GridSize.y;
        //Obtein size of tile converted in unity units
        Vector3 tile = new Vector3(SingleTileSize.x / PixelsToUnits, SingleTileSize.y / PixelsToUnits);
        //Calculate the center of a tile
        Vector2 offset = new Vector2(tile.x / 2, tile.y / 2);

        //Draw each tile
        for (int i = 0; i < total; i++)
        {
            int column = i % maxColumns;

            //Calculate the position of the current tile
            float newX = (column * tile.x) + offset.x + transform.position.x;
            float newY = (row * tile.y) + offset.y + transform.position.y;

            Gizmos.DrawWireCube(new Vector2(newX, newY), tile);

            //Go to the next row when reach the last column
            if (column == maxColumns - 1)
            {
                row++;
            }
        }
    }

    private void DrawBorder() {
        Gizmos.color = Color.white;

        //Calculate the center of the whole Grid
        float centerX = transform.position.x + UnityGridSize.x / 2;
        float centerY = transform.position.y + UnityGridSize.y / 2;

        Gizmos.DrawWireCube(new Vector2(centerX, centerY), UnityGridSize);
    }

}
