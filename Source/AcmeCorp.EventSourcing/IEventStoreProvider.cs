namespace AcmeCorp.EventSourcing
{
    using System.Threading.Tasks;

    public interface IEventStoreProvider
    {
        Task<bool> StreamExistsAsync(string eventStreamId);

        void AppendEvents(string eventStreamId, params EventStoreMessage[] eventStoreMessages);

        void AppendEvents(string eventStreamId, params object[] messages);

        Task<int> CommitEventsAsync(string eventStreamId, int expectedStreamRevision);

        Task<IEventStoreStream> ReadEventsAsync(string eventStreamId);

        Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int maximumRevision);

        Task<IEventStoreStream> ReadEventsAsync(string eventStreamId, int fromRevision, int toRevision);

        Task AddSnapshotAsync(string eventStreamId, string snapshotStreamId, EventStoreSnapshot eventStoreSnapshot);

        Task<bool> CheckSnapshotExistsAsync<T>(string snapshotStreamId)
            where T : class;

        Task<EventStoreSnapshot> ReadSnapshotAsync(string snapshotStreamId);
    }
}