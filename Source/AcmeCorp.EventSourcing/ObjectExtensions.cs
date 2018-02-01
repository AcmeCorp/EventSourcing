namespace AcmeCorp.EventSourcing
{
    using System.Text;
    using Newtonsoft.Json;

    public static class ObjectExtensions
    {
        private static readonly DecimalJsonConverter DecimalJsonConverter = new DecimalJsonConverter();

        public static byte[] SerializeToJsonByteEncoded(this object value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, DecimalJsonConverter));
        }
    }
}