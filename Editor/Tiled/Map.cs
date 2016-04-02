using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace Tiled
{
    public class Map
    {
        public int Height { get; private set; }
        public ReadOnlyCollection<Layer> Layers { get; private set; }
        public Orientation Orientation { get; private set; }
        public PropertyCollection Properties { get; private set; }
        public int TileHeight { get; private set; }
        public ReadOnlyCollection<TileSet> TileSets { get; private set; }
        public int TileWidth { get; private set; }
        public Version Version { get; private set; }
        public int Width { get; private set; }

        private Map(XDocument document, string mapDir)
        {
            Orientation =
                (Orientation)Enum.Parse(typeof(Orientation), document.Root.Attribute("orientation").Value, true);
            Width = document.Root.Attribute("width").IntValue();
            Height = document.Root.Attribute("height").IntValue();
            TileWidth = document.Root.Attribute("tilewidth").IntValue();
            TileHeight = document.Root.Attribute("tileheight").IntValue();
            Properties = new PropertyCollection(document.Root.Element("properties"));

            var tilesets = new List<TileSet>();
            TileSets = new ReadOnlyCollection<TileSet>(tilesets);
            foreach (var elem in document.Root.Elements("tileset")) {
                tilesets.Add(new TileSet(elem, mapDir));
            }

            var layers = new List<Layer>();
            Layers = new ReadOnlyCollection<Layer>(layers);
            foreach (var elem in document.Root.Elements()) {
                if (elem.Name == "objectgroup" || elem.Name == "layer") {
                    layers.Add(Layer.ReadLayer(elem));
                }
            }
        }

        public static Map FromFile(string file)
        {
            var doc = XDocument.Load(File.OpenRead(file));
            return new Map(doc, Path.GetDirectoryName(file));
        }

        public ObjectGroup GetObjectGroup(string name)
        {
            foreach (var l in Layers) {
                if (l.Name == name && l is ObjectGroup) {
                    return l as ObjectGroup;
                }
            }

            return null;
        }

        public TileLayer GetTileLayer(string name)
        {
            foreach (var l in Layers) {
                if (l.Name == name && l is TileLayer) {
                    return l as TileLayer;
                }
            }

            return null;
        }

        public TileSet GetTileSet(uint gid)
        {
            foreach (var ts in TileSets) {
                if (gid >= ts.FirstGid && gid < ts.FirstGid + ts.TileCount) {
                    return ts;
                }
            }

            return null;
        }

        public TileSet GetTileSet(string name)
        {
            foreach (var ts in TileSets) {
                if (ts.Name == name) {
                    return ts;
                }
            }

            return null;
        }
    }
}
