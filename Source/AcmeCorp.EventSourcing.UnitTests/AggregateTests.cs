namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using Xunit;

    public class AggregateTests
    {
        private const int NumberOfEventsBetweenSnapshots = 20;

        [Fact]
        public void Given_Multiple_Raised_Events_When_The_Aggregate_Is_Not_Saved_Then_StreamRevision_Should_Be_Zero()
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            ITestAggregate aggregate = new TestAggregate(eventStreamId);

            // Act
            for (int i = 0; i < 10; i++)
            {
                aggregate.BusinessLogicThatResultsInEventA(string.Empty);

                // Assert
                Assert.Equal(0, aggregate.EventStreamRevision);
            }
        }

        [Fact]
        public void Given_An_Event_And_Matching_UpdateState_Method_When_The_Event_Is_Raised_Then_The_Correct_UpdateState_Method_Is_Called_On_The_Aggregate()
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregate = new TestAggregate(eventStreamId);

            // Act
            testAggregate.BusinessLogicThatResultsInEventA("myValue");

            // Assert
            Assert.NotNull(testAggregate.LastTestMessageA);
            Assert.Equal("myValue", testAggregate.LastTestMessageA.Stuff);
        }

        [Fact]
        public void Given_An_Event_And_No_Matching_UpdateState_Method_When_The_Event_Is_Raised_Then_No_Error_Is_Raised()
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregateWithNoUpdateStateMethod testAggregateWithNoUpdateStateMethod = new TestAggregateWithNoUpdateStateMethod(eventStreamId);

            // Act
            testAggregateWithNoUpdateStateMethod.BusinessLogicThatResultsInEventA(string.Empty);

            // Assert - nothing,
        }

        [Fact]
        public void Given_An_Event_And_Matching_UpdateState_Method_When_The_Event_Is_Applied_Then_The_Uncommitted_Event_Is_The_Correct_Type()
        {
            // Arrange
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid().ToEventStreamIdFormattedString());

            // Act
            testAggregate.BusinessLogicThatResultsInEventA(string.Empty);

            // Assert
            Assert.NotNull(testAggregate.LastTestMessageA);
            Assert.Equal(1, testAggregate.UncommittedEvents.Count);
            Assert.Equal(typeof(TestMessageA), testAggregate.UncommittedEvents[0].Body.GetType());
        }

        [Fact]
        public void Given_A_Valid_Snapshot_When_The_Snapshot_Is_Loaded_By_The_Aggregate_Then_The_Aggregate_State_Is_As_Expected()
        {
            // Arrange
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid().ToEventStreamIdFormattedString());
            const string value = "the value";
            const int streamRevision = 4;
            TestAggregateSnapshot testAggregateSnapshot = new TestAggregateSnapshot(new TestMessageA { Stuff = value });
            EventStoreSnapshot eventStoreSnapshot = new EventStoreSnapshot(Guid.NewGuid(), streamRevision, testAggregateSnapshot);

            // Act
            testAggregate.Load(eventStoreSnapshot);

            // Assert
            Assert.Equal(streamRevision, testAggregate.EventStreamRevision);
            Assert.Equal(0, testAggregate.UncommittedEvents.Count);
            Assert.Equal(value, testAggregate.LastTestMessageA.Stuff);
        }

        [Fact]
        public void Given_An_Aggregate_When_An_Event_Is_Applied_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Increases_By_One()
        {
            // Arrange
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid().ToEventStreamIdFormattedString());

            // Act
            long countBefore = testAggregate.NumberOfEventsSinceLastSnapshot;
            testAggregate.BusinessLogicThatResultsInEventA("value");

            // Assert
            Assert.Equal(0, countBefore);
            Assert.Equal(1, testAggregate.NumberOfEventsSinceLastSnapshot);
        }

        [CLSCompliant(false)]
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Given_Number_Of_Events_Between_Snapshots_Less_Than_1_Then_Throws(int numberOfEventsBetweenSnapshots)
        {
            // Act
            Assert.Throws<EventSourcingException>(
                () => new TestAggregate(
                    Guid.NewGuid().ToEventStreamIdFormattedString(),
                    numberOfEventsBetweenSnapshots));
        }

        [CLSCompliant(false)]
        [Theory]
        [InlineData(NumberOfEventsBetweenSnapshots - 1)]
        public void Given_Number_Of_Events_Since_Snapshot_Below_Limit_Then_Not_Ready_To_Take_Snapshot(int numberOfEventsBetweenSnapshots)
        {
            // Arrange
            TestAggregate testAggregate = GetAggregateWithEvents(numberOfEventsBetweenSnapshots, numberOfEventsBetweenSnapshots - 1);

            // Act
            bool result = testAggregate.IsReadyToTakeSnapshot();

            // Assert
            Assert.False(result);
        }

        [CLSCompliant(false)]
        [Theory]
        [InlineData(NumberOfEventsBetweenSnapshots)]
        [InlineData(NumberOfEventsBetweenSnapshots + 1)]
        public void Given_Number_Of_Events_Since_Snapshot_Equal_Or_Above_Limit_Then_Ready_To_Take_Snapshot(
            int numberOfEventsBetweenSnapshots)
        {
            // Arrange
            TestAggregate testAggregate = GetAggregateWithEvents(numberOfEventsBetweenSnapshots, numberOfEventsBetweenSnapshots);

            // Act
            bool result = testAggregate.IsReadyToTakeSnapshot();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Given_An_Event_That_Is_An_Interface_When_The_Business_Logic_Is_Invoked_Then_The_Event_Is_Handled()
        {
            // Arrange
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid().ToEventStreamIdFormattedString(), int.MaxValue);

            // Act
            testAggregate.BusinessLogicThatResultsInEventC();

            // Assert
            Assert.True(testAggregate.TestMessageCWasHandled);
        }

        private static TestAggregate GetAggregateWithEvents(int numberOfEventsBetweenSnapshots, int numberOfEvents)
        {
            TestAggregate testAggregate = new TestAggregate(
                Guid.NewGuid().ToEventStreamIdFormattedString(),
                numberOfEventsBetweenSnapshots);
            for (int i = 0; i < numberOfEvents; i++)
            {
                testAggregate.BusinessLogicThatResultsInEventA("value");
            }

            return testAggregate;
        }
    }
}
