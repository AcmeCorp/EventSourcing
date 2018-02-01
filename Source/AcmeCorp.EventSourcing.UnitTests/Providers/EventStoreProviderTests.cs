namespace AcmeCorp.EventSourcing.UnitTests.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Configuration;
    using Xunit;

    public abstract class EventStoreProviderTests
    {
        protected async Task Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_StreamExists_Is_Called_Then_False_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            bool streamExists = await eventStoreProvider.StreamExistsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.False(streamExists);
        }

        protected async Task Given_An_Id_For_A_Stream_That_Does_Exist_When_StreamExists_Is_Called_Then_True_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            object eventStoreMessage1 = new object();

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            bool streamExists = await eventStoreProvider.StreamExistsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.True(streamExists);
        }

        protected async Task Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_ReadEvents_Is_Called_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            // Act
            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.ReadEventsAsync(Guid.NewGuid().ToEventStreamIdFormattedString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_An_Existing_Stream_When_Events_Are_Committed_With_Expectation_Of_New_Stream_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            object eventStoreMessage1 = new object();
            object eventStoreMessage2 = new object();

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_Uncommitted_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Exist(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            object eventStoreMessage1 = new object();
            object eventStoreMessage2 = new object();

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1, eventStoreMessage2);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_Committed_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Be_Empty(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            object eventStoreMessage1 = new object();
            object eventStoreMessage2 = new object();

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            Assert.Equal(2, eventStoreMessages.Count());
        }

        protected async Task Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            Assert.Single(eventStoreMessages);
        }

        protected async Task Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            Guid eventId = Guid.NewGuid();
            EventStoreMessage eventStoreMessage = new EventStoreMessage(eventId, new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> messages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            EventStoreMessage[] eventStoreMessages = messages.ToArray();
            Assert.Single(eventStoreMessages);
            Assert.Equal(eventId, eventStoreMessages[0].EventId);
            Assert.Equal(2, eventStoreMessages[0].Headers.Count);
        }

        protected async Task Given_An_Appended_Event_With_Custom_Metadata_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            const string myMetadataKey = "myKey";
            const string myMetadataValue = "myValue";

            // Act
            Guid eventId = Guid.NewGuid();
            EventStoreMessage eventStoreMessage = new EventStoreMessage(eventId, new object());
            eventStoreMessage.Headers.Add(myMetadataKey, myMetadataValue);
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> messages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            EventStoreMessage[] eventStoreMessages = messages.ToArray();
            Assert.Single(eventStoreMessages);
            Assert.Equal(eventId, eventStoreMessages[0].EventId);
            Assert.Equal(3, eventStoreMessages[0].Headers.Count);
            Assert.Equal(myMetadataValue, eventStoreMessages[0].Headers[myMetadataKey]);
        }

        protected async Task Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_In_The_Same_Commit_Then_Only_One_Event_Is_Added(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventId = Guid.NewGuid();
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(eventId, new object());
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(eventId, new object());

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            IEnumerable<EventStoreMessage> messages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.Single(messages);
        }

        protected async Task Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_Across_Different_Commits_Then_Only_One_Event_Is_Added(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            Guid eventId = Guid.NewGuid();
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(eventId, new object());
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(eventId, new object());

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            long firstCommitCount = await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);
            long secondCommitCount = await eventStoreProvider.CommitEventsAsync(eventStreamId, 1).ConfigureAwait(false);
            IEnumerable<EventStoreMessage> messages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, firstCommitCount);
            Assert.Equal(0, secondCommitCount);
            Assert.Single(messages);
        }

        protected async Task Given_A_Stream_Of_10_Events_When_Reading_Events_From_Three_To_Nine_Then_Six_Events_Should_Be_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            for (int i = 0; i < 10; i++)
            {
                Guid eventId = Guid.NewGuid();
                EventStoreMessage eventStoreMessage = new EventStoreMessage(eventId, new object());
                eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);
            }

            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEventStoreStream eventStoreStream = await eventStoreProvider.ReadEventsAsync(eventStreamId, 3, 9).ConfigureAwait(false);
            Assert.Equal(6, eventStoreStream.Count);
        }

        protected async Task Given_A_Stream_Revision_Less_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage3 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage3);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_A_Stream_Revision_Greater_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage);

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.CommitEventsAsync(eventStreamId, 1000).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_Separate_Commits_To_The_Store_Then_The_Events_Are_Saved_Successfully(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 1).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage3 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage3);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 2).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            Assert.Equal(3, eventStoreMessages.Count());
        }

        protected async Task Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_A_Single_Commit_To_The_Store_Then_The_Events_Are_Saved_Successfully(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(Guid.NewGuid(), new object());
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage3 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage3);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 2).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            Assert.Equal(3, eventStoreMessages.Count());
        }

        protected async Task Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Lower_Than_Actual_Revision_Then_A_Truncated_List_Of_Events_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage2 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage2);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 1).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage3 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage3);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 2).ConfigureAwait(false);
            EventStoreMessage eventStoreMessage4 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage4);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, 3).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId, 2).ConfigureAwait(false);
            Assert.Equal(2, eventStoreMessages.Count());
        }

        protected async Task Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Greater_Than_Actual_Revision_Then_The_Full_List_Of_Events_Is_Returned_And_No_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            EventStoreMessage eventStoreMessage1 = new EventStoreMessage(Guid.NewGuid(), new object());
            eventStoreProvider.AppendEvents(eventStreamId, eventStoreMessage1);
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);

            // Assert
            IEnumerable<EventStoreMessage> eventStoreMessages = await eventStoreProvider.ReadEventsAsync(eventStreamId).ConfigureAwait(false);
            Assert.Single(eventStoreMessages);
        }

        protected async Task Given_A_Valid_Snapshot_When_The_Snapshot_Is_Added_Then_The_Snapshot_Can_Be_Read_And_The_Retrieved_Snapshot_Has_The_Correct_Metadata(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            string snapshotStreamId = string.Format(CultureInfo.InvariantCulture, "{0}-snapshot", eventStreamId);
            Guid snapshotId = Guid.NewGuid();
            const int streamRevision = 3;

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, new object(), new object(), new object());
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreSnapshot eventStoreSnapshotToBeWritten = new EventStoreSnapshot(snapshotId, streamRevision, new object());
            await eventStoreProvider.AddSnapshotAsync(eventStreamId, snapshotStreamId, eventStoreSnapshotToBeWritten).ConfigureAwait(false);
            EventStoreSnapshot eventStoreSnapshotThatWasRead = await eventStoreProvider.ReadSnapshotAsync(snapshotStreamId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(eventStoreSnapshotThatWasRead);
            Assert.Equal(snapshotId, eventStoreSnapshotThatWasRead.SnapshotId);
            Assert.Equal(streamRevision, eventStoreSnapshotThatWasRead.StreamRevision);
            Assert.Equal(3, eventStoreSnapshotThatWasRead.Headers.Count);
        }

        protected async Task Given_No_Existing_Stream_When_The_Snapshot_Is_Added_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            string snapshotStreamId = string.Format(CultureInfo.InvariantCulture, "{0}-snapshot", eventStreamId);
            Guid snapshotId = Guid.NewGuid();
            const int streamRevision = 3;

            // Act
            EventStoreSnapshot eventStoreSnapshotToBeWritten = new EventStoreSnapshot(snapshotId, streamRevision, new object());

            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.AddSnapshotAsync(eventStreamId, snapshotStreamId, eventStoreSnapshotToBeWritten).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_No_Snapshots_For_A_Stream_When_Reading_A_Snapshot_Then_An_Error_Is_Raised(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            // Assert
            await AssertExtensions.ThrowsAsync<EventStreamNotFoundException>(
                async () => await eventStoreProvider.ReadSnapshotAsync(eventStreamId).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task Given_A_Snapshot_Of_Expected_Type_Exists_When_Checking_A_Snapshot_Then_True_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            string snapshotStreamId = string.Format(CultureInfo.InvariantCulture, "{0}-snapshot", eventStreamId);
            Guid snapshotId = Guid.NewGuid();
            const int streamRevision = 3;

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, new object(), new object(), new object());
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreSnapshot eventStoreSnapshotToBeWritten = new EventStoreSnapshot(
                snapshotId, streamRevision, new TestAggregateSnapshot(new TestMessageA()));
            await eventStoreProvider.AddSnapshotAsync(eventStreamId, snapshotStreamId, eventStoreSnapshotToBeWritten).ConfigureAwait(false);
            bool snapshotExists = await eventStoreProvider.CheckSnapshotExistsAsync<TestAggregateSnapshot>(snapshotStreamId).ConfigureAwait(false);

            // Assert
            Assert.True(snapshotExists);
        }

        protected async Task Given_No_Snapshots_For_A_Stream_When_Checking_A_Snapshot_Exists_Then_False_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();

            // Act
            bool snapshotExists = await eventStoreProvider.CheckSnapshotExistsAsync<TestAggregateSnapshot>(eventStreamId).ConfigureAwait(false);

            // Assert
            Assert.False(snapshotExists);
        }

        protected async Task Given_A_Snapshot_Of_Different_Type_Exists_When_Checking_A_Snapshot_Then_False_Is_Returned(IEventStoreProvider eventStoreProvider)
        {
            // Arrange
            string eventStreamId = Guid.NewGuid().ToEventStreamIdFormattedString();
            string snapshotStreamId = string.Format(CultureInfo.InvariantCulture, "{0}-snapshot", eventStreamId);
            Guid snapshotId = Guid.NewGuid();
            const int streamRevision = 3;

            // Act
            eventStoreProvider.AppendEvents(eventStreamId, new object(), new object(), new object());
            await eventStoreProvider.CommitEventsAsync(eventStreamId, ExpectedStreamRevision.New).ConfigureAwait(false);
            EventStoreSnapshot eventStoreSnapshotToBeWritten = new EventStoreSnapshot(snapshotId, streamRevision, new object());
            await eventStoreProvider.AddSnapshotAsync(eventStreamId, snapshotStreamId, eventStoreSnapshotToBeWritten).ConfigureAwait(false);
            bool snapshotExists = await eventStoreProvider.CheckSnapshotExistsAsync<TestAggregateSnapshot>(snapshotStreamId).ConfigureAwait(false);

            // Assert
            Assert.False(snapshotExists);
        }
    }
}
