using System;
using System.Xml.Linq;

namespace Tiled
{
    public abstract class Layer
    {
        public int height { get; private set; }
        public bool visible { get; private set; }
        public string name { get; private set; }
        public float opacity { get; private set; }
        public PropertyCollection properties { get; private set; }
        public int width { get; private set; }

        protected Layer(XElement element)
        {
            name = element.Attribute("name").StringValue();
            visible = element.Attribute("visible").BoolValue(true);
            opacity = element.Attribute("opacity").FloatValue(1.0f);
            width = element.Attribute("width").IntValue();
            height = element.Attribute("height").IntValue();
            properties = new PropertyCollection(element.Element("properties"));
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", name, GetType().Name);
        }

        internal static Layer ReadLayer(XElement element)
        {
            if (element.Name == "objectgroup") {
                return new ObjectGroup(element);
            }
            if (element.Name == "layer") {
                return new TileLayer(element);
            }
            throw new ArgumentException("element is not a valid objectgroup or layer element");
        }
    }
}
