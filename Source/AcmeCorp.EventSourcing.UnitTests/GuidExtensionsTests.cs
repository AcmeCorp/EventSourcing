namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using Xunit;

    public class GuidExtensionsTests
    {
        [Fact]
        public void Should_Return_A_Guid_String_Lowercase_With_Hyphen_Spacing_And_No_Brackets_Given_A_Known_Guid_When_Using_The_Format_Method()
        {
            // Arrange
            Guid guid = Guid.Parse("{8B71A737-8AC4-4176-8D20-56C7DE2B4646}");

            // Act
            string eventStreamIdFormattedString = guid.ToEventStreamIdFormattedString();

            // Act
            Assert.Equal("8b71a737-8ac4-4176-8d20-56c7de2b4646", eventStreamIdFormattedString);
        }
    }
}
