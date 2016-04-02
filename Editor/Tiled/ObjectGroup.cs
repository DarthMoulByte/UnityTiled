using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Tiled
{
    public class ObjectGroup : Layer, IEnumerable<Object>
    {
        public ReadOnlyCollection<Object> Objects { get; private set; }

        internal ObjectGroup(XElement element)
            : base(element)
        {
            var objects = new List<Object>();
            Objects = new ReadOnlyCollection<Object>(objects);
            foreach (var o in element.Elements("object")) {
                objects.Add(Object.ReadObject(o));
            }
        }

        public IEnumerator<Object> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Objects.GetEnumerator();
        }
    }
}
