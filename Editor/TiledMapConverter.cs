using UnityEngine;
using UnityEditor;
using TiledSharp;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TiledMapConverter : EditorWindow
{
    private GameObject _targetObject;
    private string _tmxFile;

    void OnGUI()
    {
        _targetObject = (GameObject) EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true);

        using (new EditorGUILayout.HorizontalScope())
        {
            _tmxFile = EditorGUILayout.TextField("TMX Map", _tmxFile);

            if (GUILayout.Button("..."))
            {
                var file = EditorUtility.OpenFilePanel("Choose TMX File", Application.dataPath, "tmx");
                if (!string.IsNullOrEmpty(file))
                {
                    _tmxFile = file.Replace(Application.dataPath, "");
                    if (_tmxFile.StartsWith("/") || _tmxFile.StartsWith("\\"))
                    {
                        _tmxFile = _tmxFile.Substring(1);
                    }
                    _tmxFile = Path.Combine("Assets", _tmxFile);
                }
            }
        }

        if (string.IsNullOrEmpty(_tmxFile) && !File.Exists(_tmxFile))
        {
            EditorGUILayout.HelpBox("TMX file path required.", MessageType.Error);
            GUI.enabled = false;
        }
        else
        {
            GUI.enabled = true;
        }

        if (GUILayout.Button("Convert"))
        {
            if (!_targetObject)
            {
                var mapName = Path.GetFileNameWithoutExtension(_tmxFile);
                _targetObject = new GameObject(mapName);
            }

            for (int i = _targetObject.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_targetObject.transform.GetChild(i).gameObject);
            }

            var map = new TmxMap(_tmxFile);

            var tilesetSprites = new Dictionary<int, Sprite>();
            foreach (var tileset in map.Tilesets)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(tileset.Image.Source)
                                           .OfType<Sprite>()
                                           .OrderBy(s => s.name)
                                           .ToArray();

                for (int i = 0; i < sprites.Length; i++)
                {
                    tilesetSprites[tileset.FirstGid + i] = sprites[i];
                }
            }

            var layerZ = 0;
            foreach (var layer in map.Layers)
            {
                var layerObject = new GameObject(layer.Name);
                layerObject.transform.SetParent(_targetObject.transform);
                layerObject.transform.SetAsFirstSibling();
                layerObject.transform.localPosition = new Vector3(0, 0, layerZ--);

                foreach (var tile in layer.Tiles)
                {
                    if (tile.Gid <= 0)
                    {
                        continue;
                    }

                    var tileObject = new GameObject(string.Format("Tile ({0},{1}) GID: {2}", tile.X, tile.Y, tile.Gid));
                    tileObject.transform.SetParent(layerObject.transform);
                    tileObject.transform.localPosition = new Vector3(tile.X, -tile.Y, 0);

                    var tileRenderer = tileObject.AddComponent<SpriteRenderer>();
                    tileRenderer.sprite = tilesetSprites[tile.Gid];
                }
            }
        }
    }

    [MenuItem("Window/Tiled/Map Converter")]
    private static void OpenWindow()
    {
        var window = GetWindow<TiledMapConverter>();
        window.titleContent = new GUIContent("Map Converter");
        window.Show();
    }
}