using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UnityTiled
{
    public class Map
    {
        public int height { get; private set; }
        public ReadOnlyCollection<Layer> layers { get; private set; }
        public Orientation orientation { get; private set; }
        public PropertyCollection properties { get; private set; }
        public int tileHeight { get; private set; }
        public ReadOnlyCollection<TileSet> tileSets { get; private set; }
        public int tileWidth { get; private set; }
        public int width { get; private set; }

        private Map(XDocument document, string mapDir)
        {
            orientation =
                (Orientation)Enum.Parse(typeof(Orientation), document.Root.Attribute("orientation").Value, true);
            width = document.Root.Attribute("width").IntValue();
            height = document.Root.Attribute("height").IntValue();
            tileWidth = document.Root.Attribute("tilewidth").IntValue();
            tileHeight = document.Root.Attribute("tileheight").IntValue();
            properties = new PropertyCollection(document.Root.Element("properties"));

            var tilesets = new List<TileSet>();
            foreach (var elem in document.Root.Elements("tileset")) {
                tilesets.Add(new TileSet(elem, mapDir));
            }
            this.tileSets = new ReadOnlyCollection<TileSet>(tilesets);

            var layers = new List<Layer>();
            foreach (var elem in document.Root.Elements()) {
                if (elem.Name == "objectgroup" || elem.Name == "layer") {
                    layers.Add(Layer.ReadLayer(elem));
                }
            }
            this.layers = new ReadOnlyCollection<Layer>(layers);
        }

        public static Map FromFile(string file)
        {
            var doc = XDocument.Load(File.OpenRead(file));
            return new Map(doc, Path.GetDirectoryName(file));
        }

        public PropertyCollection GetProperties(uint gid)
        {
            return tileSets.Select(ts => ts.GetTileProperties(gid)).FirstOrDefault(p => p != null) ?? new PropertyCollection();
        }
    }
}
