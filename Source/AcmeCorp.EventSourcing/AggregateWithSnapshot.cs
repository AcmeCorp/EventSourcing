namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Globalization;

    public abstract class AggregateWithSnapshot<T> : Aggregate, IAggregateWithSnapshot<T>
        where T : class
    {
        private readonly int numberOfEventsBetweenSnapshots;

        protected AggregateWithSnapshot(string eventStreamId, int numberOfEventsBetweenSnapshots = 25)
            : base(eventStreamId)
        {
            if (numberOfEventsBetweenSnapshots < 1)
            {
                throw new EventSourcingException("The number of events between snapshots must be greater than zero.");
            }

            this.numberOfEventsBetweenSnapshots = numberOfEventsBetweenSnapshots;
        }

        public virtual string SnapshotStreamId => string.Format(CultureInfo.InvariantCulture, "{0}-snapshot", this.EventStreamId);

        public abstract void LoadFromSnapshot(T snapshot);

        public abstract T TakeSnapshot();

        public bool IsReadyToTakeSnapshot()
        {
            return this.NumberOfEventsSinceLastSnapshot >= this.numberOfEventsBetweenSnapshots;
        }

        internal void Load(EventStoreSnapshot eventStoreSnapshot)
        {
            if (eventStoreSnapshot == null)
            {
                throw new ArgumentNullException(nameof(eventStoreSnapshot));
            }

            T body = eventStoreSnapshot.Body as T;
            if (body == null)
            {
                throw new EventSourcingException("Can't cast event type to given snapshot type.");
            }

            this.LoadFromSnapshot(body);
            this.EventStreamRevision = eventStoreSnapshot.StreamRevision;
            this.NumberOfEventsSinceLastSnapshot = 0;
        }
    }
}
