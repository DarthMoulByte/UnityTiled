using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace TiledUtilities
{
    [CustomEditor(typeof(TiledMap))]
    public class TiledMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var map = (TiledMap)target;

            if (GUILayout.Button("Locate TMX Map"))
            {
                var file = EditorUtility.OpenFilePanel("Choose TMX File", Application.dataPath, "tmx,xml");
                if (!string.IsNullOrEmpty(file))
                    map.tmxFile = TiledHelpers.GetAssetPath(file);
            }

            if (string.IsNullOrEmpty(map.tmxFile) && !File.Exists(map.tmxFile))
            {
                EditorGUILayout.HelpBox("TMX file path required.", MessageType.Error);
                GUI.enabled = false;
            }
            else
            {
                if (GUILayout.Button("Import"))
                {
                    for (int i = map.transform.childCount - 1; i >= 0; i--)
                        DestroyImmediate(map.transform.GetChild(i).gameObject);

                    var tmxMap = Map.FromFile(map.tmxFile);
                    var spriteCache = new SpriteCache(tmxMap);

                    var allCreatedObjects = new List<GameObject>();

                    var layerZ = 0;
                    foreach (var layer in tmxMap.layers)
                    {
                        var layerObject = new GameObject(layer.name);
                        layerObject.isStatic = true;
                        layerObject.transform.SetParent(map.transform);
                        layerObject.transform.SetAsFirstSibling();
                        layerObject.transform.localPosition = new Vector3(0, 0, layerZ--);

                        var tileLayer = layer as TileLayer;
                        if (tileLayer != null)
                            CreateSpritesForLayer(spriteCache, layerObject, tileLayer);

                        var objectGroup = layer as ObjectGroup;
                        if (objectGroup != null)
                            allCreatedObjects.AddRange(CreateObjects(tmxMap, layer, layerObject, objectGroup));
                    }

                    SendOnCreatedByTiledUtilitiesMessage(allCreatedObjects, true);

                    // When sending the message to the map, don't send it to all the children again
                    SendOnCreatedByTiledUtilitiesMessage(map.gameObject, false);
                }
            }
        }

        private static void CreateSpritesForLayer(SpriteCache spriteCache, GameObject layerObject, TileLayer tileLayer)
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
                    tileObject.isStatic = true;
                    tileObject.transform.SetParent(layerObject.transform);
                    tileObject.transform.localPosition = new Vector3(x, -y, 0);

                    var tileRenderer = tileObject.AddComponent<SpriteRenderer>();
                    tileRenderer.sprite = spriteCache.GetSprite(tile.gid);
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
                objGameObject.transform.localPosition = new Vector3(
                    obj.x / map.tileWidth,
                    -obj.y / map.tileHeight,
                    0);

                // Tiled seems to place tiled based objects with the origin at the bottom, which is
                // different than rectangle objects. So we have to scoot tile objects up one tile.
                if (obj is TileObject)
                {
                    objGameObject.transform.localPosition += new Vector3(0, 1, 0);
                }

                SetScriptProperties(obj, objGameObject);
            }
            return createdGameObjects;
        }

        private void SetScriptProperties(TiledUtilities.Object obj, GameObject objGameObject)
        {
            foreach (var userScript in GetAllUserComponents(objGameObject, true))
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

        private static string GetObjectName(TiledUtilities.Object obj)
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

        private static void SendOnCreatedByTiledUtilitiesMessage(List<GameObject> gameObjects, bool includeChildren)
        {
            foreach (var gameObject in gameObjects)
            {
                SendOnCreatedByTiledUtilitiesMessage(gameObject, includeChildren);
            }
        }

        private static void SendOnCreatedByTiledUtilitiesMessage(GameObject gameObject, bool includeChildren)
        {
            foreach (var userScript in GetAllUserComponents(gameObject, includeChildren))
            {
                // NOTE: SendMessage generates errors so reflection it is.
                var method = userScript.GetType().GetMethod("OnTmxMapImported", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                    method.Invoke(userScript, null);
            }
        }

        private static Component[] GetAllUserComponents(GameObject gameObject, bool includeChildren)
        {
            if (includeChildren)
                return gameObject.GetComponentsInChildren(typeof(MonoBehaviour));
            else
                return gameObject.GetComponents(typeof(MonoBehaviour));
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
            var guids = AssetDatabase.FindAssets(type + " t:GameObject");
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

        [MenuItem("GameObject/2D Object/Tiled Imported Map")]
        private static void Create()
        {
            var go = new GameObject("Tiled Map");
            go.isStatic = true;
            go.AddComponent<TiledMap>();
            Selection.activeGameObject = go;
        }
    }
}