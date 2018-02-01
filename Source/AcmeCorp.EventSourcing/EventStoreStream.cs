namespace AcmeCorp.EventSourcing
{
    using System.Collections.Generic;

    public class EventStoreStream : List<EventStoreMessage>, IEventStoreStream
    {
        public EventStoreStream(int revision)
        {
            this.Revision = revision;
        }

        public int Revision { get; private set; }
    }
}
