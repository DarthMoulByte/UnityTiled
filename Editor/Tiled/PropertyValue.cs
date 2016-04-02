using System;
using System.Globalization;

namespace Tiled
{
    public class PropertyValue
    {
        private readonly string _value;

        internal PropertyValue(string value)
        {
            _value = value;
        }

        public bool AsBool()
        {
            return bool.Parse(_value);
        }

        public T AsEnum<T>()
        {
            return (T)Enum.Parse(typeof(T), _value, true);
        }

        public float AsFloat()
        {
            return float.Parse(_value, CultureInfo.InvariantCulture);
        }

        public int AsInt()
        {
            return int.Parse(_value, CultureInfo.InvariantCulture);
        }

        public string AsString()
        {
            return _value;
        }

        public uint AsUInt()
        {
            if (_value.Contains("#")) {
                string color = _value.Substring(1);
                if (color.Length != 8) {
                    // add alpha channel
                    color = "FF" + color;
                }

                return uint.Parse(color, NumberStyles.HexNumber);
            }
            return uint.Parse(_value, CultureInfo.InvariantCulture);
        }
    }
}
