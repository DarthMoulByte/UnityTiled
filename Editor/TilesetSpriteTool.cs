using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityTiled
{
    public class TilesetSpriteTool : EditorWindow
    {
        private Texture2D _tilesetTexture;
        private int _margin;
        private int _padding;
        private int _tileWidth = 32;
        private int _tileHeight = 32;

        void OnGUI()
        {
            _tilesetTexture = (Texture2D) EditorGUILayout.ObjectField("Tileset", _tilesetTexture, typeof(Texture2D), false);
            _tileWidth = EditorGUILayout.IntField("Tile Width", _tileWidth);
            _tileHeight = EditorGUILayout.IntField("Tile Height", _tileHeight);
            _margin = EditorGUILayout.IntField("Margin", _margin);
            _padding = EditorGUILayout.IntField("Padding", _padding);

            if (GUILayout.Button("Cut Sprites"))
            {
                var path = AssetDatabase.GetAssetPath(_tilesetTexture);
                var importer = (TextureImporter) AssetImporter.GetAtPath(path);

                importer.spriteImportMode = SpriteImportMode.Multiple;

                var spritesheet = new List<SpriteMetaData>();

                var gid = 0;
                for (int y = _margin; y <= _tilesetTexture.height - _margin - _tileHeight; y += _tileHeight + _padding)
                {
                    for (int x = _margin; x <= _tilesetTexture.width - _margin - _tileWidth; x += _tileWidth + _padding)
                    {
                        var metadata = new SpriteMetaData();
                        metadata.alignment = (int)SpriteAlignment.TopLeft;
                        metadata.name = string.Format("{0}_{1}", _tilesetTexture.name, gid + 1);
                        metadata.rect = new Rect(
                            x,
                            _tilesetTexture.height - y - _tileHeight,
                            _tileWidth,
                            _tileHeight
                        );
                        spritesheet.Add(metadata);

                        gid++;
                    }
                }

                importer.spritesheet = spritesheet.ToArray();

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Window/Tiled/Tileset Tool")]
        private static void OpenWindow()
        {
            var window = GetWindow<TilesetSpriteTool>();
            window.titleContent = new GUIContent("Tileset Tool");
            window.Show();
        }
    }
}