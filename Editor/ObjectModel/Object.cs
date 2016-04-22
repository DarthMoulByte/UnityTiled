using System.Xml.Linq;

namespace UnityTiled
{
    public abstract class Object
    {
        public bool visible { get; private set; }
        public string name { get; private set; }
        public float x { get; private set; }
        public float y { get; private set; }
        public PropertyCollection properties { get; private set; }
        public float rotation { get; private set; }
        public float width { get; private set; }
        public float height { get; private set; }
        public string type { get; private set; }

        protected Object(XElement element)
        {
            name = element.Attribute("name").StringValue();
            type = element.Attribute("type").StringValue();
            visible = element.Attribute("visible").BoolValue(true);
            x = element.Attribute("x").FloatValue();
            y = element.Attribute("y").FloatValue();
            width = element.Attribute("width").FloatValue();
            height = element.Attribute("height").FloatValue();
            rotation = element.Attribute("rotation").FloatValue();
            properties = new PropertyCollection(element.Element("properties"));
        }

        public override string ToString()
        {
            string str = base.ToString();

            if (!string.IsNullOrEmpty(type)) {
                str = string.Format("({0}) [{1}]", type, str);
            }

            if (!string.IsNullOrEmpty(name)) {
                str = name + " " + str;
            }

            return str;
        }

        internal static Object ReadObject(XElement element)
        {
            var gid = element.Attribute("gid");
            var ellipse = element.Element("ellipse");
            var polygon = element.Element("polygon");
            var polyline = element.Element("polyline");

            if (gid != null) {
                return new TileObject(element);
            }
            if (ellipse != null) {
                return new EllipseObject(element);
            }
            if (polygon != null) {
                return new PolygonObject(element);
            }
            if (polyline != null) {
                return new PolyLineObject(element);
            }
            return new RectangleObject(element);
        }
    }
}
