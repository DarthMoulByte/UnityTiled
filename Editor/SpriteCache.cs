using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace UnityTiled
{
    public class SpriteCache
    {
        private readonly List<TilesetInfo> _tilesetInfo = new List<TilesetInfo>();

        private readonly Dictionary<uint, Sprite> _tilesetSprites = new Dictionary<uint, Sprite>();

        public SpriteCache(Map map)
        {
            foreach (var tileset in map.tileSets)
            {
                var info = new TilesetInfo();
                info.firstGid = tileset.firstGid;
                info.assetPath = TiledHelpers.GetAssetPath(tileset.source);

                var sprites = AssetDatabase.LoadAllAssetsAtPath(info.assetPath)
                                        .OfType<Sprite>()
                                        .ToArray();

                info.lastGid = (uint)(info.firstGid + sprites.Length - 1);
                _tilesetInfo.Add(info);
            }
        }

        public Sprite GetSprite(uint gid)
        {
            Sprite sprite;
            if (_tilesetSprites.TryGetValue(gid, out sprite))
                return sprite;

            foreach (var info in _tilesetInfo)
            {
                if (info.firstGid <= gid && info.lastGid >= gid)
                {
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(info.assetPath)
                                            .OfType<Sprite>()
                                            .ToArray();
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        sprite = sprites[i];
                        var spriteGid = int.Parse(sprite.name.Substring(sprite.name.LastIndexOf("_") + 1));
                        _tilesetSprites[(uint)(info.firstGid + spriteGid - 1)] = sprite;
                    }

                    break;
                }
            }

            return _tilesetSprites[gid];
        }

        private class TilesetInfo
        {
            public uint firstGid;
            public uint lastGid;
            public string assetPath;
        }
    }
}
