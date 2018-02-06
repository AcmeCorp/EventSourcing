namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Configuration;
    using AcmeCorp.EventSourcing.Logging;
    using AcmeCorp.EventSourcing.Providers.InMemory;
    using Moq;
    using Xunit;

    public class EventStoreDomainRepositoryTests
    {
        [Fact]
        public async Task Given_An_Invalid_Aggregate_When_LoadIfExists_Is_Called_Then_An_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);

            // Act
            // Assert
            await AssertExtensions.ThrowsAsync<EventSourcingException>(
                async () => await eventStoreDomainRepository.LoadIfExistsAsync(new TestAggregateThatIsInvalid(string.Empty)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Invalid_Event_Stream_Id_When_LoadIfExists_Is_Called_Then_False_Is_Returned()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregate = new TestAggregate(eventStreamId);

            // Act
            bool aggregateLoaded = await eventStoreDomainRepository.LoadIfExistsAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.False(aggregateLoaded);
        }

        [Fact]
        public async Task Given_Events_Stored_For_The_Event_Stream_When_LoadIfExists_Is_Called_Then_True_Is_Returned_And_The_Aggregate_Can_Be_Loaded_Successfully()
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);

            eventStoreProvider.AppendEvents(eventStreamId, new EventStoreMessage(Guid.NewGuid(), new TestMessageA()));
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            TestAggregate testAggregate = new TestAggregate(eventStreamId);

            // Act
            bool aggregateLoaded = await eventStoreDomainRepository.LoadIfExistsAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.True(aggregateLoaded);
            Assert.Equal(eventStreamId, testAggregate.EventStreamId);
            Assert.Equal(0, testAggregate.UncommittedEvents.Count);
            Assert.Equal(1, testAggregate.EventStreamRevision);
            Assert.NotNull(testAggregate.LastTestMessageA);
        }

        [Fact]
        public async Task Given_An_Invalid_Event_Stream_Id_When_Load_Is_Called_Then_An_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregate = new TestAggregate(eventStreamId);

            // Act
            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreDomainRepository.LoadAsync(testAggregate).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Invalid_Event_Stream_Id_When_The_Exists_Check_Is_Called_Using_The_Event_Stream_Id_Then_The_Aggregate_Should_Report_As_Not_Existing()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            bool existsById = await eventStoreDomainRepository.ExistsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.False(existsById);
        }

        [Fact]
        public async Task Given_An_Invalid_Event_Stream_Id_When_The_Exists_Check_Is_Called_Using_The_Aggregate_Reference_Then_The_Aggregate_Should_Report_As_Not_Existing()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregate = new TestAggregate(eventStreamId);

            // Act
            bool existsByReference = await eventStoreDomainRepository.ExistsAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.False(existsByReference);
        }

        [Fact]
        public async Task Given_A_Valid_Event_Stream_Id_When_The_Exists_Check_Is_Called_Using_The_Event_Stream_Id_Then_The_Aggregate_Should_Report_As_Not_Existing()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            bool existsById = await eventStoreDomainRepository.ExistsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.True(existsById);
        }

        [Fact]
        public async Task Given_A_Valid_Event_Stream_Id_When_The_Exists_Check_Is_Called_Using_The_Aggregate_Reference_Then_The_Aggregate_Should_Report_As_Not_Existing()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            bool existsByReference = await eventStoreDomainRepository.ExistsAsync(testAggregateToSave).ConfigureAwait(false);

            // Assert
            Assert.True(existsByReference);
        }

        [Fact]
        public async Task Given_No_Uncommitted_Events_For_The_Aggregate_When_Save_Is_Called_Then_An_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            // Assert
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            await AssertExtensions.ThrowsAsync<EventSourcingException>(
                async () => await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Uncommitted_Event_For_The_Aggregate_When_Save_Is_Called_Then_The_Event_Is_Added_To_The_Event_Store()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.Single(eventStoreMessages);
        }

        [Fact]
        public async Task Given_An_Uncommitted_Event_For_The_Aggregate_When_Save_Is_Called_Then_The_Event_Is_Added_To_The_Event_Store_With_Correct_Headers()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            IEnumerable<EventStoreMessage> messages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            EventStoreMessage[] eventStoreMessages = messages.ToArray();

            // Assert
            Assert.Single(eventStoreMessages);
            Assert.Equal(3, eventStoreMessages[0].Headers.Count);
            object headerValue = eventStoreMessages[0].Headers[EventStoreMessageHeaderKey.EventStreamId];
            Assert.NotNull(headerValue);
            Assert.Equal(eventStreamId, headerValue);
        }

        [Fact]
        public async Task Given_An_Uncommitted_Event_For_The_Aggregate_When_Save_Is_Called_Then_The_Event_Is_Added_To_The_Event_Store_And_The_EventStreamRevision_And_UncommittedEvents_Are_Updated_On_The_Aggregate()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.Equal(0, testAggregate.UncommittedEvents.Count);
            Assert.Equal(1, testAggregate.EventStreamRevision);
        }

        [Fact]
        public async Task Given_A_Valid_Stream_Of_Events_When_The_Aggregate_Is_Loaded_Then_It_Does_So_Successfully()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage = new EventStoreMessage(Guid.NewGuid(), new TestMessageA());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.Equal(eventStreamId, testAggregate.EventStreamId);
            Assert.Equal(0, testAggregate.UncommittedEvents.Count);
            Assert.Equal(1, testAggregate.EventStreamRevision);
        }

        [Fact]
        public async Task Given_Three_Events_Stored_In_The_Event_Stream_In_A_Single_Operation_When_The_Aggregate_Is_Loaded_Then_The_Aggregate_Revision_Is_Three()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(3, testAggregateToLoad.EventStreamRevision);
        }

        [Fact]
        public async Task Given_Three_Events_Stored_In_The_Event_Stream_Using_Discrete_Operations_When_The_Aggregate_Is_Loaded_Then_The_Aggregate_Revision_Is_Three()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(3, testAggregateToLoad.EventStreamRevision);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_Uncommitted_Events_When_The_Aggregate_Is_Saved_Then_Afterwards_The_Aggregate_Can_Be_Loaded_Successfully()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate loadedTestAggregate = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(loadedTestAggregate).ConfigureAwait(false);

            // Assert
            Assert.Equal(eventStreamId, loadedTestAggregate.EventStreamId);
            Assert.Equal(0, loadedTestAggregate.UncommittedEvents.Count);
            Assert.Equal(1, loadedTestAggregate.EventStreamRevision);
        }

        [Fact]
        public void Given_A_Mock_Domain_Repository_And_Mock_Aggregate_When_Tests_Are_Run_On_The_Domain_Repository_Then_The_Test_Succeeds()
        {
            // Arrange
            Mock<IAggregate> mockAggregate = new Mock<IAggregate>();
            Mock<IDomainRepository> mockDomainRepository = new Mock<IDomainRepository>();

            // Act
            TestObjectThatUsesDomainRepository.DoSomething(mockDomainRepository.Object, mockAggregate.Object);

            // Assert
            mockDomainRepository.Verify(mdr => mdr.LoadAsync(It.IsAny<IAggregate>()));
            mockDomainRepository.Verify(mdr => mdr.SaveAsync(It.IsAny<IAggregate>()));
            mockDomainRepository.VerifyAll();
        }

        [Fact]
        public async Task Given_A_Separate_Update_To_The_Event_Stream_When_Save_Is_Used_Then_An_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            // Create an aggregate with some events and save it.
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);

            // Load a new instance of the aggregate and add another event.
            TestAggregate loadedTestAggregateA = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(loadedTestAggregateA).ConfigureAwait(false);
            loadedTestAggregateA.BusinessLogicThatResultsInEventA(string.Empty);

            // Load another instance of the aggregate, add another event and save it.
            TestAggregate loadedTestAggregateB = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(loadedTestAggregateB).ConfigureAwait(false);
            loadedTestAggregateB.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(loadedTestAggregateB).ConfigureAwait(false);

            // Assert
            // Save the first aggregate.
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                    async () => await eventStoreDomainRepository.SaveAsync(loadedTestAggregateA).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Preexisting_Aggregate_With_A_Stream_When_It_Is_Hydrated_Concurrently_Then_Acted_Upon_And_Saved_Should_Throw_Concurrency_Exception()
        {
            // Arrange
            string streamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            eventStoreProvider.AppendEvents(streamId, new TestMessageB());
            await eventStoreProvider.CommitEventsAsync(streamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);

            // Act
            TestAggregate testAggregate1 = new TestAggregate(streamId);
            TestAggregate testAggregate2 = new TestAggregate(streamId);
            await eventStoreDomainRepository.LoadAsync(testAggregate1).ConfigureAwait(false);
            await eventStoreDomainRepository.LoadAsync(testAggregate2).ConfigureAwait(false);
            testAggregate1.BusinessLogicThatResultsInEventA(string.Empty);
            testAggregate2.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregate1).ConfigureAwait(false);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreDomainRepository.SaveAsync(testAggregate2).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Separate_Update_To_The_Event_Stream_When_SaveWithoutConcurrencyCheck_Is_Used_Then_No_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            IDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            // Create an aggregate with some events and save it.
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);

            // Load a new instance of the aggregate and add another event.
            TestAggregate loadedTestAggregateA = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(loadedTestAggregateA).ConfigureAwait(false);
            loadedTestAggregateA.BusinessLogicThatResultsInEventA(string.Empty);

            // Load another instance of the aggregate, add another event and save it.
            TestAggregate loadedTestAggregateB = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(loadedTestAggregateB).ConfigureAwait(false);
            loadedTestAggregateB.BusinessLogicThatResultsInEventA(string.Empty);
            await eventStoreDomainRepository.SaveAsync(loadedTestAggregateB).ConfigureAwait(false);

            // Save the first aggregate.
            await eventStoreDomainRepository.SaveWithoutConcurrencyCheckAsync(loadedTestAggregateA).ConfigureAwait(false);

            // Reload the aggregate so we can check it.
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(testAggregate).ConfigureAwait(false);

            // Assert.
            // Check that all 3 events have been applied to the aggregate.
            Assert.Equal(3, testAggregate.EventStreamRevision);
        }

        [Fact]
        public async Task Given_An_Aggregate_Containing_Two_Events_With_The_Same_Id_When_The_Aggregate_Is_Saved_Then_Only_One_Event_Should_Written_To_The_Stream_And_A_Subsequent_Save_Of_The_Aggregate_Should_Succeed_Because_The_Stream_Revision_Of_The_Aggregate_Is_Kept_In_Sync()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventIdA = Guid.NewGuid();
            Guid eventIdB = Guid.NewGuid();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("some value 1", eventIdA);
            testAggregate.BusinessLogicThatResultsInEventA("some value 2", eventIdA);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            testAggregate.BusinessLogicThatResultsInEventA("some value 3", eventIdB);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);

            // Assert
            Assert.Equal(2, testAggregate.EventStreamRevision);
        }

        [Fact]
        public async Task Given_An_Aggregate_Containing_Two_Events_With_The_Same_Id_When_The_Aggregate_Is_Saved_Then_Only_The_First_Event_Should_Written_To_The_Stream()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            const string expectedValue = "the value we should end up with";
            Guid eventId = Guid.NewGuid();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA(expectedValue, eventId);
            testAggregate.BusinessLogicThatResultsInEventA("some other value", eventId);
            await eventStoreDomainRepository.SaveAsync(testAggregate).ConfigureAwait(false);

            // Assert
            TestAggregate testAggregateThatIsLoaded = new TestAggregate(eventStreamId);
            await eventStoreDomainRepository.LoadAsync(testAggregateThatIsLoaded).ConfigureAwait(false);
            Assert.Equal(1, testAggregateThatIsLoaded.EventStreamRevision);
            Assert.Equal(0, testAggregateThatIsLoaded.UncommittedEvents.Count);
            Assert.Equal(expectedValue, testAggregateThatIsLoaded.LastTestMessageA.Stuff);
        }

        [Fact]
        public async Task Given_An_Empty_Stream_When_An_Event_Is_Written_To_The_Stream_Through_SaveWithoutConcurrencyCheck_Then_Aggregate_Reflects_The_Correct_Stream_Revision()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend = Guid.NewGuid();

            // Act
            TestAggregate testAggregateA = new TestAggregate(eventStreamId);
            testAggregateA.BusinessLogicThatResultsInEventA("some value 1", eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend);
            await eventStoreDomainRepository.SaveWithoutConcurrencyCheckAsync(testAggregateA).ConfigureAwait(false);
            long eventStreamRevisionAfterFirstEvent = testAggregateA.EventStreamRevision;

            // Assert
            Assert.Equal(1, eventStreamRevisionAfterFirstEvent);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_An_Existing_Stream_When_A_Second_Event_Is_Written_To_The_Stream_Through_SaveWithoutConcurrencyCheck_With_An_Event_Id_Already_Used_In_The_Stream_Then_Aggregate_Reflects_The_Correct_Stream_Revision()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend = Guid.NewGuid();

            // Act
            TestAggregate testAggregateA = new TestAggregate(eventStreamId);
            testAggregateA.BusinessLogicThatResultsInEventA("some value 1", eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend);
            await eventStoreDomainRepository.SaveWithoutConcurrencyCheckAsync(testAggregateA).ConfigureAwait(false);
            long eventStreamRevisionAfterFirstEvent = testAggregateA.EventStreamRevision;
            testAggregateA.BusinessLogicThatResultsInEventA("some value 2", eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend);
            await eventStoreDomainRepository.SaveWithoutConcurrencyCheckAsync(testAggregateA).ConfigureAwait(false);
            long eventStreamRevisionAfterSecondEvent = testAggregateA.EventStreamRevision;

            // Assert
            Assert.Equal(1, eventStreamRevisionAfterFirstEvent);
            Assert.Equal(1, eventStreamRevisionAfterSecondEvent);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_An_Existing_Stream_When_A_Second_Event_Is_Written_With_An_Event_Id_Already_Used_In_The_Stream_Then_The_Second_Event_Is_Not_Written_To_The_Stream_And_The_Stream_Revision_Of_The_Aggregate_Reflects_This()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend = Guid.NewGuid();

            // Act
            TestAggregate testAggregateA = new TestAggregate(eventStreamId);
            testAggregateA.BusinessLogicThatResultsInEventA("some value 1", eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend);
            await eventStoreDomainRepository.SaveAsync(testAggregateA).ConfigureAwait(false);
            long eventStreamRevisionAfterFirstEvent = testAggregateA.EventStreamRevision;
            testAggregateA.BusinessLogicThatResultsInEventA("some value 2", eventIdThatWeAreGoingToReUseToCauseAnIdempotentAppend);
            await eventStoreDomainRepository.SaveAsync(testAggregateA).ConfigureAwait(false);
            long eventStreamRevisionAfterSecondEvent = testAggregateA.EventStreamRevision;

            // Assert
            Assert.Equal(1, eventStreamRevisionAfterFirstEvent);
            Assert.Equal(1, eventStreamRevisionAfterSecondEvent);
        }

        [Fact]
        public async Task Given_An_Aggregate_Which_Has_Not_Been_Saved_When_A_Snapshot_Is_Taken_Then_An_Error_Is_Raised()
        {
            // Arrange
            IEventStoreProvider eventStoreProvider = GetEventStoreProvider();
            EventStoreDomainRepository eventStoreDomainRepository = new EventStoreDomainRepository(eventStoreProvider);
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(string.Empty);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreDomainRepository.SaveSnapshotAsync(testAggregateToSave).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private static IEventStoreProvider GetEventStoreProvider()
        {
            return new InMemoryEventStoreProvider(new ConsoleLogger());
        }
    }
}
