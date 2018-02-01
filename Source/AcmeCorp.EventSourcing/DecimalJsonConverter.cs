namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public class DecimalJsonConverter : JsonConverter
    {
        private const string TwoDecimalsString = ".00";

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(IsWholeValue(value)
                ? JsonConvert.ToString(Convert.ToInt64(value, CultureInfo.InvariantCulture)) + TwoDecimalsString
                : JsonConvert.ToString(value));
        }

        private static bool IsWholeValue(object value)
        {
            if (value is decimal)
            {
                decimal decimalValue = (decimal)value;
                int precision = (decimal.GetBits(decimalValue)[3] >> 16) & 0x000000FF;
                return precision == 0;
            }

            return false;
        }
    }
}