using UnityEngine;
using UnityEditor;
using Tiled;
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

            var map = Map.FromFile(_tmxFile);

            var tilesetSprites = new Dictionary<long, Sprite>();
            foreach (var tileset in map.TileSets)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(tileset.Source)
                                           .OfType<Sprite>()
                                           .ToArray();

                for (int i = 0; i < sprites.Length; i++)
                {
                    var sprite = sprites[i];
                    var spriteGid = int.Parse(sprite.name.Substring(sprite.name.LastIndexOf("_") + 1));
                    tilesetSprites[tileset.FirstGid + spriteGid - 1] = sprite;
                }
            }

            var layerZ = 0;
            foreach (var layer in map.Layers)
            {
                var layerObject = new GameObject(layer.Name);
                layerObject.transform.SetParent(_targetObject.transform);
                layerObject.transform.SetAsFirstSibling();
                layerObject.transform.localPosition = new Vector3(0, 0, layerZ--);

                var tileLayer = layer as TileLayer;
                if (tileLayer != null)
                {
                    for (int y = 0; y < tileLayer.Height; y++)
                    {
                        for (int x = 0; x < tileLayer.Width; x++)
                        {
                            var tile = tileLayer[x, y];
                            if (tile.Gid <= 0)
                            {
                                continue;
                            }

                            var tileObject = new GameObject(string.Format("Tile ({0},{1}) GID: {2}", x, y, tile.Gid));
                            tileObject.transform.SetParent(layerObject.transform);
                            tileObject.transform.localPosition = new Vector3(x, -y, 0);

                            var tileRenderer = tileObject.AddComponent<SpriteRenderer>();
                            tileRenderer.sprite = tilesetSprites[tile.Gid];
                        }
                    }
                }

                var objectGroup = layer as ObjectGroup;
                if (objectGroup != null)
                {
                    foreach (var obj in objectGroup.Objects)
                    {
                        if (string.IsNullOrEmpty(obj.Type))
                        {
                            // TODO: What could we do with objects that don't have types?
                            continue;
                        }

                        var prefab = FindPrefabForObject(obj.Type);
                        if (!prefab)
                        {
                            Debug.LogError(string.Format("Did not have a prefab for object '{0}' of type '{1}' on layer '{2}'", obj.Name, obj.Type, layer.Name));
                            continue;
                        }

                        var objGameObject = (GameObject) PrefabUtility.InstantiatePrefab(prefab);

                        string name = obj.Name;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = obj.Type;
                        }
                        else
                        {
                            name = string.Format("{0} ({1})", obj.Name, obj.Type);
                        }

                        objGameObject.name = name;
                        objGameObject.transform.SetParent(layerObject.transform);
                        objGameObject.transform.localPosition = new Vector3((float)obj.Position.x / map.TileWidth, (float)-obj.Position.y / map.TileHeight + 1, 0);

                    }
                }
            }
        }
    }

    private GameObject FindPrefabForObject(string type)
    {
        var guids = AssetDatabase.FindAssets(type + " t:GameObject", new[] { Path.GetDirectoryName(_tmxFile)});
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
            if (prefab)
            {
                return prefab;
            }
        }

        return null;
    }

    [MenuItem("Window/Tiled/Map Converter")]
    private static void OpenWindow()
    {
        var window = GetWindow<TiledMapConverter>();
        window.titleContent = new GUIContent("Map Converter");
        window.Show();
    }
}