using System.Xml.Linq;

namespace Tiled
{
    public class Property
    {
        public string name { get; private set; }
        public PropertyValue value { get; private set; }

        internal Property(XElement element)
        {
            name = element.Attribute("name").Value;
            value = new PropertyValue(element.Attribute("value").Value);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", name, value.AsString());
        }
    }
}
