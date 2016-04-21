using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TiledUtilities
{
    public class TileSet
    {
        private readonly Dictionary<uint, PropertyCollection> tileProperties =
            new Dictionary<uint, PropertyCollection>();

        public uint firstGid { get; private set; }
        public int height { get; private set; }
        public int margin { get; private set; }

        public string name { get; private set; }
        public string source { get; private set; }
        public int spacing { get; private set; }
        public int tileHeight { get; private set; }
        public int tileWidth { get; private set; }
        public string tsxFile { get; private set; }
        public int width { get; private set; }

        internal TileSet(XElement element, string rootDir)
        {
            firstGid = element.Attribute("firstgid").UIntValue();

            var tileSetElem = element;

            if (element.Attribute("source") != null) {
                tsxFile = Path.Combine(rootDir, element.Attribute("source").Value);
                rootDir = Path.GetDirectoryName(tsxFile);
                var tsxDoc = XDocument.Load(File.OpenRead(tsxFile));
                tileSetElem = tsxDoc.Root;
            }

            var imageElem = tileSetElem.Element("image");
            name = tileSetElem.Attribute("name").StringValue();
            tileWidth = tileSetElem.Attribute("tilewidth").IntValue();
            tileHeight = tileSetElem.Attribute("tileheight").IntValue();
            spacing = tileSetElem.Attribute("spacing").IntValue();
            margin = tileSetElem.Attribute("margin").IntValue();

            source = Path.Combine(rootDir, imageElem.Attribute("source").StringValue());
            width = imageElem.Attribute("width").IntValue();
            height = imageElem.Attribute("height").IntValue();

            foreach (var t in tileSetElem.Elements("tile")) {
                tileProperties[t.Attribute("id").UIntValue()] = new PropertyCollection(t.Element("properties"));
            }
        }

        public PropertyCollection GetTileProperties(uint gid)
        {
            PropertyCollection c;
            if (!tileProperties.TryGetValue(gid - firstGid, out c)) {
                c = new PropertyCollection();
            }

            return c;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", name, source);
        }
    }
}
