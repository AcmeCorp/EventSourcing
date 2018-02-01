namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    public static class ByteArrayExtensions
    {
        public static object DeserializeFromJsonByteEncoded(this byte[] jsonByteEncoded, Type targetType)
        {
            string objectString = Encoding.UTF8.GetString(jsonByteEncoded);
            object body = JsonConvert.DeserializeObject(objectString, targetType);
            return body;
        }

        public static T DeserializeFromJsonByteEncoded<T>(this byte[] jsonByteEncoded)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(jsonByteEncoded));
        }
    }
}
