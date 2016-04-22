using System.Xml.Linq;

namespace UnityTiled
{
    public class TileObject : Object
    {
        public uint gid { get; private set; }

        internal TileObject(XElement element)
            : base(element)
        {
            gid = element.Attribute("gid").UIntValue();
        }
    }
}
