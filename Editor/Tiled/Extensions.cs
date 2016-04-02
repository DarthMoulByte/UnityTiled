using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Tiled
{
    internal static class Extensions
    {
        public static bool BoolValue(this XAttribute attr)
        {
            return BoolValue(attr, false);
        }

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

        public static T EnumValue<T>(this XAttribute attr)
        {
            return EnumValue(attr, default(T));
        }

        public static T EnumValue<T>(this XAttribute attr, T defaultValue)
        {
            if (attr == null) {
                return defaultValue;
            }

            return (T)Enum.Parse(typeof(T), attr.Value, true);
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

        public static XAttribute GetProperty(this XElement elem, string propertyName)
        {
            var properties = elem.Element("properties");
            if (properties == null) {
                return null;
            }

            foreach (var property in properties.Elements("property")) {
                if (property.Attribute("name").Value == propertyName) {
                    return property.Attribute("value");
                }
            }

            return null;
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

        public static void SetProperty(this XElement elem, string propertyName, string propertyValue)
        {
            var properties = elem.Element("properties");
            if (properties == null) {
                properties = new XElement("properties");
                elem.Add(properties);
            }

            foreach (var property in properties.Elements("property")) {
                if (property.Attribute("name").Value == propertyName) {
                    property.Attribute("value").Value = propertyValue;
                    return;
                }
            }

            var newProperty = new XElement("property");
            newProperty.Add(new XAttribute("name", propertyName));
            newProperty.Add(new XAttribute("value", propertyValue));
            properties.Add(newProperty);
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

        public static string ToCodePropertyName(this string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return name;
            }

            var builder = new StringBuilder();
            bool capitalizeNextChar = false;
            for (int i = 0; i < name.Length; i++) {
                char c = name[i];
                if (c == '_') {
                    capitalizeNextChar = true;
                }
                else {
                    if (capitalizeNextChar) {
                        builder.Append(c.ToString().ToUpperInvariant());
                        capitalizeNextChar = false;
                    }
                    else {
                        builder.Append(c);
                    }
                }
            }
            return builder.ToString();
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
