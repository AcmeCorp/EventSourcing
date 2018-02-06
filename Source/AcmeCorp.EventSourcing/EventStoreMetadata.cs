namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AcmeCorp.EventSourcing.Configuration;

    public static class EventStoreMetadata
    {
        public static Dictionary<string, object> CreateDefaultForMessage(object message, Guid eventId)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string fullNameAssemblyQualified = string.Format(
                CultureInfo.InvariantCulture,
                "{0}, {1}",
                message.GetType().FullName,
                message.GetType().Assembly.GetName().Name);

            Dictionary<string, object> metadata = new Dictionary<string, object>
            {
                { EventStoreMessageHeaderKey.EventId, eventId.ToEventStreamIdFormattedString() },
                { EventStoreMetadataKey.FullNameAssemblyQualifiedStrongName, message.GetType().AssemblyQualifiedName },
                { EventStoreMetadataKey.FullNameAssemblyQualified, fullNameAssemblyQualified }
            };
            return metadata;
        }

        public static Dictionary<string, object> CreateDefaultForSnapshot(object message, Guid eventId, string stream, long streamRevision)
        {
            Dictionary<string, object> metadata = CreateDefaultForMessage(message, eventId);
            metadata.Add(EventStoreMessageHeaderKey.EventStreamId, stream);
            metadata.Add(EventStoreMetadataKey.StreamRevision, streamRevision);
            return metadata;
        }
    }
}
