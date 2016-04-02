using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tiled
{
    public class PropertyCollection : IEnumerable<Property>
    {
        private readonly Dictionary<string, Property> properties = new Dictionary<string, Property>();

        public bool IsEmpty
        {
            get { return properties.Count == 0; }
        }

        public Property this[string key]
        {
            get { return properties[key]; }
        }

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

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }

        public bool TryGetValue(string key, out Property property)
        {
            return properties.TryGetValue(key, out property);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.Values.GetEnumerator();
        }
    }
}
