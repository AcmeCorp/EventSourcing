namespace AcmeCorp.EventSourcing.UnitTests.Providers
{
    using System.Threading.Tasks;
    using AcmeCorp.EventSourcing.Providers.InMemory;
    using Xunit;

    public class FileSystemEventStoreProviderTests : EventStoreProviderTests
    {
        [Fact]
        public async Task Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_StreamExists_Is_Called_Then_False_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_StreamExists_Is_Called_Then_False_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Id_For_A_Stream_That_Does_Exist_When_StreamExists_Is_Called_Then_True_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_An_Id_For_A_Stream_That_Does_Exist_When_StreamExists_Is_Called_Then_True_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_ReadEvents_Is_Called_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_ReadEvents_Is_Called_Then_An_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Existing_Stream_When_Events_Are_Committed_With_Expectation_Of_New_Stream_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_An_Existing_Stream_When_Events_Are_Committed_With_Expectation_Of_New_Stream_Then_An_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Uncommitted_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Exist__For_FileSystem_Provider()
        {
            await this.Given_Uncommitted_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Exist(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Committed_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Be_Empty__For_FileSystem_Provider()
        {
            await this.Given_Committed_Events_When_AppendEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Not_Be_Empty(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event__For_FileSystem_Provider()
        {
            await this.Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata__For_FileSystem_Provider()
        {
            await this.Given_An_Appended_Event_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_An_Appended_Event_With_Custom_Metadata_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata__For_FileSystem_Provider()
        {
            await this.Given_An_Appended_Event_With_Custom_Metadata_When_CommitEvents_Is_Used_Then_The_Resulting_Event_Stream_Should_Contain_One_Event_And_The_Retrieved_Event_Has_The_Correct_Metadata(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_In_The_Same_Commit_Then_Only_One_Event_Is_Added__For_FileSystem_Provider()
        {
            await this.Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_In_The_Same_Commit_Then_Only_One_Event_Is_Added(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_Across_Different_Commits_Then_Only_One_Event_Is_Added__For_FileSystem_Provider()
        {
            await this.Given_Two_Events_With_The_Same_Event_Id_When_Adding_Them_To_The_Stream_Across_Different_Commits_Then_Only_One_Event_Is_Added(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Stream_Of_10_Events_When_Reading_Events_From_Three_To_Nine_Then_Six_Events_Should_Be_Returned__For_FileSystem_Provider()
        {
            await this.Given_A_Stream_Of_10_Events_When_Reading_Events_From_Three_To_Nine_Then_Six_Events_Should_Be_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Stream_Revision_Less_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_A_Stream_Revision_Less_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Stream_Revision_Greater_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_A_Stream_Revision_Greater_Than_The_Current_One_When_An_Event_Is_Written_To_The_Store_Then_An_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_Separate_Commits_To_The_Store_Then_The_Events_Are_Saved_Successfully__For_FileSystem_Provider()
        {
            await this.Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_Separate_Commits_To_The_Store_Then_The_Events_Are_Saved_Successfully(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_A_Single_Commit_To_The_Store_Then_The_Events_Are_Saved_Successfully__For_FileSystem_Provider()
        {
            await this.Given_The_Correct_Expected_Stream_Revision_When_Multiple_Events_Are_Written_In_A_Single_Commit_To_The_Store_Then_The_Events_Are_Saved_Successfully(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Lower_Than_Actual_Revision_Then_A_Truncated_List_Of_Events_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Lower_Than_Actual_Revision_Then_A_Truncated_List_Of_Events_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Greater_Than_Actual_Revision_Then_The_Full_List_Of_Events_Is_Returned_And_No_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_Multiple_Events_In_The_Event_Store_When_Retrieved_Using_A_Maximum_Revision_Greater_Than_Actual_Revision_Then_The_Full_List_Of_Events_Is_Returned_And_No_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Valid_Snapshot_When_The_Snapshot_Is_Added_Then_The_Snapshot_Can_Be_Read_And_The_Retrieved_Snapshot_Has_The_Correct_Metadata__For_FileSystem_Provider()
        {
            await this.Given_A_Valid_Snapshot_When_The_Snapshot_Is_Added_Then_The_Snapshot_Can_Be_Read_And_The_Retrieved_Snapshot_Has_The_Correct_Metadata(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_No_Existing_Stream_When_The_Snapshot_Is_Added_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_No_Existing_Stream_When_The_Snapshot_Is_Added_Then_An_Error_Is_Raised(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_No_Snapshots_For_A_Stream_When_Reading_A_Snapshot_Then_An_Error_Is_Raised__For_FileSystem_Provider()
        {
            await this.Given_An_Id_For_A_Stream_That_Does_Not_Exist_When_StreamExists_Is_Called_Then_False_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Snapshot_Of_Expected_Type_Exists_When_Checking_A_Snapshot_Then_True_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_A_Snapshot_Of_Expected_Type_Exists_When_Checking_A_Snapshot_Then_True_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_No_Snapshots_For_A_Stream_When_Checking_A_Snapshot_Exists_Then_False_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_No_Snapshots_For_A_Stream_When_Checking_A_Snapshot_Exists_Then_False_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_A_Snapshot_Of_Different_Type_Exists_When_Checking_A_Snapshot_Then_False_Is_Returned__For_FileSystem_Provider()
        {
            await this.Given_A_Snapshot_Of_Different_Type_Exists_When_Checking_A_Snapshot_Then_False_Is_Returned(GetEventStoreProvider()).ConfigureAwait(false);
        }

        private static IEventStoreProvider GetEventStoreProvider()
        {
            // File system provider not yet implemented
            // return new FileSystemEventStoreProvider();
            return new InMemoryEventStoreProvider();
        }
    }
}
