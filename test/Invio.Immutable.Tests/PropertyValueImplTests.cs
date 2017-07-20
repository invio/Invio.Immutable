using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class PropertyValueImplTests {

        private Random random { get; }

        public PropertyValueImplTests() {
            this.random = new Random();
        }

        [Fact]
        public void GetPropertyValueImpl_NullPropertyName() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var exception = Record.Exception(
                () => fake.GetPropertyValue(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValueImpl_NonExistentProperty() {

            // Arrange

            var fake = this.NextFake();
            const string propertyName = "ThisPropertyDoesNotExist";

            // Act

            var exception = Record.Exception(
                () => fake.GetPropertyValue(propertyName)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{propertyName}' property was not found." +
                Environment.NewLine + "Parameter name: propertyName",
                exception.Message
            );
        }

        [Fact]
        public void GetPropertyValueImpl_NullValue() {

            // Arrange

            var fake = this.NextFake().SetStringProperty(null);

            // Act

            var value = fake.GetPropertyValue(nameof(Fake.StringProperty));

            // Assert

            Assert.Null(value);
        }

        [Fact]
        public void GetPropertyValueImpl_NonNullValue() {

            // Arrange

            const int number = 42;
            var fake = this.NextFake().SetNumber(number);

            // Act

            var value = fake.GetPropertyValue(nameof(Fake.Number));

            // Assert

            Assert.Equal(number, value);
        }

        [Fact]
        public void SetPropertyValueImpl_NullPropertyName() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValue(null, 5)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void SetPropertyValueImpl_NonExistentProperty() {

            // Arrange

            var fake = this.NextFake();
            const string propertyName = "ThisPropertyDoesNotExist";

            // Act

            var exception = Record.Exception(
               () => fake.SetPropertyValue(propertyName, 5)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{propertyName}' property was not found." +
                Environment.NewLine + "Parameter name: propertyName",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValueImpl_InvalidValueType_Null() {

            // Arrange

            var fake = this.NextFake();
            var propertyName = nameof(Fake.Number);

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValue(propertyName, null)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"Unable to assign the value (null) to the '{propertyName}' property." +
                Environment.NewLine + "Parameter name: value",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValueImpl_InvalidPropertyType_NonNull() {

            // Arrange

            var fake = this.NextFake();
            var propertyName = nameof(Fake.Number);
            const string value = "foo";

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValue(propertyName, value)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"Unable to assign the value ({value}) to the '{propertyName}' property." +
                Environment.NewLine + "Parameter name: value",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValueImpl_NullableProperty() {

            // Arrange

            var fake = this.NextFake();
            var propertyName = nameof(Fake.NullableDateTime);

            // Act

            var updated = fake.SetPropertyValue(propertyName, null);

            // Assert

            Assert.Null(updated.NullableDateTime);
        }

        private Fake NextFake() {
            return new Fake(
                this.random.Next(),
                Guid.NewGuid().ToString("N"),
                this.random.Next(2) == 0
                    ? (DateTime?)null
                    : (DateTime?)new DateTime(this.random.Next(), DateTimeKind.Utc)
            );
        }

        private class Fake : ImmutableBase<Fake> {

            public int Number { get; }
            public String StringProperty { get; }
            public DateTime? NullableDateTime { get; }

            public Fake(
                int number,
                String stringProperty,
                DateTime? nullableDateTime) {

                this.Number = number;
                this.StringProperty = stringProperty;
                this.NullableDateTime = nullableDateTime;
            }

            public object GetPropertyValue(String propertyName) {
                return this.GetPropertyValueImpl(propertyName);
            }

            public Fake SetPropertyValue(String propertyName, object value) {
                return this.SetPropertyValueImpl(propertyName, value);
            }

            public Fake SetNumber(int number) {
                return this.SetPropertyValueImpl(nameof(Number), number);
            }

            public Fake SetStringProperty(string stringProperty) {
                return this.SetPropertyValueImpl(nameof(StringProperty), stringProperty);
            }

            public Fake SetNullableDateTime(DateTime? nullable) {
                return this.SetPropertyValueImpl(nameof(NullableDateTime), nullable);
            }

        }

    }

}
