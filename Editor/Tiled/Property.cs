using System.Xml.Linq;

namespace Tiled
{
    public class Property
    {
        public string Name { get; private set; }
        public PropertyValue Value { get; private set; }

        internal Property(XElement element)
        {
            Name = element.Attribute("name").Value;
            Value = new PropertyValue(element.Attribute("value").Value);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", Name, Value.AsString());
        }
    }
}
