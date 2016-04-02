using System;
using System.Globalization;

namespace Tiled
{
    public class PropertyValue
    {
        private readonly string rawValue;

        internal PropertyValue(string value)
        {
            rawValue = value;
        }

        public bool AsBool()
        {
            return bool.Parse(rawValue);
        }

        public T AsEnum<T>()
        {
            return (T)Enum.Parse(typeof(T), rawValue, true);
        }

        public float AsFloat()
        {
            return float.Parse(rawValue, CultureInfo.InvariantCulture);
        }

        public int AsInt()
        {
            return int.Parse(rawValue, CultureInfo.InvariantCulture);
        }

        public string AsString()
        {
            return rawValue;
        }

        public uint AsUInt()
        {
            if (rawValue.Contains("#")) {
                string color = rawValue.Substring(1);
                if (color.Length != 8) {
                    // add alpha channel
                    color = "FF" + color;
                }

                return uint.Parse(color, NumberStyles.HexNumber);
            }
            return uint.Parse(rawValue, CultureInfo.InvariantCulture);
        }
    }
}
