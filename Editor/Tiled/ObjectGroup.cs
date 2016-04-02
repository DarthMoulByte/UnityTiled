using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Tiled
{
    public class ObjectGroup : Layer
    {
        public ReadOnlyCollection<Object> objects { get; private set; }

        internal ObjectGroup(XElement element)
            : base(element)
        {
            var objects = new List<Object>();
            foreach (var o in element.Elements("object")) {
                objects.Add(Object.ReadObject(o));
            }
            this.objects = new ReadOnlyCollection<Object>(objects);
        }
    }
}
