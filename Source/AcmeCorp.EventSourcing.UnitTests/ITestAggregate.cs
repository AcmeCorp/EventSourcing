namespace AcmeCorp.EventSourcing.UnitTests
{
    public interface ITestAggregate : IAggregateWithSnapshot<TestAggregateSnapshot>
    {
        TestMessageA LastTestMessageA { get; }

        void BusinessLogicThatResultsInEventA(string value);
    }
}
