namespace AcmeCorp.EventSourcing.Providers.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing;
    using AcmeCorp.EventSourcing.Configuration;

    public class InMemoryEventStoreProvider : EventStoreProvider
    {
        private static readonly object EventStreamsSync = new object();

        private static readonly object SnapshotStreamsSync = new object();

        private readonly IDictionary<string, IList<EventStoreMessage>> eventStreams = new Dictionary<string, IList<EventStoreMessage>>();

        private readonly IDictionary<string, IList<EventStoreSnapshot>> snapshotStreams = new Dictionary<string, IList<EventStoreSnapshot>>();

        public override async Task<bool> StreamExistsAsync(string eventStreamId)
        {
            return this.eventStreams.ContainsKey(eventStreamId);
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

                IList<EventStoreMessage> eventStoreMessages = this.eventStreams[eventStreamId];
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

                    IList<EventStoreSnapshot> stream = this.GetOrCreateSnapshotStream(snapshotStreamId);
                    stream.Add(eventStoreSnapshot);
                }
            }
        }

        public override async Task<bool> CheckSnapshotExistsAsync<T>(string snapshotStreamId)
        {
            lock (SnapshotStreamsSync)
            {
                bool streamExists = this.snapshotStreams.ContainsKey(snapshotStreamId);
                if (!streamExists)
                {
                    return false;
                }

                IList<EventStoreSnapshot> snapshots = this.snapshotStreams[snapshotStreamId];
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
                if (!this.snapshotStreams.ContainsKey(snapshotStreamId))
                {
                    throw new EventStreamNotFoundException($"No Event Stream was found for ID '{snapshotStreamId}'.");
                }

                return this.snapshotStreams[snapshotStreamId].Last();
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
                        this.eventStreams.Add(eventStreamId, new List<EventStoreMessage>());
                    }
                    else
                    {
                        throw new EventStreamNotFoundException($"No Event Stream was found for ID '{eventStreamId}' and expected revision '{expectedStreamRevision}'. Either the stream did not exist at all or the stream existed but did not match the expected revision.");
                    }
                }

                IList<EventStoreMessage> stream = this.eventStreams[eventStreamId];
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
                    count++;
                }

                return count;
            }
        }

        private IList<EventStoreMessage> GetOrCreateEventStream(string streamId)
        {
            bool streamExists = this.StreamExistsAsync(streamId).Result;
            IList<EventStoreMessage> stream;
            if (!streamExists)
            {
                stream = this.AddEventStream(streamId);
            }
            else
            {
                stream = this.eventStreams[streamId];
            }

            return stream;
        }

        private IList<EventStoreMessage> AddEventStream(string streamId)
        {
            IList<EventStoreMessage> newStream = new List<EventStoreMessage>();
            this.eventStreams.Add(streamId, newStream);
            return newStream;
        }

        private IList<EventStoreSnapshot> GetOrCreateSnapshotStream(string streamId)
        {
            bool streamExists = this.snapshotStreams.ContainsKey(streamId);
            IList<EventStoreSnapshot> stream;
            if (!streamExists)
            {
                stream = this.AddSnapshotStream(streamId);
            }
            else
            {
                stream = this.snapshotStreams[streamId];
            }

            return stream;
        }

        private IList<EventStoreSnapshot> AddSnapshotStream(string streamId)
        {
            IList<EventStoreSnapshot> newStream = new List<EventStoreSnapshot>();
            this.snapshotStreams.Add(streamId, newStream);
            return newStream;
        }
    }
}
