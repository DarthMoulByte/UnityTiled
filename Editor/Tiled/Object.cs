using System.Xml.Linq;
using UnityEngine;

namespace Tiled
{
    public abstract class Object
    {
        public bool IsVisible { get; private set; }
        public string Name { get; private set; }
        public Vector2 Position { get; private set; }
        public PropertyCollection Properties { get; private set; }
        public float Rotation { get; private set; }
        public Vector2 Size { get; private set; }
        public string Type { get; private set; }

        protected Object(XElement element)
        {
            Name = element.Attribute("name").StringValue();
            Type = element.Attribute("type").StringValue();
            IsVisible = element.Attribute("visible").BoolValue(true);
            Position = new Vector2(element.Attribute("x").FloatValue(), element.Attribute("y").FloatValue());
            Size = new Vector2(element.Attribute("width").FloatValue(), element.Attribute("height").FloatValue());
            Rotation = element.Attribute("rotation").FloatValue();
            Properties = new PropertyCollection(element.Element("properties"));
        }

        public override string ToString()
        {
            string str = base.ToString();

            if (!string.IsNullOrEmpty(Type)) {
                str = string.Format("({0}) [{1}]", Type, str);
            }

            if (!string.IsNullOrEmpty(Name)) {
                str = Name + " " + str;
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
