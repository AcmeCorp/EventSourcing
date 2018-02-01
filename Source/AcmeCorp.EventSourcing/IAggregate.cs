namespace AcmeCorp.EventSourcing
{
    using System.Collections.Generic;

    public interface IAggregate
    {
        string EventStreamId { get; }

        int EventStreamRevision { get; set; }

        int NumberOfEventsSinceLastSnapshot { get; set; }

        IList<DomainEvent> UncommittedEvents { get; }
    }
}