namespace AcmeCorp.EventSourcing.UnitTests
{
    public class TestAggregateSnapshot
    {
        public TestAggregateSnapshot(TestMessageA testMessageA)
        {
            this.TestMessageA = testMessageA;
        }

        public TestMessageA TestMessageA { get; private set; }
    }
}
