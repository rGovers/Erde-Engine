using System;
using System.Linq;
using Newtonsoft.Json;

namespace Erde.Discord
{
    public class EnumConverter : JsonConverter
    {
        public override bool CanConvert (Type a_objectType)
        {
            return a_objectType.IsEnum;
        }

        string ToCamelCase (string a_string)
        {
            if (string.IsNullOrEmpty(a_string))
            {
                return null;
            }

            // This is why I hate linq no idea how with this syntax it just works
            return a_string.ToLowerInvariant().Split(new string[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1)).Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }
        string ToShakeCase (string a_string)
        {
            if (string.IsNullOrEmpty(a_string))
            {
                return string.Empty;
            }

            return string.Concat(a_string.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()).ToArray()).ToUpper();
        }

        public override object ReadJson (JsonReader a_reader, Type a_objectType, object a_existingValue, JsonSerializer a_serializer)
        {
            if (a_reader.Value == null || ((string)a_reader.Value) == null)
            {
                return null;
            }

            Type type = a_objectType;
            if (a_objectType.IsGenericType && a_objectType.GetGenericTypeDefinition() ==  typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            
            string line = ToCamelCase((string)a_reader.Value);

            return Enum.Parse(type, line, true);
        }

        public override void WriteJson (JsonWriter a_writer, object a_value, JsonSerializer a_serializer)
        {
            Type type = a_value.GetType();
            string name = Enum.GetName(type, a_value);
            a_writer.WriteValue(ToShakeCase(name));
        }
    }
}
