namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Configuration;

    public sealed class EventStoreDomainRepository : IDomainRepository
    {
        private readonly IEventStoreProvider eventStoreProvider;

        public EventStoreDomainRepository(IEventStoreProvider eventStoreProvider)
        {
            this.eventStoreProvider = eventStoreProvider;
        }

        public async Task<bool> ExistsAsync(string eventStreamId)
        {
            return await this.eventStoreProvider.StreamExistsAsync(eventStreamId).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(IAggregate aggregate)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            return await this.ExistsAsync(aggregate.EventStreamId).ConfigureAwait(false);
        }

        public async Task LoadAsync(IAggregate aggregate)
        {
            Aggregate baseAggregate = aggregate as Aggregate;
            if (baseAggregate == null)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "The supplied Aggregate did not inherit from {0}.", typeof(Aggregate).FullName);
                throw new EventSourcingException(exceptionMessage);
            }

            IEventStoreStream eventStoreStream = await this.eventStoreProvider.ReadEventsAsync(aggregate.EventStreamId).ConfigureAwait(false);
            if (eventStoreStream.Count < 1)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "No events could be found in the event store for Event Stream ID '{0}'.", aggregate.EventStreamId);
                throw new EventSourcingException(exceptionMessage);
            }

            baseAggregate.Load(eventStoreStream);
        }

        public async Task<bool> LoadIfExistsAsync(IAggregate aggregate)
        {
            Aggregate baseAggregate = aggregate as Aggregate;
            if (baseAggregate == null)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "The supplied Aggregate did not inherit from {0}.", typeof(Aggregate).FullName);
                throw new EventSourcingException(exceptionMessage);
            }

            bool streamExists = await this.eventStoreProvider.StreamExistsAsync(aggregate.EventStreamId).ConfigureAwait(false);
            if (!streamExists)
            {
                return false;
            }

            IEventStoreStream eventStoreStream = await this.eventStoreProvider.ReadEventsAsync(aggregate.EventStreamId).ConfigureAwait(false);
            baseAggregate.Load(eventStoreStream);
            return true;
        }

        /// <summary>
        /// Loads the aggregate using the latest snapshot and then applies all events
        /// in the event stream from the stream revision that the snapshot was taken.
        /// </summary>
        /// <typeparam name="T">Snapshot type</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadFromLatestSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class
        {
            AggregateWithSnapshot<T> baseAggregate = aggregate as AggregateWithSnapshot<T>;
            if (baseAggregate == null)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "The supplied Aggregate did not inherit from {0}.", typeof(Aggregate).FullName);
                throw new EventSourcingException(exceptionMessage);
            }

            // Load from the snapshot.
            EventStoreSnapshot eventStoreSnapshot = await this.eventStoreProvider.ReadSnapshotAsync(aggregate.SnapshotStreamId).ConfigureAwait(false);
            baseAggregate.Load(eventStoreSnapshot);

            // Now load all events since the snapshot was taken.
            IEventStoreStream eventStoreStream =
                await this.eventStoreProvider.ReadEventsAsync(
                    aggregate.EventStreamId,
                    eventStoreSnapshot.StreamRevision,
                    ExpectedStreamRevision.End).ConfigureAwait(false);
            baseAggregate.Load(eventStoreStream);
        }

        public async Task LoadFromLatestSnapshotIfExistsAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class
        {
            AggregateWithSnapshot<T> baseAggregate = aggregate as AggregateWithSnapshot<T>;
            if (baseAggregate == null)
            {
                string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "The supplied Aggregate did not inherit from {0}.", typeof(Aggregate).FullName);
                throw new EventSourcingException(exceptionMessage);
            }

            bool snapshotExists = await this.eventStoreProvider.CheckSnapshotExistsAsync<T>(aggregate.SnapshotStreamId).ConfigureAwait(false);
            if (snapshotExists)
            {
                await this.LoadFromLatestSnapshotAsync(aggregate).ConfigureAwait(false);
            }
            else
            {
                await this.LoadIfExistsAsync(aggregate).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(IAggregate aggregate)
        {
            await this.SaveAsync(aggregate, null, null).ConfigureAwait(false);
        }

        public async Task SaveAsync(IAggregate aggregate, string causationId, string conversationId)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            await this.SaveAsync(aggregate, aggregate.EventStreamRevision, causationId, conversationId).ConfigureAwait(false);
        }

        public async Task SaveWithoutConcurrencyCheckAsync(IAggregate aggregate)
        {
            await this.SaveWithoutConcurrencyCheckAsync(aggregate, null, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Use with caution.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="causationId">The causation ID.</param>
        /// <param name="conversationId"> The conversation ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveWithoutConcurrencyCheckAsync(IAggregate aggregate, string causationId, string conversationId)
        {
            await this.SaveAsync(aggregate, ExpectedStreamRevision.Any, causationId, conversationId).ConfigureAwait(false);
        }

        public async Task SaveSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class
        {
            await this.SaveSnapshotAsync(aggregate, Guid.NewGuid()).ConfigureAwait(false);
        }

        public async Task SaveSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate, Guid snapshotId)
            where T : class
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            T snapshot = aggregate.TakeSnapshot();
            EventStoreSnapshot eventStoreSnapshot = new EventStoreSnapshot(snapshotId, aggregate.EventStreamRevision, snapshot);
            await this.eventStoreProvider.AddSnapshotAsync(aggregate.EventStreamId, aggregate.SnapshotStreamId, eventStoreSnapshot).ConfigureAwait(false);
            aggregate.NumberOfEventsSinceLastSnapshot = 0;
        }

        private async Task SaveAsync(IAggregate aggregate, int expectedStreamRevision, string causationId, string conversationId)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            foreach (DomainEvent uncommittedEvent in aggregate.UncommittedEvents)
            {
                EventStoreMessage eventStoreMessage = new EventStoreMessage(uncommittedEvent.EventId, uncommittedEvent.Body);
                eventStoreMessage.Headers.Add(EventStoreMessageHeaderKey.EventStreamId, aggregate.EventStreamId);
                if (!string.IsNullOrWhiteSpace(causationId))
                {
                    eventStoreMessage.Headers.Add(EventStoreMessageHeaderKey.CausationId, causationId);
                }

                if (!string.IsNullOrWhiteSpace(conversationId))
                {
                    eventStoreMessage.Headers.Add(EventStoreMessageHeaderKey.ConversationId, conversationId);
                }

                this.eventStoreProvider.AppendEvents(aggregate.EventStreamId, eventStoreMessage);
            }

            int committedEvents = await this.eventStoreProvider.CommitEventsAsync(aggregate.EventStreamId, expectedStreamRevision).ConfigureAwait(false);
            aggregate.EventStreamRevision += committedEvents;
            aggregate.UncommittedEvents.Clear();
        }
    }
}
