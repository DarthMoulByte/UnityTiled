using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Linq;

namespace TiledUtilities
{
	public class TileProperties : MonoBehaviour 
	{
        [Serializable]
        private class Property
        {
            public string Key;
            public string Value;
        }
        
        [SerializeField] private List<Property> _properties;
		
		public void SetProperties(Dictionary<string, string> properties)
		{
			_properties = properties.Select(kvp => new Property { Key = kvp.Key, Value = kvp.Value })
                                    .ToList();
		}
		
		public bool ContainsProperty(string key)
		{
			return _properties.Any(p => p.Key == key);
		}
		
        public bool GetBool(string key)
        {
            return bool.Parse(GetString(key));
        }

        public T GetEnum<T>(string key)
        {
            return (T)Enum.Parse(typeof(T), GetString(key), true);
        }

        public float GetFloat(string key)
        {
            return float.Parse(GetString(key), CultureInfo.InvariantCulture);
        }

        public int GetInt(string key)
        {
            return int.Parse(GetString(key), CultureInfo.InvariantCulture);
        }

        public string GetString(string key)
        {
            return _properties.First(p => p.Key == key).Value;
        }

        public uint GetUInt(string key)
        {
			var value = GetString(key);
			
            if (value.Contains("#")) {
                string color = value.Substring(1);
                if (color.Length != 8) {
                    // add alpha channel
                    color = "FF" + color;
                }

                return uint.Parse(color, NumberStyles.HexNumber);
            }
            return uint.Parse(value, CultureInfo.InvariantCulture);
        }
	}
}