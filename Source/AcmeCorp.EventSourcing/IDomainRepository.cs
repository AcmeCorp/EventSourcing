namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Threading.Tasks;

    public interface IDomainRepository
    {
        Task<bool> ExistsAsync(string eventStreamId);

        Task<bool> ExistsAsync(IAggregate aggregate);

        Task LoadAsync(IAggregate aggregate);

        Task<bool> LoadIfExistsAsync(IAggregate aggregate);

        Task LoadFromLatestSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class;

        Task LoadFromLatestSnapshotIfExistsAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class;

        Task SaveAsync(IAggregate aggregate);

        Task SaveAsync(IAggregate aggregate, string causationId, string conversationId);

        Task SaveWithoutConcurrencyCheckAsync(IAggregate aggregate);

        Task SaveWithoutConcurrencyCheckAsync(IAggregate aggregate, string causationId, string conversationId);

        Task SaveSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate)
            where T : class;

        Task SaveSnapshotAsync<T>(IAggregateWithSnapshot<T> aggregate, Guid snapshotId)
            where T : class;
    }
}
