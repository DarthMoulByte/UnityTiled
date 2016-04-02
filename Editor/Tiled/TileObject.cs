using System.Xml.Linq;

namespace Tiled
{
    public class TileObject : Object
    {
        public uint Gid { get; private set; }

        internal TileObject(XElement element)
            : base(element)
        {
            Gid = element.Attribute("gid").UIntValue();
        }
    }
}
