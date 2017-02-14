using System;
using Xunit;

namespace Invio.Immutable {

    public class SetPropertyValueImplTests {

        private Random random { get; }

        public SetPropertyValueImplTests() {
            this.random = new Random();
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
            var propertyName = nameof(ExposedSettersFake.Number);

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
            var propertyName = nameof(ExposedSettersFake.Number);
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
            var propertyName = nameof(ExposedSettersFake.NullableDateTime);

            // Act

            var updated = fake.SetPropertyValue(propertyName, null);

            // Assert

            Assert.Null(updated.NullableDateTime);
        }

        private ExposedSettersFake NextFake() {
            return new ExposedSettersFake(
                this.random.Next(),
                Guid.NewGuid().ToString("N"),
                this.random.Next(2) == 0
                    ? (DateTime?)null
                    : (DateTime?)new DateTime(this.random.Next(), DateTimeKind.Utc)
            );
        }

        private class ExposedSettersFake : ImmutableBase<ExposedSettersFake> {

            public int Number { get; }
            public String StringProperty { get; }
            public DateTime? NullableDateTime { get; }

            public ExposedSettersFake(
                int number,
                String stringProperty,
                DateTime? nullableDateTime) {

                this.Number = number;
                this.StringProperty = stringProperty;
                this.NullableDateTime = nullableDateTime;
            }

            public ExposedSettersFake SetPropertyValue(String propertyName, object value) {
                return this.SetPropertyValueImpl(propertyName, value);
            }

        }

    }

}
