using UnityEngine;
using UnityEditor;

public class TilePickerWindow : EditorWindow {

    //Zoom scale of the texture
	public enum ZoomScale{
		x1,
		x2,
		x3,
		x4,
		x5
	}

	ZoomScale scale;
    //Current scroll position
	public Vector2 scrollPosition = Vector2.zero;
    //Current tile selected
	Vector2 currentSelection = Vector2.zero;

    //Creates a TilePickerWindow
	[MenuItem("Window/Tile Picker")]
	public static void OpenTilePickerWindow(){
		EditorWindow window = EditorWindow.GetWindow (typeof(TilePickerWindow));
		window.titleContent.text = "Tile Picker";
	}

    //Paints and handle click event
    void OnGUI() {
        //Refresh screen
        Repaint();
        if (Selection.activeObject == null) { 
            return;
        }
		TileMap selection = ((GameObject)Selection.activeObject).GetComponent<TileMap> ();
        if (selection != null)
        {
            Texture2D texture2D = selection.SourceTexture;
            if (texture2D != null)
            {
                //Draws the texture with the corresponding scale and sets the scroll
                scale = (ZoomScale)EditorGUILayout.EnumPopup("Zoom", scale);
                float newScale = ((int)scale) + 1;
                Vector2 newTextureSize = new Vector2(texture2D.width, texture2D.height) * newScale;
                Vector2 offset = new Vector2(10, 25);

                Rect viewPort = new Rect(0, 0, position.width - 5, position.height - 5);
                Rect contentSize = new Rect(0, 0, newTextureSize.x + offset.x, newTextureSize.y + offset.y);
                scrollPosition = GUI.BeginScrollView(viewPort, scrollPosition, contentSize);

                GUI.DrawTexture(new Rect(offset.x, offset.y, newTextureSize.x, newTextureSize.y), texture2D);

                //Divide the texture in tiles and sets the current cursor position visual hint
                Vector2 tile = selection.SingleTileSize * newScale;
                Vector2 grid = new Vector2(newTextureSize.x / tile.x, newTextureSize.y / tile.y);

                Vector2 selectionPosition = new Vector2(tile.x * currentSelection.x + offset.x, tile.y * currentSelection.y + offset.y);

                Texture2D boxTex = new Texture2D(1, 1);
                boxTex.SetPixel(0, 0, new Color(0, 0.5f, 1f, 0.4f));
                boxTex.Apply();

                GUIStyle style = new GUIStyle(GUI.skin.customStyles[0]);
                style.normal.background = boxTex;
                GUI.Box(new Rect(selectionPosition.x, selectionPosition.y, tile.x, tile.y), "", style);

                //Check when a new tile has been selected to update the brush
                Event cEvent = Event.current;
                Vector2 mousePosition = cEvent.mousePosition;
                if (cEvent.type == EventType.mouseDown && cEvent.button == 0)
                {
                    currentSelection.x = Mathf.Floor((mousePosition.x + scrollPosition.x) / tile.x);
                    currentSelection.y = Mathf.Floor((mousePosition.y + scrollPosition.y) / tile.y);

                    if (currentSelection.x > grid.x - 1)
                        currentSelection.x = grid.x - 1;
                    if (currentSelection.y > grid.y - 1)
                        currentSelection.y = grid.y - 1;

                    selection.TileID = (int)(currentSelection.x + (currentSelection.y * grid.x) + 1);

                }

                GUI.EndScrollView();
            }
            else {
                EditorGUILayout.HelpBox("You have not assigned a texture to the TileMap script", MessageType.Warning);
            }
        }
        else {
            EditorGUILayout.HelpBox("You have not selected a GameObject with a TileMap script", MessageType.Warning);
        }

    }

}
