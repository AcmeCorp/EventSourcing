namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;

    public class TestAggregate : AggregateWithSnapshot<TestAggregateSnapshot>,
        ITestAggregate,
        IHandleEvent<TestMessageA>,
        IHandleEvent<ITestMessageC>
    {
        private long? maxSnapshotsInStream;

        public TestAggregate(string eventStreamId, int numberOfEventsBetweenSnapshots = 25)
            : base(eventStreamId, numberOfEventsBetweenSnapshots)
        {
        }

        public TestMessageA LastTestMessageA { get; private set; }

        public bool TestMessageCWasHandled { get; private set; }

        public void BusinessLogicThatResultsInEventA(string value)
        {
            this.BusinessLogicThatResultsInEventA(value, Guid.NewGuid());
        }

        public void BusinessLogicThatResultsInEventA(string value, Guid desiredEventId)
        {
            this.Apply(new TestMessageA { Stuff = value }, desiredEventId);
        }

        public void BusinessLogicThatResultsInEventC()
        {
            this.Apply(new TestMessageC());
        }

        public void SetMaxSnapshotsInStream(long? maxCount)
        {
            this.maxSnapshotsInStream = maxCount;
        }

        public void Handle(TestMessageA message)
        {
            this.LastTestMessageA = message;
        }

        public void Handle(ITestMessageC message)
        {
            this.TestMessageCWasHandled = true;
        }

        public override void LoadFromSnapshot(TestAggregateSnapshot snapshot)
        {
            this.LastTestMessageA = snapshot.TestMessageA;
        }

        public override TestAggregateSnapshot TakeSnapshot()
        {
            TestAggregateSnapshot aggregateSnapshot = new TestAggregateSnapshot(this.LastTestMessageA);
            return aggregateSnapshot;
        }
    }
}
