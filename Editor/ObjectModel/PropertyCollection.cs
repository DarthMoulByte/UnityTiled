using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace UnityTiled
{
    public class PropertyCollection : IEnumerable<Property>
    {
        private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();

        internal PropertyCollection()
            : this(null) {}

        internal PropertyCollection(XElement element)
        {
            if (element != null) {
                foreach (var p in element.Elements("property")) {
                    _properties[p.Attribute("name").Value] = new Property(p);
                }
            }
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }
        
        public bool Contains(string key)
        {
            return _properties.ContainsKey(key);
        }
        
        public PropertyValue GetProperty(string key)
        {
            return _properties[key].value;
        }

        public Dictionary<string, string> GetDictionary()
        {
           return _properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.value.AsString());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }
    }
}
