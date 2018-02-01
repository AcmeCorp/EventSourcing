namespace AcmeCorp.EventSourcing
{
    using System.Collections.Generic;

    public interface IEventStoreStream : IList<EventStoreMessage>
    {
        int Revision { get; }
    }
}
