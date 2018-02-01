namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class EventStoreDomainRepositoryLoadFromSnapshotTests
    {
        protected async Task Given_An_Aggregate_Which_Has_Been_Saved_When_A_Snapshot_Is_Taken_Then_The_Aggregate_Can_Be_Loaded_From_The_Latest_Snapshot(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            const string testValue = "some value";

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(testValue);
            await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(testAggregateToSave.EventStreamId, testAggregateToLoad.EventStreamId);
            Assert.Equal(testAggregateToSave.EventStreamRevision, testAggregateToLoad.EventStreamRevision);
            Assert.Equal(testValue, testAggregateToLoad.LastTestMessageA.Stuff);
            Assert.Equal(0, testAggregateToLoad.UncommittedEvents.Count);
        }

        protected async Task Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_And_Had_Subsequent_Events_Committed_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_State_Includes_The_Events_Committed_After_The_Snapshot_Was_Saved(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            const string firstTestValue = "value at time of snapshot";
            const string secondTestValue = "value post snapshot";

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(firstTestValue);
            await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregateToSave).ConfigureAwait(false);
            testAggregateToSave.BusinessLogicThatResultsInEventA(secondTestValue);
            await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(eventStreamId, testAggregateToLoad.EventStreamId);
            Assert.Equal(2, testAggregateToLoad.EventStreamRevision);
            Assert.Equal(0, testAggregateToLoad.UncommittedEvents.Count);
            Assert.Equal(secondTestValue, testAggregateToLoad.LastTestMessageA.Stuff);
        }

        protected async Task Given_An_Aggregate_With_Two_Events_That_Have_The_Same_Id_When_Saving_A_Snapshot_Then_The_Revision_Should_Be_One(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventId = Guid.NewGuid();

            // Act
            TestAggregate testAggregateToSaveAndSnapshot = new TestAggregate(eventStreamId);
            testAggregateToSaveAndSnapshot.BusinessLogicThatResultsInEventA("some value 1", eventId);
            testAggregateToSaveAndSnapshot.BusinessLogicThatResultsInEventA("some value 2", eventId);
            await domainRepository.SaveAsync(testAggregateToSaveAndSnapshot).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregateToSaveAndSnapshot).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, testAggregateToLoad.EventStreamRevision);
        }

        protected async Task Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_Loads_From_The_Snapshot_With_No_Additional_Events(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            const string testValue = "value at time of snapshot";

            // Act
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA(testValue);
            await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregateToSave).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(eventStreamId, testAggregateToLoad.EventStreamId);
            Assert.Equal(1, testAggregateToLoad.EventStreamRevision);
            Assert.Equal(0, testAggregateToLoad.UncommittedEvents.Count);
            Assert.Equal(testValue, testAggregateToLoad.LastTestMessageA.Stuff);
        }

        protected async Task Given_An_Aggregate_With_Events_When_A_Snapshot_Is_Taken_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_One_Before_The_Snapshot_And_Zero_After_The_Snapshot(IDomainRepository domainRepository)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            long numberOfEventsSinceLastSnapshotBeforeUpdate = testAggregate.NumberOfEventsSinceLastSnapshot;
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            long numberOfEventsSinceLastSnapshotBeforeSave = testAggregate.NumberOfEventsSinceLastSnapshot;
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterSave = testAggregate.NumberOfEventsSinceLastSnapshot;
            await domainRepository.SaveSnapshotAsync(testAggregate).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterAfterSnapshot = testAggregate.NumberOfEventsSinceLastSnapshot;
            testAggregate.BusinessLogicThatResultsInEventA("value2");
            long numberOfEventsSinceLastSnapshotAfterAfterSnapshotAfterUpdate = testAggregate.NumberOfEventsSinceLastSnapshot;

            // Assert
            Assert.Equal(0, numberOfEventsSinceLastSnapshotBeforeUpdate);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotBeforeSave);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterSave);
            Assert.Equal(0, numberOfEventsSinceLastSnapshotAfterAfterSnapshot);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterAfterSnapshotAfterUpdate);
        }

        protected async Task Given_An_Aggregate_Loaded_From_A_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_Zero_After_The_Load_And_One_After_An_Update(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregate).ConfigureAwait(false);
            TestAggregate testAggregateLoadedFromSnapshot = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshot).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterLoadFromSnapshot = testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot;
            testAggregateLoadedFromSnapshot.BusinessLogicThatResultsInEventA("valeu2");
            long numberOfEventsSinceLastSnapshotAfterLoadFromSnapshotAfterUpdate = testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot;

            // Assert
            Assert.Equal(0, numberOfEventsSinceLastSnapshotAfterLoadFromSnapshot);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterLoadFromSnapshotAfterUpdate);
        }

        protected async Task Given_An_Aggregate_With_Snapshot_And_One_Event_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregate).ConfigureAwait(false);
            testAggregate.BusinessLogicThatResultsInEventA("value2");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            TestAggregate testAggregateLoadedFromSnapshot = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshot).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot);
        }

        protected async Task Given_An_Aggregate_With_Snapshot_And_Three_Events_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_Three(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregate).ConfigureAwait(false);
            testAggregate.BusinessLogicThatResultsInEventA("value2");
            testAggregate.BusinessLogicThatResultsInEventA("value3");
            testAggregate.BusinessLogicThatResultsInEventA("value4");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            TestAggregate testAggregateLoadedFromSnapshot = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshot).ConfigureAwait(false);

            // Assert
            Assert.Equal(3, testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot);
        }

        protected async Task Given_An_Aggregate_With_A_Stream_And_Snapshot_At_The_End_Of_The_Stream_When_The_Aggregate_Is_Loaded_From_The_Snapshot_And_Saved_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            testAggregate.BusinessLogicThatResultsInEventA("value2");
            testAggregate.BusinessLogicThatResultsInEventA("value3");
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            await domainRepository.SaveSnapshotAsync(testAggregate).ConfigureAwait(false);
            TestAggregate testAggregateLoadedFromSnapshot = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshot).ConfigureAwait(false);
            testAggregateLoadedFromSnapshot.BusinessLogicThatResultsInEventA("value4");
            await domainRepository.SaveAsync(testAggregateLoadedFromSnapshot).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot);
        }

        protected async Task Given_An_Aggregate_Throughout_The_Lifecycle_When_The_Number_Of_Events_Since_Last_Snapshot_Checked_Then_It_Reports_The_Correct_Value(IDomainRepository domainRepository, Func<TestAggregate, Task> loadAggregateAsync)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            TestAggregate testAggregate = new TestAggregate(eventStreamId);
            testAggregate.BusinessLogicThatResultsInEventA("value1");
            long numberOfEventsSinceLastSnapshotAfterApply = testAggregate.NumberOfEventsSinceLastSnapshot;
            await domainRepository.SaveAsync(testAggregate).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterSave = testAggregate.NumberOfEventsSinceLastSnapshot;
            TestAggregate testAggregateLoadedFromStream = new TestAggregate(eventStreamId);
            await domainRepository.LoadAsync(testAggregateLoadedFromStream).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterLoadFromStream = testAggregateLoadedFromStream.NumberOfEventsSinceLastSnapshot;
            await domainRepository.SaveSnapshotAsync(testAggregateLoadedFromStream).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterSnapshot = testAggregateLoadedFromStream.NumberOfEventsSinceLastSnapshot;
            TestAggregate testAggregateLoadedFromSnapshot = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshot).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterLoadFromSnapshot = testAggregateLoadedFromSnapshot.NumberOfEventsSinceLastSnapshot;
            testAggregateLoadedFromSnapshot.BusinessLogicThatResultsInEventA("value2");
            await domainRepository.SaveAsync(testAggregateLoadedFromSnapshot).ConfigureAwait(false);
            TestAggregate testAggregateLoadedFromSnapshotAndSubsequentStreamEvents = new TestAggregate(eventStreamId);
            await loadAggregateAsync.Invoke(testAggregateLoadedFromSnapshotAndSubsequentStreamEvents).ConfigureAwait(false);
            long numberOfEventsSinceLastSnapshotAfterLoadFromSnapshotAndSubsequentStreamEvents = testAggregateLoadedFromSnapshotAndSubsequentStreamEvents.NumberOfEventsSinceLastSnapshot;

            // Assert
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterApply);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterSave);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterLoadFromStream);
            Assert.Equal(0, numberOfEventsSinceLastSnapshotAfterSnapshot);
            Assert.Equal(0, numberOfEventsSinceLastSnapshotAfterLoadFromSnapshot);
            Assert.Equal(1, numberOfEventsSinceLastSnapshotAfterLoadFromSnapshotAndSubsequentStreamEvents);
        }

        protected async Task Should_Correctly_Count_Number_Of_Events_Since_Last_Snapshot_And_Load_From_Latest_Snapshot(IDomainRepository domainRepository)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregateToSave = new TestAggregate(eventStreamId);
            testAggregateToSave.BusinessLogicThatResultsInEventA("init");
            await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
            int snapshotsTaken = 0;

            // Act
            for (int i = 0; i < 100; i++)
            {
                testAggregateToSave = new TestAggregate(eventStreamId);
                await domainRepository.LoadFromLatestSnapshotIfExistsAsync(testAggregateToSave).ConfigureAwait(false);
                testAggregateToSave.BusinessLogicThatResultsInEventA(i.ToString(CultureInfo.InvariantCulture));
                if (testAggregateToSave.UncommittedEvents.Count > 0)
                {
                    await domainRepository.SaveAsync(testAggregateToSave).ConfigureAwait(false);
                    if (testAggregateToSave.NumberOfEventsSinceLastSnapshot >= 25)
                    {
                        snapshotsTaken++;
                        await domainRepository.SaveSnapshotAsync(testAggregateToSave).ConfigureAwait(false);
                    }
                }
            }

            // Assert
            Assert.Equal(4, snapshotsTaken);
        }
    }
}
