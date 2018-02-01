namespace AcmeCorp.EventSourcing.UnitTests
{
    using System.Collections.Generic;

    public class TestAggregateThatIsInvalid : IAggregate
    {
        public TestAggregateThatIsInvalid(string eventStreamId)
        {
            this.EventStreamId = eventStreamId;
            this.UncommittedEvents = new List<DomainEvent>();
        }

        public string EventStreamId { get; private set; }

        public int EventStreamRevision { get; set; }

        public int NumberOfEventsSinceLastSnapshot { get; set; }

        public IList<DomainEvent> UncommittedEvents { get; }
    }
}
