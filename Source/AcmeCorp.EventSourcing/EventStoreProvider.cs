namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Configuration;

    public abstract class EventStoreProvider : IEventStoreProvider
    {
        private readonly Dictionary<string, List<EventStoreMessage>> uncommittedStreamEventMessages = new Dictionary<string, List<EventStoreMessage>>();

        public abstract Task<bool> StreamExistsAsync(string eventStreamId);

        public void AppendEvents(string eventStreamId, params EventStoreMessage[] eventStoreMessages)
        {
            if (eventStoreMessages == null)
            {
                throw new ArgumentNullException(nameof(eventStoreMessages));
            }

            if (eventStoreMessages.Length > 0)
            {
                if (!this.uncommittedStreamEventMessages.ContainsKey(eventStreamId))
                {
                    this.uncommittedStreamEventMessages.Add(eventStreamId, new List<EventStoreMessage>());
                }

                this.uncommittedStreamEventMessages[eventStreamId].AddRange(eventStoreMessages);
            }
        }

        public void AppendEvents(string eventStreamId, params object[] messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            EventStoreMessage[] eventStoreMessages = new EventStoreMessage[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                eventStoreMessages[i] = new EventStoreMessage(Guid.NewGuid(), messages[i]);
            }

            this.AppendEvents(eventStreamId, eventStoreMessages);
        }

        public async Task<int> CommitEventsAsync(string eventStreamId, int expectedStreamRevision)
        {
            if (!this.uncommittedStreamEventMessages.ContainsKey(eventStreamId) || this.uncommittedStreamEventMessages[eventStreamId].Count < 1)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "There are no uncommitted events for stream ID '{0}'.", eventStreamId);
                throw new EventSourcingException(exceptionMessage);
            }

            int committedEvents = await this.CommitEventsAsync(eventStreamId, expectedStreamRevision, this.uncommittedStreamEventMessages[eventStreamId]).ConfigureAwait(false);
            this.uncommittedStreamEventMessages[eventStreamId].Clear();
            return committedEvents;
        }

        public async Task<IEventStoreStream> ReadEventsAsync(string eventStreamId)
        {
            return await this.ReadEventsAsync(eventStreamId, ExpectedStreamRevision.End).ConfigureAwait(false);
        }

        public abstract Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int maximumRevision);

        public abstract Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int fromRevision, int toRevision);

        public abstract Task AddSnapshotAsync(string eventStreamId, string snapshotStreamId, EventStoreSnapshot eventStoreSnapshot);

        public abstract Task<bool> CheckSnapshotExistsAsync<T>(string snapshotStreamId)
            where T : class;

        public abstract Task<EventStoreSnapshot> ReadSnapshotAsync(string snapshotStreamId);

        protected abstract Task<int> CommitEventsAsync(string eventStreamId, int expectedStreamRevision, IEnumerable<EventStoreMessage> eventStoreMessages);
    }
}
