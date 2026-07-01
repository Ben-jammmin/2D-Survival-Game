using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public static class LevelGenerator
{
    const string LEVELS_FOLDER  = "Assets/Scenes/Levels";
    const string TILES_FOLDER   = "Assets/Tiles";

    const int MAP_W = 30;
    const int MAP_H = 20;

    // 0=floor  1=wall  2=exit trigger
    static readonly int[,] L1 =
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,1},
        {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,1},
        {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
    };

    static readonly int[,] L2 =
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
    };

    static readonly int[,] L3 =
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,1,0,0,1},
        {1,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
    };

    // -------------------------------------------------------------------------

    [MenuItem("Survival Game/Generate All Levels")]
    public static void GenerateAllLevels()
    {
        EnsureFolders();

        var groundTile = GetOrCreateTile("GroundTile");
        var wallTile   = GetOrCreateTile("WallTile");
        var props      = LoadPropSprites();

        BuildLevel("Level1_Wastes",
            L1, groundTile, wallTile,
            groundColor: new Color(0.61f, 0.52f, 0.36f),
            wallColor:   new Color(0.30f, 0.24f, 0.15f),
            lightColor:  new Color(0.95f, 0.88f, 0.68f),
            nextScene:   "Level2_Camp",
            props, seed: 42, propCount: 28);

        BuildLevel("Level2_Camp",
            L2, groundTile, wallTile,
            groundColor: new Color(0.55f, 0.47f, 0.30f),
            wallColor:   new Color(0.22f, 0.18f, 0.10f),
            lightColor:  new Color(0.88f, 0.80f, 0.60f),
            nextScene:   "Level3_Base",
            props, seed: 77, propCount: 32);

        BuildLevel("Level3_Base",
            L3, groundTile, wallTile,
            groundColor: new Color(0.45f, 0.38f, 0.24f),
            wallColor:   new Color(0.18f, 0.14f, 0.08f),
            lightColor:  new Color(0.80f, 0.72f, 0.52f),
            nextScene:   "Level1_Wastes",   // loops back
            props, seed: 99, propCount: 36);

        AddToBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Levels Ready!",
            "3 levels generated in Assets/Scenes/Levels/\n\n" +
            "  Level1_Wastes  →  Level2_Camp  →  Level3_Base  → (loop)\n\n" +
            "Steps to play:\n" +
            "1. Add a Player GameObject tagged \"Player\" with a Rigidbody2D + Collider2D\n" +
            "2. Open Level1_Wastes and press Play",
            "Let's go!");
    }

    // =========================================================================
    //  Scene builder
    // =========================================================================

    static void BuildLevel(
        string sceneName,
        int[,] map,
        Tile groundTile, Tile wallTile,
        Color groundColor, Color wallColor, Color lightColor,
        string nextScene,
        Sprite[] props, int seed, int propCount)
    {
        string path = $"{LEVELS_FOLDER}/{sceneName}.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        AddCamera();
        AddGlobalLight(lightColor);

        var grid = CreateGrid();
        var groundMap = AddTilemapLayer(grid, "Ground", sortOrder: 0);
        var wallMap   = AddTilemapLayer(grid, "Walls",  sortOrder: 2);
        wallMap.gameObject.AddComponent<TilemapCollider2D>();

        PaintMap(map, groundTile, groundColor, wallTile, wallColor, groundMap, wallMap);
        PlaceProps(map, props, seed, propCount);
        AddExitAndSpawn(map, nextScene);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[LevelGenerator] Saved {path}");
    }

    // =========================================================================
    //  Scene objects
    // =========================================================================

    static void AddCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.07f, 0.04f);
        cam.nearClipPlane    = 0.3f;
        cam.farClipPlane     = 100f;
        go.transform.position = new Vector3(0f, 0f, -10f);
        go.AddComponent<AudioListener>();
        go.AddComponent<UniversalAdditionalCameraData>();
    }

    static void AddGlobalLight(Color color)
    {
        var go    = new GameObject("Global Light 2D");
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color     = color;
        light.intensity = 1f;
    }

    static Grid CreateGrid()
    {
        var go   = new GameObject("Grid");
        go.transform.position = new Vector3(-MAP_W / 2f, -MAP_H / 2f, 0f);
        return go.AddComponent<Grid>();
    }

    static Tilemap AddTilemapLayer(Grid grid, string layerName, int sortOrder)
    {
        var go = new GameObject(layerName);
        go.transform.SetParent(grid.transform);
        go.transform.localPosition = Vector3.zero;
        var tm  = go.AddComponent<Tilemap>();
        var tmr = go.AddComponent<TilemapRenderer>();
        tmr.sortingOrder = sortOrder;
        return tm;
    }

    // =========================================================================
    //  Map painting
    // =========================================================================

    static void PaintMap(
        int[,] map,
        Tile groundTile, Color groundColor,
        Tile wallTile,   Color wallColor,
        Tilemap groundMap, Tilemap wallMap)
    {
        groundMap.color = groundColor;
        wallMap.color   = wallColor;

        for (int row = 0; row < MAP_H; row++)
        {
            for (int col = 0; col < MAP_W; col++)
            {
                int cell = map[row, col];
                int ty   = MAP_H - 1 - row;          // flip Y for Unity tilemap
                var pos  = new Vector3Int(col, ty, 0);

                groundMap.SetTile(pos, groundTile);   // always paint floor
                if (cell == 1)
                    wallMap.SetTile(pos, wallTile);
            }
        }
    }

    // =========================================================================
    //  Props
    // =========================================================================

    static void PlaceProps(int[,] map, Sprite[] props, int seed, int count)
    {
        if (props == null || props.Length == 0) return;

        var parent = new GameObject("Props");
        var rng    = new System.Random(seed);
        int placed = 0, tries = 0;

        while (placed < count && tries < 2000)
        {
            tries++;
            int col = rng.Next(2, MAP_W - 2);
            int row = rng.Next(2, MAP_H - 2);
            if (map[row, col] != 0) continue;

            var sprite = props[rng.Next(props.Length)];
            if (sprite == null) continue;

            var go = new GameObject($"Prop_{placed}");
            go.transform.SetParent(parent.transform);

            float wx = -MAP_W / 2f + col + 0.5f;
            float wy =  MAP_H / 2f - row - 0.5f;
            go.transform.position = new Vector3(wx, wy, 0f);

            var sr     = go.AddComponent<SpriteRenderer>();
            sr.sprite  = sprite;
            sr.sortingOrder = 1;

            // Scale oversized sprites down to fit within ~1 unit
            float maxSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            if (maxSize > 1.2f)
                go.transform.localScale = Vector3.one * (1.0f / maxSize);

            placed++;
        }
    }

    // =========================================================================
    //  Exit trigger + player spawn
    // =========================================================================

    static void AddExitAndSpawn(int[,] map, string nextScene)
    {
        // Find exit cell (value 2) and spawn cell (first open floor near left edge)
        Vector3 exitWorld  = Vector3.zero;
        Vector3 spawnWorld = new Vector3(-MAP_W / 2f + 2.5f, -MAP_H / 2f + 1.5f, 0f);

        for (int row = 0; row < MAP_H; row++)
        {
            for (int col = 0; col < MAP_W; col++)
            {
                if (map[row, col] == 2)
                {
                    float wx = -MAP_W / 2f + col + 0.5f;
                    float wy =  MAP_H / 2f - row - 0.5f;
                    exitWorld = new Vector3(wx, wy, 0f);
                }
            }
        }

        // Exit trigger
        var exitGO    = new GameObject("Exit");
        exitGO.transform.position = exitWorld;
        var col2d     = exitGO.AddComponent<BoxCollider2D>();
        col2d.isTrigger = true;
        col2d.size      = Vector2.one;

        var transition = exitGO.AddComponent<SceneTransition>();
        transition.nextSceneName = nextScene;

        // Visual marker (green tint) so you can see it in editor
        var sr           = exitGO.AddComponent<SpriteRenderer>();
        sr.color         = new Color(0.2f, 1f, 0.3f, 0.45f);
        sr.sortingOrder  = 5;

        // Player spawn point
        var spawnGO = new GameObject("Player Spawn");
        spawnGO.transform.position = spawnWorld;
    }

    // =========================================================================
    //  Tile assets
    // =========================================================================

    static Tile GetOrCreateTile(string tileName)
    {
        string tilePath = $"{TILES_FOLDER}/{tileName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null) return existing;

        string pngName = $"{tileName}_src.png";
        string pngPath = $"{TILES_FOLDER}/{pngName}";

        if (!File.Exists(pngPath))
        {
            // 4-pixel white square
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(pngPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.filterMode          = FilterMode.Point;
            importer.spritePixelsPerUnit = 4f;   // 4px = 1 unit
            importer.wrapMode            = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        var tile   = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color  = Color.white;
        AssetDatabase.CreateAsset(tile, tilePath);
        return tile;
    }

    // =========================================================================
    //  Sprite loading
    // =========================================================================

    static Sprite[] LoadPropSprites()
    {
        var sprites = new List<Sprite>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.Contains("tile-B") && !assetPath.Contains("Auto-tile"))
                continue;

            var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var obj in all)
            {
                if (obj is Sprite s)
                    sprites.Add(s);
            }
        }

        Debug.Log($"[LevelGenerator] Loaded {sprites.Count} prop sprites.");
        return sprites.ToArray();
    }

    // =========================================================================
    //  Build settings
    // =========================================================================

    static void AddToBuildSettings()
    {
        string[] sceneNames = { "Level1_Wastes", "Level2_Camp", "Level3_Base" };
        var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (var name in sceneNames)
        {
            string path = $"{LEVELS_FOLDER}/{name}.unity";
            bool already = current.Exists(s => s.path == path);
            if (!already)
                current.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = current.ToArray();
    }

    // =========================================================================
    //  Utilities
    // =========================================================================

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(LEVELS_FOLDER))
            AssetDatabase.CreateFolder("Assets/Scenes", "Levels");
        if (!AssetDatabase.IsValidFolder(TILES_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Tiles");
    }
}
