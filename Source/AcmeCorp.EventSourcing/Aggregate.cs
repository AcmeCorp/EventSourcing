namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class Aggregate : IAggregate
    {
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, MethodInfo> UpdateStateMethodCache = new ConcurrentDictionary<Tuple<Type, Type>, MethodInfo>();

        protected Aggregate(string eventStreamId)
        {
            this.EventStreamId = eventStreamId;
            this.UncommittedEvents = new List<DomainEvent>();
        }

        public string EventStreamId { get; }

        public int EventStreamRevision { get; set; }

        public int NumberOfEventsSinceLastSnapshot { get; set; }

        public IList<DomainEvent> UncommittedEvents { get; }

        internal void Load(IEnumerable<EventStoreMessage> eventStream)
        {
            if (eventStream == null)
            {
                throw new ArgumentNullException(nameof(eventStream));
            }

            foreach (EventStoreMessage eventStoreMessage in eventStream)
            {
                this.ApplyObject(eventStoreMessage.Body);
                this.NumberOfEventsSinceLastSnapshot++;
                this.EventStreamRevision++;
            }
        }

        protected void Apply(object eventMessage)
        {
            this.Apply(eventMessage, Guid.NewGuid());
        }

        protected void Apply(object eventMessage, Guid eventId)
        {
            if (eventMessage == null)
            {
                throw new ArgumentNullException(nameof(eventMessage));
            }

            if (eventId == Guid.Empty)
            {
                throw new ArgumentException("The Event ID must be a valid GUID.", nameof(eventMessage));
            }

            this.UncommittedEvents.Add(new DomainEvent(eventId, eventMessage));
            this.NumberOfEventsSinceLastSnapshot++;
            this.ApplyObject(eventMessage);
        }

        private void ApplyObject(object eventMessage)
        {
            // Key off a combination of the aggregate type and the message type (multiple aggregates may accept the same message).
            Tuple<Type, Type> updateStateMethodCacheKey = new Tuple<Type, Type>(this.GetType(), eventMessage.GetType());

            MethodInfo method;
            if (UpdateStateMethodCache.TryGetValue(updateStateMethodCacheKey, out method))
            {
                method.Invoke(this, new[] { eventMessage });
            }
            else
            {
                if (this.GetType().TryGetAggregateUpdateStateMethodForMessage(eventMessage.GetType(), out method))
                {
                    UpdateStateMethodCache.TryAdd(updateStateMethodCacheKey, method);
                    method.Invoke(this, new[] { eventMessage });
                }
            }
        }
    }
}
