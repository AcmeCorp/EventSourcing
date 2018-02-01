namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Collections.Generic;
    using AcmeCorp.EventSourcing.Configuration;
    using Xunit;

    public class EventStoreMetadataTests
    {
        [Fact]
        public void Should_Produce_Metadata_Given_A_Test_Event_And_Id_When_Building_Default_Metadata_For_Message()
        {
            // Arrange
            TestEvent testEvent = new TestEvent();
            Guid eventId = Guid.NewGuid();

            // Act
            Dictionary<string, object> metadata = EventStoreMetadata.CreateDefaultForMessage(testEvent, eventId);

            // Assert
            Assert.Equal(3, metadata.Count);
            Assert.True(metadata.ContainsKey(EventStoreMessageHeaderKey.EventId));
            Assert.True(metadata.ContainsKey(EventStoreMetadataKey.FullNameAssemblyQualified));
            Assert.True(metadata.ContainsKey(EventStoreMetadataKey.FullNameAssemblyQualifiedStrongName));
        }
    }
}
