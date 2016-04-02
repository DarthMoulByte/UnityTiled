using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tiled
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }
    }
}
