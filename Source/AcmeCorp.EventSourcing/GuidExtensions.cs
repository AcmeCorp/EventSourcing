namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Globalization;

    public static class GuidExtensions
    {
        public static string ToEventStreamIdFormattedString(this Guid value)
        {
            return value.ToString("D", CultureInfo.InvariantCulture);
        }
    }
}
