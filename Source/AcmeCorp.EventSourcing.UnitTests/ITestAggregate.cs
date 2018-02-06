namespace AcmeCorp.EventSourcing.UnitTests
{
    public interface ITestAggregate : IAggregateWithSnapshot<TestAggregateSnapshot>
    {
        TestMessageA LastTestMessageA { get; }

        bool TestMessageCWasHandled { get; }

        void BusinessLogicThatResultsInEventA(string value);

        void BusinessLogicThatResultsInEventC();
    }
}
