using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Tiled
{
    public class TileSet
    {
        private readonly Dictionary<uint, PropertyCollection> tileProperties =
            new Dictionary<uint, PropertyCollection>();

        public uint FirstGid { get; private set; }
        public int Height { get; private set; }
        public int Margin { get; private set; }

        public string Name { get; private set; }
        public string Source { get; private set; }
        public int Spacing { get; private set; }
        public int TileCount { get; private set; }
        public int TileHeight { get; private set; }

        public int TilesX { get; private set; }
        public int TilesY { get; private set; }
        public int TileWidth { get; private set; }

        public string TsxFile { get; private set; }
        public int Width { get; private set; }

        internal TileSet(XElement element, string rootDir)
        {
            FirstGid = element.Attribute("firstgid").UIntValue();

            var tileSetElem = element;

            if (element.Attribute("source") != null) {
                TsxFile = Path.Combine(rootDir, element.Attribute("source").Value);
                rootDir = Path.GetDirectoryName(TsxFile);
                var tsxDoc = XDocument.Load(File.OpenRead(TsxFile));
                tileSetElem = tsxDoc.Root;
            }

            var imageElem = tileSetElem.Element("image");
            Name = tileSetElem.Attribute("name").StringValue();
            TileWidth = tileSetElem.Attribute("tilewidth").IntValue();
            TileHeight = tileSetElem.Attribute("tileheight").IntValue();
            Spacing = tileSetElem.Attribute("spacing").IntValue();
            Margin = tileSetElem.Attribute("margin").IntValue();

            Source = Path.Combine(rootDir, imageElem.Attribute("source").StringValue());
            Width = imageElem.Attribute("width").IntValue();
            Height = imageElem.Attribute("height").IntValue();

            foreach (var t in tileSetElem.Elements("tile")) {
                tileProperties[t.Attribute("id").UIntValue()] = new PropertyCollection(t.Element("properties"));
            }

            TilesX = (Width - Margin + Spacing) / (TileWidth + Spacing);
            TilesY = (Height - Margin + Spacing) / (TileHeight + Spacing);
            TileCount = TilesX * TilesY;
        }

        public Rectangle GetTile(uint gid)
        {
            int localId = (int)(gid - FirstGid);

            int y = localId / TilesX;
            int x = localId - y * TilesX;
            return new Rectangle(Margin + Spacing + x * (TileWidth + Spacing),
                                 Margin + Spacing + y * (TileHeight + Spacing),
                                 TileWidth,
                                 TileHeight);
        }

        public PropertyCollection GetTileProperties(uint gid)
        {
            PropertyCollection c;
            if (!tileProperties.TryGetValue(gid - FirstGid, out c)) {
                c = new PropertyCollection();
            }

            return c;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Source);
        }
    }
}
