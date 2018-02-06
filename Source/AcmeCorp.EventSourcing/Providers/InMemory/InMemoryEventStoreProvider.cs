namespace AcmeCorp.EventSourcing.Providers.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing;
    using AcmeCorp.EventSourcing.Configuration;
    using AcmeCorp.EventSourcing.Logging;

    public class InMemoryEventStoreProvider : EventStoreProvider
    {
        private static readonly object EventStreamsSync = new object();

        private static readonly object SnapshotStreamsSync = new object();

        private static readonly IDictionary<string, IList<EventStoreMessage>> EventStreams = new Dictionary<string, IList<EventStoreMessage>>();

        private static readonly IDictionary<string, IList<EventStoreSnapshot>> SnapshotStreams = new Dictionary<string, IList<EventStoreSnapshot>>();

        private readonly ILogger logger;

        public InMemoryEventStoreProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> StreamExistsAsync(string eventStreamId)
        {
            return EventStreams.ContainsKey(eventStreamId);
        }

        public override async Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int maximumRevision)
        {
            return await this.ReadEventsAsync(eventStreamId, 0, maximumRevision).ConfigureAwait(false);
        }

        public override async Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int fromRevision, int toRevision)
        {
            lock (EventStreamsSync)
            {
                bool streamExists = this.StreamExistsAsync(eventStreamId).Result;
                if (!streamExists)
                {
                    throw new EventStreamNotFoundException($"No Event Stream was found for ID '{eventStreamId}' and expected revision '{toRevision}'. Either the stream did not exist at all or the stream existed but did not match the expected revision.");
                }

                IList<EventStoreMessage> eventStoreMessages = EventStreams[eventStreamId];
                if (toRevision == ExpectedStreamRevision.End)
                {
                    toRevision = eventStoreMessages.Count;
                }

                if (toRevision < 0)
                {
                    throw new ArgumentException(
                        $"The value must specify the revision of the stream or the end of the stream ('{ExpectedStreamRevision.End}').",
                        nameof(toRevision));
                }

                IEventStoreStream eventStoreStream = new EventStoreStream(toRevision);
                for (int i = fromRevision; i < toRevision; i++)
                {
                    eventStoreStream.Add(eventStoreMessages[i]);
                    this.logger.Info($"Read message of type '{eventStoreMessages[i].Body.GetType().Name}' from stream '{eventStreamId}'.");
                }

                return eventStoreStream;
            }
        }

        public override async Task AddSnapshotAsync(string eventStreamId, string snapshotStreamId, EventStoreSnapshot eventStoreSnapshot)
        {
            if (eventStoreSnapshot == null)
            {
                throw new ArgumentNullException(nameof(eventStoreSnapshot));
            }

            lock (SnapshotStreamsSync)
            {
                lock (EventStreamsSync)
                {
                    bool eventStreamExists = this.StreamExistsAsync(eventStreamId).Result;
                    if (!eventStreamExists)
                    {
                        throw new EventStreamNotFoundException($"No Event Stream was found for ID '{eventStreamId}'.");
                    }

                    Dictionary<string, object> metadata = EventStoreMetadata.CreateDefaultForSnapshot(eventStoreSnapshot.Body, eventStoreSnapshot.SnapshotId, eventStreamId, eventStoreSnapshot.StreamRevision);
                    foreach (KeyValuePair<string, object> keyValuePair in metadata)
                    {
                        if (!string.Equals(keyValuePair.Key, EventStoreMessageHeaderKey.EventId, StringComparison.Ordinal) &&
                            !string.Equals(keyValuePair.Key, EventStoreMetadataKey.StreamRevision, StringComparison.Ordinal))
                        {
                            eventStoreSnapshot.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }

                    IList<EventStoreSnapshot> stream = GetOrCreateSnapshotStream(snapshotStreamId);
                    stream.Add(eventStoreSnapshot);
                }
            }
        }

        public override async Task<bool> CheckSnapshotExistsAsync<T>(string snapshotStreamId)
        {
            lock (SnapshotStreamsSync)
            {
                bool streamExists = SnapshotStreams.ContainsKey(snapshotStreamId);
                if (!streamExists)
                {
                    return false;
                }

                IList<EventStoreSnapshot> snapshots = SnapshotStreams[snapshotStreamId];
                if (snapshots.Count < 1)
                {
                    return false;
                }

                // We are checking not just whether a snapshot
                // exists but also if it is of the expected type.
                EventStoreSnapshot eventStoreSnapshot = snapshots.Last();
                return eventStoreSnapshot.Body is T;
            }
        }

        public override async Task<EventStoreSnapshot> ReadSnapshotAsync(string snapshotStreamId)
        {
            lock (SnapshotStreamsSync)
            {
                if (!SnapshotStreams.ContainsKey(snapshotStreamId))
                {
                    throw new EventStreamNotFoundException($"No Event Stream was found for ID '{snapshotStreamId}'.");
                }

                return SnapshotStreams[snapshotStreamId].Last();
            }
        }

        protected override async Task<int> CommitEventsAsync(string eventStreamId, int expectedStreamRevision, IEnumerable<EventStoreMessage> eventStoreMessages)
        {
            if (eventStoreMessages == null)
            {
                throw new ArgumentNullException(nameof(eventStoreMessages));
            }

            // To Do: De-dupe on event id?
            lock (EventStreamsSync)
            {
                bool streamExists = this.StreamExistsAsync(eventStreamId).Result;
                if (!streamExists)
                {
                    if (expectedStreamRevision == ExpectedStreamRevision.New || expectedStreamRevision == ExpectedStreamRevision.Any)
                    {
                        EventStreams.Add(eventStreamId, new List<EventStoreMessage>());
                    }
                    else
                    {
                        throw new EventStreamNotFoundException($"No Event Stream was found for ID '{eventStreamId}' and expected revision '{expectedStreamRevision}'. Either the stream did not exist at all or the stream existed but did not match the expected revision.");
                    }
                }

                IList<EventStoreMessage> stream = EventStreams[eventStreamId];
                if (expectedStreamRevision != stream.Count && expectedStreamRevision != ExpectedStreamRevision.Any)
                {
                    throw new EventStreamNotFoundException($"No Event Stream was found for ID '{eventStreamId}' and expected revision '{expectedStreamRevision}'. Either the stream did not exist at all or the stream existed but did not match the expected revision.");
                }

                int count = 0;
                foreach (EventStoreMessage eventStoreMessage in eventStoreMessages)
                {
                    Dictionary<string, object> metadata = EventStoreMetadata.CreateDefaultForMessage(eventStoreMessage.Body, eventStoreMessage.EventId);
                    foreach (KeyValuePair<string, object> keyValuePair in metadata)
                    {
                        // We don't want to get the Event ID as metadata, its a property on the object.
                        if (!string.Equals(keyValuePair.Key, EventStoreMessageHeaderKey.EventId, StringComparison.Ordinal))
                        {
                            eventStoreMessage.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }

                    stream.Add(eventStoreMessage);
                    this.logger.Info($"Added message of type '{eventStoreMessage.Body.GetType().Name}' to stream '{eventStreamId}'.");
                    count++;
                }

                return count;
            }
        }

        private static IList<EventStoreSnapshot> GetOrCreateSnapshotStream(string streamId)
        {
            bool streamExists = SnapshotStreams.ContainsKey(streamId);
            IList<EventStoreSnapshot> stream;
            if (!streamExists)
            {
                stream = AddSnapshotStream(streamId);
            }
            else
            {
                stream = SnapshotStreams[streamId];
            }

            return stream;
        }

        private static IList<EventStoreSnapshot> AddSnapshotStream(string streamId)
        {
            IList<EventStoreSnapshot> newStream = new List<EventStoreSnapshot>();
            SnapshotStreams.Add(streamId, newStream);
            return newStream;
        }
    }
}
