using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MyDotnet.Domain.Dto.System
{
    public class LongToStringConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var converter = TypeDescriptor.GetConverter(objectType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(null, null, reader.Value.ToString());
            }

            converter = TypeDescriptor.GetConverter(typeof(string));
            if (converter.CanConvertTo(objectType))
            {
                return converter.ConvertTo(null, null, reader.Value.ToString(), objectType);
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}
