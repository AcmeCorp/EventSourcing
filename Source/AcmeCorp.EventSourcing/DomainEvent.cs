namespace AcmeCorp.EventSourcing
{
    using System;

    public class DomainEvent
    {
        public DomainEvent(Guid eventId, object body)
        {
            this.EventId = eventId;
            this.Body = body;
        }

        public Guid EventId { get; private set; }

        public object Body { get; private set; }
    }
}
