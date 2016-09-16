using UnityEngine;
using UnityEditor;
using System;

//Custom editor class for TileMap component
[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{

    public TileMap map;
    private TileBrush brush;
    private Vector3 mouseHitPos;

    //Retruns whether the mouse is on the tile grid bounds
    bool MouseOnMap
    {
        get
        {
            return mouseHitPos.x > 0 && mouseHitPos.x < map.UnityGridSize.x && mouseHitPos.y > 0 && mouseHitPos.y < map.UnityGridSize.y;
        }
    }

    //Display custom GUI
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        Vector2 oldSize = map.GridSize;
        map.GridSize = EditorGUILayout.Vector2Field("Grid Size:", map.GridSize);

        Texture2D oldTexture = map.SourceTexture;
        map.SourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture:", map.SourceTexture, typeof(Texture2D), false);

        if (oldTexture != map.SourceTexture)
        {
            UpdateCalculation();
            map.TileID = 1;
            DestroyBrush();
            if (map.SpriteReferences.Length > 0)
                CreateBrush();
        }


        if (map.SourceTexture == null)
        {
            EditorGUILayout.HelpBox("You have not selected a source texture yet.", MessageType.Warning);
        }
        else
        {
            if (map.GridSize != oldSize)
            {
                UpdateCalculation();
            }

            EditorGUILayout.LabelField("Single Tile Size:", map.SingleTileSize.x + "x" + map.SingleTileSize.y);
            EditorGUILayout.LabelField("Grid Size in Units:", map.UnityGridSize.x + "x" + map.UnityGridSize.y);
            EditorGUILayout.LabelField("Pixels to Units:", map.PixelsToUnits.ToString());

            UpdateBrush(map.CurrentTileBrush);

            EditorGUILayout.LabelField("Draw:", "Hold Shift to draw");
            EditorGUILayout.LabelField("Erase:", "Hold Alt to erase");

            if (GUILayout.Button("Clear Tiles"))
            {
                if (EditorUtility.DisplayDialog("Clear map tiles?", "Are you sure?", "Clear", "Do not clear"))
                {
                    ClearMap();
                }
            }
        }

        EditorGUILayout.EndVertical();
        EditorUtility.SetDirty(target);
    }

    //Setup component
    void OnEnable()
    {
        map = target as TileMap;
        Tools.current = Tool.View;

        GameObject tiles = GameObject.Find("Tiles");

        if (tiles == null)
        {
            GameObject go = new GameObject("Tiles");
            go.transform.SetParent(map.transform);
            go.transform.position = Vector3.zero;

            map.Tiles = go;
        }
        else
        {
            map.Tiles = tiles;
        }

        //Destroy possible remanent when reloading scene
        DestroyImmediate(GameObject.Find("Brush"));

        if (map.SourceTexture != null)
        {
            UpdateCalculation();
            if (map.SpriteReferences.Length > 0)
                NewBrush();
        }
    }

    //Destroy brush
    void OnDisable()
    {
        DestroyBrush();
    }

    //On Render
    void OnSceneGUI()
    {
        if (brush != null)
        {
            UpdateHitPosition();
            MoveBrush();
            if (map.SourceTexture != null && MouseOnMap)
            {
                Event current = Event.current;
                if (current.shift)
                {
                    Draw();
                }
                else if (current.alt)
                {
                    RemoveTile();
                }
            }
        }
    }

    //Recalculate parameters
    private void UpdateCalculation()
    {
        string path = AssetDatabase.GetAssetPath(map.SourceTexture);
        map.SpriteReferences = AssetDatabase.LoadAllAssetsAtPath(path);

        //spriteReferences[0] is a reference to the whole texture
        if (map.SpriteReferences.Length > 0)
        {
            try
            {
                Sprite sprite = (Sprite)map.SpriteReferences[1];

                float height = sprite.rect.height;
                float width = sprite.rect.width;

                map.SingleTileSize = new Vector2(width, height);
                map.PixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);

                map.UnityGridSize = new Vector2((width / map.PixelsToUnits) * map.GridSize.x, (height / map.PixelsToUnits) * map.GridSize.y);
            }
            catch (Exception e) {
                float height = 0;
                float width = 0;

                map.SingleTileSize = new Vector2(width, height);
                map.PixelsToUnits = 0;

                map.UnityGridSize = new Vector2(0, 0);
                Debug.LogWarning(e.Message);
            }
        }
    }

    //Create a new brush
    void CreateBrush()
    {
        Sprite sprite = map.CurrentTileBrush;
        if (sprite != null)
        {
            GameObject go = new GameObject("Brush");
            go.transform.SetParent(map.transform);
            brush = go.AddComponent<TileBrush>();
            brush.Renderer2D = go.AddComponent<SpriteRenderer>();

            brush.BrushSize = new Vector2(sprite.textureRect.width / map.PixelsToUnits, sprite.textureRect.height / map.PixelsToUnits);
            brush.UpdateBrush(sprite);
        }
    }

    void NewBrush()
    {
        if (brush == null)
        {
            CreateBrush();
        }
    }

    void DestroyBrush()
    {
        if (brush != null)
        {
            DestroyImmediate(brush.gameObject);
        }
    }

    public void UpdateBrush(Sprite sprite)
    {
        if (brush != null)
        {
            brush.UpdateBrush(sprite);
        }
    }

    //Transform the mouse position to the coordinates of the world
    private void UpdateHitPosition()
    {
        Plane p = new Plane(map.transform.TransformDirection(Vector3.forward), Vector3.zero);
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 hit = Vector3.zero;
        float dist = 0f;

        if (p.Raycast(ray, out dist))
        {
            hit = ray.origin + ray.direction.normalized * dist;
        }
        mouseHitPos = map.transform.InverseTransformPoint(hit);
    }

    //Move the current bursh on the Scene window
    private void MoveBrush()
    {
        float tileSize = map.SingleTileSize.x / map.PixelsToUnits;

        float x = Mathf.Floor(mouseHitPos.x / tileSize) * tileSize;
        float y = Mathf.Floor(mouseHitPos.y / tileSize) * tileSize;

        int row = (int)(x / tileSize);
        int column = (int)Mathf.Abs((y / tileSize)) - 1;

        if (!MouseOnMap)
            return;

        int id = (int)(column * map.GridSize.x) + row;

        brush.TileID = id;

        x += map.transform.position.x + tileSize / 2;
        y += map.transform.position.y + tileSize / 2;

        brush.transform.position = new Vector3(x, y, map.transform.position.z);

    }

    //Draw a single tile
    private void Draw()
    {
        string id = brush.TileID.ToString();

        float posX = brush.transform.position.x;
        float posY = brush.transform.position.y;

        GameObject tile = GameObject.Find(map.name + "/Tiles/tile_" + id);

        if (tile == null)
        {
            tile = new GameObject("tile_" + id);
            tile.transform.SetParent(map.Tiles.transform);
            tile.transform.position = new Vector3(posX, posY, 0);
            tile.AddComponent<SpriteRenderer>();
        }

        tile.GetComponent<SpriteRenderer>().sprite = brush.Renderer2D.sprite;
    }

    //Remove a single tile
    private void RemoveTile()
    {
        string id = brush.TileID.ToString();

        GameObject tile = GameObject.Find(map.name + "/Tiles/tile_" + id);

        if (tile != null)
        {
            DestroyImmediate(tile);
        }

    }

    //Destroy all the tiles placed on the map
    private void ClearMap()
    {
        while (map.Tiles.transform.childCount > 0)
        {
            int i = 0;
            Transform t = map.Tiles.transform.GetChild(i);
            DestroyImmediate(t.gameObject);
        }
    }
}
