namespace AcmeCorp.EventSourcing.UnitTests
{
    public class TestAggregateWithNoUpdateStateMethod : Aggregate
    {
        public TestAggregateWithNoUpdateStateMethod(string eventStreamId)
            : base(eventStreamId)
        {
        }

        public void BusinessLogicThatResultsInEventA(string value)
        {
            this.Apply(new TestMessageA { Stuff = value });
        }
    }
}
