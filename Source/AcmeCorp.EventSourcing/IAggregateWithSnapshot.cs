namespace AcmeCorp.EventSourcing
{
    public interface IAggregateWithSnapshot<TSnapshot> : IAggregate
    {
        string SnapshotStreamId { get; }

        void LoadFromSnapshot(TSnapshot snapshot);

        TSnapshot TakeSnapshot();

        bool IsReadyToTakeSnapshot();
    }
}
