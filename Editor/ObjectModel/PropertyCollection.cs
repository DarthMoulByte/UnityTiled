using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TiledUtilities
{
    public class PropertyCollection : IEnumerable<Property>
    {
        private readonly Dictionary<string, Property> properties = new Dictionary<string, Property>();

        internal PropertyCollection()
            : this(null) {}

        internal PropertyCollection(XElement element)
        {
            if (element != null) {
                foreach (var p in element.Elements("property")) {
                    properties[p.Attribute("name").Value] = new Property(p);
                }
            }
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }

        public Dictionary<string, string> GetDictionary()
        {
           return properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.value.AsString());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }
    }
}
