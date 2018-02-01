namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Globalization;
    using Xunit;

    public class ObjectAndByteArrayExtensionsTests
    {
        [Fact]
        public void Should_Deserialize_Successfully_To_Expected_Type_Given_A_Valid_Object_When_Using_Generic_Deserialize_Method()
        {
            // Arrange
            TestObjectToSerializeAsJsonByteEncoded testObjectA = new TestObjectToSerializeAsJsonByteEncoded
            {
                SomeProperty = "myValue"
            };

            // Act
            byte[] byteEncoded = testObjectA.SerializeToJsonByteEncoded();
            TestObjectToSerializeAsJsonByteEncoded testObjectB = byteEncoded.DeserializeFromJsonByteEncoded<TestObjectToSerializeAsJsonByteEncoded>();

            // Assert
            Assert.NotNull(testObjectB);
            Assert.Equal(testObjectA.SomeProperty, testObjectB.SomeProperty);
        }

        [Fact]
        public void Should_Deserialize_Successfully_To_Expected_Type_Given_A_Valid_Object_When_Using_Deserialize_Method()
        {
            // Arrange
            TestObjectToSerializeAsJsonByteEncoded testObjectA = new TestObjectToSerializeAsJsonByteEncoded
            {
                SomeProperty = "myValue"
            };

            // Act
            byte[] byteEncoded = testObjectA.SerializeToJsonByteEncoded();
            object result = byteEncoded.DeserializeFromJsonByteEncoded(typeof(TestObjectToSerializeAsJsonByteEncoded));
            TestObjectToSerializeAsJsonByteEncoded testObjectB = result as TestObjectToSerializeAsJsonByteEncoded;

            // Assert
            Assert.NotNull(testObjectB);
            Assert.Equal(testObjectA.SomeProperty, testObjectB.SomeProperty);
        }

        [CLSCompliant(false)]
        [Theory]
        [InlineData(10, "10.00")]
        [InlineData(10.0, "10.00")]
        [InlineData(10.1, "10.1")]
        [InlineData(10.123, "10.123")]
        public void Should_Deserialize_Decimal_Type_And_Remove_Redundant_Zero(decimal value, string expectedSerializedValue)
        {
            // Arrange
            TestObjectToSerializeAsJsonByteEncoded testObjectA = new TestObjectToSerializeAsJsonByteEncoded
            {
                Value = value
            };

            // Act
            byte[] byteEncoded = testObjectA.SerializeToJsonByteEncoded();
            object result = byteEncoded.DeserializeFromJsonByteEncoded(typeof(TestObjectToSerializeAsJsonByteEncoded));
            TestObjectToSerializeAsJsonByteEncoded testObjectB = result as TestObjectToSerializeAsJsonByteEncoded;

            // Assert
            Assert.NotNull(testObjectB);
            Assert.Equal(expectedSerializedValue, testObjectB.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
