namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Collections.Generic;

    public class EventStoreSnapshot
    {
        public EventStoreSnapshot(Guid snapshotId, int streamRevision, object body)
        {
            this.SnapshotId = snapshotId;
            this.Body = body;
            this.Headers = new Dictionary<string, object>();
            this.StreamRevision = streamRevision;
        }

        public Guid SnapshotId { get; private set; }

        public object Body { get; private set; }

        public Dictionary<string, object> Headers { get; private set; }

        public int StreamRevision { get; private set; }
    }
}
