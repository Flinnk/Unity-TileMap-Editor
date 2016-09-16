using UnityEngine;
using UnityEditor;

//Utility Editor class for creating a TileMap object from Toolbar
public class TileMapMenu {
    //Path in the toolbar
	[MenuItem("TileMap Editor/Tile Map")]
    //Create a single object and attach a TileMap component
	public static void CreateTileMap(){
		GameObject go = new GameObject ("TileMap");
		go.AddComponent<TileMap> ();
	}
}
