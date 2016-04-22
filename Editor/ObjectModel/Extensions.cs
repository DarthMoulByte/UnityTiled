using System.Globalization;
using System.Xml.Linq;

namespace UnityTiled
{
    internal static class Extensions
    {
        public static bool BoolValue(this XAttribute attr, bool defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            bool value;
            if (bool.TryParse(attr.Value, out value)) {
                return value;
            }

            return int.Parse(attr.Value, CultureInfo.InvariantCulture) == 1;
        }

        public static float FloatValue(this XAttribute attr)
        {
            return FloatValue(attr, 0);
        }

        public static float FloatValue(this XAttribute attr, float defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            return float.Parse(attr.Value, CultureInfo.InvariantCulture);
        }

        public static int IntValue(this XAttribute attr)
        {
            return IntValue(attr, 0);
        }

        public static int IntValue(this XAttribute attr, int defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            return int.Parse(attr.Value, CultureInfo.InvariantCulture);
        }

        public static string StringValue(this XAttribute attr)
        {
            return StringValue(attr, string.Empty);
        }

        public static string StringValue(this XAttribute attr, string defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            return attr.Value;
        }

        public static uint UIntValue(this XAttribute attr)
        {
            return UIntValue(attr, 0);
        }

        public static uint UIntValue(this XAttribute attr, uint defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            return uint.Parse(attr.Value, CultureInfo.InvariantCulture);
        }
    }
}
