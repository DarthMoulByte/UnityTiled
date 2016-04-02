using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace Tiled
{
    public class TileLayer : Layer
    {
        private readonly Tile[,] _tiles;

        internal TileLayer(XElement element)
            : base(element)
        {
            _tiles = new Tile[width, height];

            var data = element.Element("data");
            string encoding = data.Attribute("encoding").StringValue();
            if (encoding == "base64") {
                var base64data = Convert.FromBase64String(data.Value);
                Stream stream = new MemoryStream(base64data, false);

                string compression = data.Attribute("compression").StringValue();
                if (compression == "gzip") {
                    stream = new GZipStream(stream, CompressionMode.Decompress, false);
                }
                else {
                    throw new Exception("Unsupported compression.");
                }

                using (stream) {
                    using (var br = new BinaryReader(stream)) {
                        for (int y = 0; y < height; y++) {
                            for (int x = 0; x < width; x++) {
                                _tiles[x, y] = new Tile(br.ReadUInt32());
                            }
                        }
                    }
                }
            }
            else if (encoding == "csv") {
                int index = 0;
                foreach (string s in data.Value.Split(',')) {
                    uint gid = uint.Parse(s.Trim(), CultureInfo.InvariantCulture);
                    int x = index % width;
                    int y = index / width;
                    _tiles[x, y] = new Tile(gid);
                    index++;
                }
            }
            else if (encoding == null) {
                int index = 0;
                foreach (var e in data.Elements("tile")) {
                    uint gid = (uint)e.Attribute("gid");
                    int x = index % width;
                    int y = index / width;
                    _tiles[x, y] = new Tile(gid);
                    index++;
                }
            }
            else {
                throw new Exception("Unknown encoding.");
            }
        }

        public Tile GetTile(int x, int y)
        {
            return _tiles[x, y];
        }
    }
}
