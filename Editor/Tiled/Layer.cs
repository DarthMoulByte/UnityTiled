using System;
using System.Xml.Linq;

namespace Tiled
{
    public abstract class Layer
    {
        public int Height { get; private set; }
        public bool IsVisible { get; private set; }
        public string Name { get; private set; }
        public float Opacity { get; private set; }
        public PropertyCollection Properties { get; private set; }
        public int Width { get; private set; }

        protected Layer(XElement element)
        {
            Name = element.Attribute("name").StringValue();
            IsVisible = element.Attribute("visible").BoolValue(true);
            Opacity = element.Attribute("opacity").FloatValue(1.0f);
            Width = element.Attribute("width").IntValue();
            Height = element.Attribute("height").IntValue();
            Properties = new PropertyCollection(element.Element("properties"));
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, GetType().Name);
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
