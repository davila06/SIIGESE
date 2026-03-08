using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi.Converters
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "dd-MM-yyyy";
        private const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            
            if (string.IsNullOrEmpty(dateString))
                return default;

            // Intentar parsear con formato completo primero
            if (DateTime.TryParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeResult))
                return dateTimeResult;

            // Intentar parsear solo fecha
            if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateResult))
                return dateResult;

            // Si no funciona, intentar parseo estándar
            if (DateTime.TryParse(dateString, out var result))
                return result;

            throw new JsonException($"Unable to parse '{dateString}' as DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Si la hora es 00:00:00, solo mostrar fecha
            if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
            {
                writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteStringValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            }
        }
    }

    public class JsonDateTimeNullableConverter : JsonConverter<DateTime?>
    {
        private const string DateFormat = "dd-MM-yyyy";
        private const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            
            if (string.IsNullOrEmpty(dateString))
                return null;

            // Intentar parsear con formato completo primero
            if (DateTime.TryParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeResult))
                return dateTimeResult;

            // Intentar parsear solo fecha
            if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateResult))
                return dateResult;

            // Si no funciona, intentar parseo estándar
            if (DateTime.TryParse(dateString, out var result))
                return result;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            // Si la hora es 00:00:00, solo mostrar fecha
            if (value.Value.Hour == 0 && value.Value.Minute == 0 && value.Value.Second == 0)
            {
                writer.WriteStringValue(value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteStringValue(value.Value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            }
        }
    }
}