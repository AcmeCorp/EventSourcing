namespace AcmeCorp.EventSourcing.Providers.FileSystem
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing;

    public class FileSystemEventStoreProvider : EventStoreProvider
    {
        public override async Task<bool> StreamExistsAsync(string eventStreamId)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int maximumRevision)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int fromRevision, int toRevision)
        {
            throw new System.NotImplementedException();
        }

        public override async Task AddSnapshotAsync(string eventStreamId, string snapshotStreamId, EventStoreSnapshot eventStoreSnapshot)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<bool> CheckSnapshotExistsAsync<T>(string snapshotStreamId)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<EventStoreSnapshot> ReadSnapshotAsync(string snapshotStreamId)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<int> CommitEventsAsync(string eventStreamId, int expectedStreamRevision, IEnumerable<EventStoreMessage> eventStoreMessages)
        {
            throw new System.NotImplementedException();
        }
    }
}
