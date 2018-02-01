namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Providers.InMemory;
    using Xunit;

    public class EventStoreDomainRepositoryLoadFromSnapshotIfExistsTests : EventStoreDomainRepositoryLoadFromSnapshotTests
    {
        [Fact]
        public async Task Given_An_Aggregate_Which_Has_Been_Saved_When_A_Snapshot_Is_Taken_Then_The_Aggregate_Can_Be_Loaded_From_The_Latest_Snapshot_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_Which_Has_Been_Saved_When_A_Snapshot_Is_Taken_Then_The_Aggregate_Can_Be_Loaded_From_The_Latest_Snapshot(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_And_Had_Subsequent_Events_Committed_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_State_Includes_The_Events_Committed_After_The_Snapshot_Was_Saved_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_And_Had_Subsequent_Events_Committed_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_State_Includes_The_Events_Committed_After_The_Snapshot_Was_Saved(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_Two_Events_That_Have_The_Same_Id_When_Saving_A_Snapshot_Then_The_Revision_Should_Be_One_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_With_Two_Events_That_Have_The_Same_Id_When_Saving_A_Snapshot_Then_The_Revision_Should_Be_One(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_Loads_From_The_Snapshot_With_No_Additional_Events_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_That_Has_Been_Saved_And_Had_A_Snapshot_Saved_When_The_Aggregate_Is_Loaded_From_The_Snapshot_Then_The_Aggregate_Loads_From_The_Snapshot_With_No_Additional_Events(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_Events_When_A_Snapshot_Is_Taken_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_One_Before_The_Snapshot_And_Zero_After_The_Snapshot_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_With_Events_When_A_Snapshot_Is_Taken_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_One_Before_The_Snapshot_And_Zero_After_The_Snapshot(domainRepository).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_Loaded_From_A_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_Zero_After_The_Load_And_One_After_An_Update_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_Loaded_From_A_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Count_Is_Zero_After_The_Load_And_One_After_An_Update(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_Snapshot_And_One_Event_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_With_Snapshot_And_One_Event_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_Snapshot_And_Three_Events_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_Three_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_With_Snapshot_And_Three_Events_In_The_Stream_Since_The_Snapshot_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_Three(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_A_Stream_And_Snapshot_At_The_End_Of_The_Stream_When_The_Aggregate_Is_Loaded_From_The_Snapshot_And_Saved_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_With_A_Stream_And_Snapshot_At_The_End_Of_The_Stream_When_The_Aggregate_Is_Loaded_From_The_Snapshot_And_Saved_Then_The_Number_Of_Events_Since_Last_Snapshot_Is_One(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_Throughout_The_Lifecycle_When_The_Number_Of_Events_Since_Last_Snapshot_Checked_Then_It_Reports_The_Correct_Value_For_Load_From_Latest_If_Exists()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Given_An_Aggregate_Throughout_The_Lifecycle_When_The_Number_Of_Events_Since_Last_Snapshot_Checked_Then_It_Reports_The_Correct_Value(domainRepository, LoadAggregateAsync(domainRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Should_Correctly_Count_Number_Of_Events_Since_Last_Snapshot_And_Load_From_Latest_Snapshot_For_Load_From_Latest()
        {
            IDomainRepository domainRepository = GetDomainRepository();
            await this.Should_Correctly_Count_Number_Of_Events_Since_Last_Snapshot_And_Load_From_Latest_Snapshot(domainRepository).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_No_Snapshots_When_Loading_From_Latest_Snapshot_Then_The_Aggregate_Should_Be_Loaded_From_The_Stream()
        {
            // Arrange
            IDomainRepository domainRepository = GetDomainRepository();
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventId = Guid.NewGuid();

            // Act
            TestAggregate testAggregateToSaveAndSnapshot = new TestAggregate(eventStreamId);
            testAggregateToSaveAndSnapshot.BusinessLogicThatResultsInEventA("some value 1", eventId);
            await domainRepository.SaveAsync(testAggregateToSaveAndSnapshot).ConfigureAwait(false);
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);
            await domainRepository.LoadFromLatestSnapshotIfExistsAsync(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, testAggregateToLoad.EventStreamRevision);
        }

        [Fact]
        public async Task Given_An_Aggregate_With_No_Stream_And_No_Snapshots_When_Loading_From_Latest_Snapshot_Then_The_Aggregate_Should_Be_Empty()
        {
            // Arrange
            IDomainRepository domainRepository = GetDomainRepository();
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            TestAggregate testAggregateToLoad = new TestAggregate(eventStreamId);

            // Act
            await domainRepository.LoadFromLatestSnapshotIfExistsAsync(testAggregateToLoad).ConfigureAwait(false);

            // Assert
            Assert.Equal(0, testAggregateToLoad.EventStreamRevision);
        }

        private static IDomainRepository GetDomainRepository()
        {
            IEventStoreProvider eventStoreProvider = new InMemoryEventStoreProvider();
            IDomainRepository domainRepository = new EventStoreDomainRepository(eventStoreProvider);
            return domainRepository;
        }

        private static Func<TestAggregate, Task> LoadAggregateAsync(IDomainRepository domainRepository)
        {
            return domainRepository.LoadFromLatestSnapshotIfExistsAsync;
        }
    }
}
