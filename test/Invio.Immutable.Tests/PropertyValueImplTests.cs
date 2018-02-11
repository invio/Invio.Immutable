using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public void GetPropertyValueImpl_CaseInsensitivePropertyName() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var actualValue = fake.GetPropertyValue(nameof(Fake.StringProperty).ToLower());

            // Assert

            Assert.Equal(fake.StringProperty, actualValue);
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
        public void SetPropertyValueImpl_CaseInsensitivePropertyName() {

            // Arrange

            var original = this.NextFake();
            var propertyName = nameof(Fake.Number).ToUpper();
            const int number = 5;

            // Act

            var updated = original.SetPropertyValue(propertyName, number);

            // Assert

            Assert.Equal(number, updated.Number);
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

        [Fact]
        public void SetPropertyValuesImpl_Null() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValues(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void SetPropertyValuesImpl_Empty() {

            // Arrange

            var original = this.NextFake();
            var propertyValues = ImmutableDictionary<string, object>.Empty;

            // Act

            var result = original.SetPropertyValues(propertyValues);

            // Assert

            Assert.Equal(original, result);
        }

        [Fact]
        public void SetPropertyValuesImpl_NonExistentProperty() {

            // Arrange

            var fake = this.NextFake();

            const string propertyName = "ThisPropertyDoesNotExist";

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(propertyName, 5);

            // Act

            var exception = Record.Exception(
               () => fake.SetPropertyValues(propertyValues)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{propertyName}' property was not found." +
                Environment.NewLine + "Parameter name: propertyValues",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValuesImpl_RedundantPropertyAssignments() {

            // Arrange

            var fake = this.NextFake();

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(nameof(Fake.Number).ToUpper(), 5)
                    .Add(nameof(Fake.Number).ToLower(), 5);

            // Act

            var exception = Record.Exception(
               () => fake.SetPropertyValues(propertyValues)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{nameof(Fake.Number)}' property was specified more than once." +
                Environment.NewLine + "Parameter name: propertyValues",
                exception.Message,
                ignoreCase: true
            );
        }

        [Fact]
        public void SetPropertyValuesImpl_CaseInsensitivePropertyNames() {

            // Arrange

            var original = this.NextFake();

            var propertyName = nameof(Fake.Number).ToUpper();
            const int number = 5;

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(propertyName, number);

            // Act

            var updated = original.SetPropertyValues(propertyValues);

            // Assert

            Assert.Equal(number, updated.Number);
        }

        [Fact]
        public void SetPropertyValuesImpl_InvalidValueType_Null() {

            // Arrange

            var fake = this.NextFake();

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(nameof(Fake.Number), null);

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValues(propertyValues)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"Unable to assign the value (null) to the 'Number' property." +
                Environment.NewLine + "Parameter name: propertyValues",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValuesImpl_InvalidPropertyType_NonNull() {

            // Arrange

            var fake = this.NextFake();

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(nameof(Fake.Number), "foo");

            // Act

            var exception = Record.Exception(
                () => fake.SetPropertyValues(propertyValues)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"Unable to assign the value (foo) to the 'Number' property." +
                Environment.NewLine + "Parameter name: propertyValues",
                exception.Message
            );
        }

        [Fact]
        public void SetPropertyValuesImpl_MultipleProperties() {

            // Arrange

            var original = this.NextFake();

            const int assignedNumber = 5;
            const string assignedString = "Foo";

            var propertyValues =
                ImmutableDictionary<string, object>
                    .Empty
                    .Add(nameof(Fake.Number), assignedNumber)
                    .Add(nameof(Fake.StringProperty), assignedString);

            // Act

            var updated = original.SetPropertyValues(propertyValues);

            // Assert

            Assert.Equal(assignedNumber, updated.Number);
            Assert.Equal(assignedString, updated.StringProperty);
            Assert.Equal(original.NullableDateTime, updated.NullableDateTime);
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

            public Fake SetPropertyValues(IDictionary<string, object> propertyValues) {
                return this.SetPropertyValuesImpl(propertyValues);
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
