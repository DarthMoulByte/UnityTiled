using UnityEngine;
using UnityEditor;
using Tiled;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                var file = EditorUtility.OpenFilePanel("Choose TMX File", Application.dataPath, "tmx,xml");
                if (!string.IsNullOrEmpty(file))
                    _tmxFile = GetAssetPath(file);
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
            PrepareMapRoot();

            var map = Map.FromFile(_tmxFile);
            Dictionary<long, Sprite> tilesetSprites = LoadSprites(map);

            var allCreatedObjects = new List<GameObject>();

            var layerZ = 0;
            foreach (var layer in map.layers)
            {
                var layerObject = new GameObject(layer.name);
                layerObject.transform.SetParent(_targetObject.transform);
                layerObject.transform.SetAsFirstSibling();
                layerObject.transform.localPosition = new Vector3(0, 0, layerZ--);

                var tileLayer = layer as TileLayer;
                if (tileLayer != null)
                {
                    CreateSpritesForLayer(tilesetSprites, layerObject, tileLayer);
                }

                var objectGroup = layer as ObjectGroup;
                if (objectGroup != null)
                {
                    allCreatedObjects.AddRange(CreateObjects(map, layer, layerObject, objectGroup));
                }
            }

            SendOnCreatedByTiledUtilitiesMessage(allCreatedObjects);
        }
    }

    private static string GetAssetPath(string file)
    {
        file = Path.GetFullPath(file);
        file = file.Replace(Application.dataPath, "");
        if (file.StartsWith("/") || file.StartsWith("\\"))
        {
            file = file.Substring(1);
        }
        file = Path.Combine("Assets", file);
        return file;
    }

    private void PrepareMapRoot()
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
    }

    private static Dictionary<long, Sprite> LoadSprites(Map map)
    {
        var tilesetSprites = new Dictionary<long, Sprite>();
        foreach (var tileset in map.tileSets)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(GetAssetPath(tileset.source))
                                       .OfType<Sprite>()
                                       .ToArray();

            for (int i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                var spriteGid = int.Parse(sprite.name.Substring(sprite.name.LastIndexOf("_") + 1));
                tilesetSprites[tileset.firstGid + spriteGid - 1] = sprite;
            }
        }

        return tilesetSprites;
    }

    private static void CreateSpritesForLayer(Dictionary<long, Sprite> tilesetSprites, GameObject layerObject, TileLayer tileLayer)
    {
        for (int y = 0; y < tileLayer.height; y++)
        {
            for (int x = 0; x < tileLayer.width; x++)
            {
                var tile = tileLayer.GetTile(x, y);
                if (tile.gid <= 0)
                {
                    continue;
                }

                var tileObject = new GameObject(string.Format("Tile ({0},{1}) GID: {2}", x, y, tile.gid));
                tileObject.transform.SetParent(layerObject.transform);
                tileObject.transform.localPosition = new Vector3(x, -y, 0);

                var tileRenderer = tileObject.AddComponent<SpriteRenderer>();
                tileRenderer.sprite = tilesetSprites[tile.gid];
            }
        }
    }

    private List<GameObject> CreateObjects(Map map, Layer layer, GameObject layerObject, ObjectGroup objectGroup)
    {
        var createdGameObjects = new List<GameObject>();
        foreach (var obj in objectGroup.objects)
        {
            if (string.IsNullOrEmpty(obj.type))
            {
                // TODO: What could we do with objects that don't have types?
                continue;
            }

            var prefab = FindPrefabForObject(obj.type);
            if (!prefab)
            {
                Debug.LogError(string.Format("Did not have a prefab for object '{0}' of type '{1}' on layer '{2}'", obj.name, obj.type, layer.name));
                continue;
            }

            var objGameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            createdGameObjects.Add(objGameObject);

            objGameObject.name = GetObjectName(obj);
            objGameObject.transform.SetParent(layerObject.transform);
            objGameObject.transform.localPosition = new Vector3((float)obj.x / map.tileWidth, (float)-obj.y / map.tileHeight + 1, 0);
            SetScriptProperties(obj, objGameObject);
        }
        return createdGameObjects;
    }

    private void SetScriptProperties(Tiled.Object obj, GameObject objGameObject)
    {
        foreach (var userScript in GetAllUserComponents(objGameObject))
        {
            var target = new SerializedObject(userScript);

            foreach (var prop in obj.properties)
            {
                var serializedProperty = FindSerializedPropertyForObject(target, prop.name);
                if (serializedProperty == null)
                    continue;

                switch (serializedProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        serializedProperty.intValue = prop.value.AsInt();
                        break;
                    case SerializedPropertyType.Boolean:
                        serializedProperty.boolValue = prop.value.AsBool();
                        break;
                    case SerializedPropertyType.Float:
                        serializedProperty.floatValue = prop.value.AsFloat();
                        break;
                    case SerializedPropertyType.String:
                        serializedProperty.stringValue = prop.value.AsString();
                        break;
                    case SerializedPropertyType.Color:
                        var color = prop.value.AsUInt();
                        byte a = (byte)(color >> 24);
                        byte r = (byte)(color >> 16);
                        byte g = (byte)(color >> 8);
                        byte b = (byte)(color >> 0);
                        serializedProperty.colorValue = new Color(r, g, b, a);
                        break;
                    default:
                        Debug.LogWarning("Can't handle property of type " + serializedProperty.propertyType);
                        break;
                }
            }

            target.ApplyModifiedProperties();
        }
    }

    private static string GetObjectName(Tiled.Object obj)
    {
        string name = obj.name;
        if (string.IsNullOrEmpty(name))
        {
            name = obj.type;
        }
        else
        {
            name = string.Format("{0} ({1})", obj.name, obj.type);
        }

        return name;
    }

    private static void SendOnCreatedByTiledUtilitiesMessage(List<GameObject> createdGameObjects)
    {
        foreach (var gameObject in createdGameObjects)
        {
            foreach (var userScript in GetAllUserComponents(gameObject))
            {
                // NOTE: SendMessage generates errors so reflection it is.
                var method = userScript.GetType().GetMethod("OnCreatedByTiledUtilities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                    method.Invoke(userScript, null);
            }
        }
    }

    private static List<Component> GetAllUserComponents(GameObject gameObject)
    {
        var allComponents = new List<Component>();
        allComponents.AddRange(gameObject.GetComponents(typeof(MonoBehaviour)));
        allComponents.AddRange(gameObject.GetComponentsInChildren(typeof(MonoBehaviour)));
        return allComponents;
    }

    private SerializedProperty FindSerializedPropertyForObject(SerializedObject target, string propertyName)
    {
        var serializedProperty = target.FindProperty(propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        serializedProperty = target.FindProperty("_" + propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        propertyName = propertyName.Replace(" ", "");

        serializedProperty = target.FindProperty(propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        serializedProperty = target.FindProperty("_" + propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        propertyName = char.ToLowerInvariant(propertyName[0]).ToString() + propertyName.Substring(1);

        serializedProperty = target.FindProperty(propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        serializedProperty = target.FindProperty("_" + propertyName);
        if (serializedProperty != null)
            return serializedProperty;

        return null;
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
