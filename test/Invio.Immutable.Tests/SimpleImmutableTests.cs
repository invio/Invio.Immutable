using System;
using Xunit;

namespace Invio.Immutable {

    public class SimpleImmutableTests {

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("foo")]
        public void SetReferenceProperty(String newReferenceValue) {

            // Arrange

            var fake = new SimpleImmutableFake();
            var defaultValue = fake.ReferenceProperty;

            // Act

            var updated = fake.SetReferenceProperty(newReferenceValue);

            // Assert

            Assert.Equal(defaultValue, fake.ReferenceProperty);
            Assert.Equal(newReferenceValue, updated.ReferenceProperty);
        }

        [Theory]
        [InlineData(Int32.MinValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void SetValueProperty(int newValue) {

            // Arrange

            var fake = new SimpleImmutableFake();
            var defaultValue = fake.ValueProperty;

            // Act

            var updated = fake.SetValueProperty(newValue);

            // Assert

            Assert.Equal(defaultValue, fake.ValueProperty);
            Assert.Equal(newValue, updated.ValueProperty);
        }

        [Fact]
        public void Inequality_Null() {

            // Arrange

            var fake = new SimpleImmutableFake();

            // Act

            var isEqual = fake.Equals(null);

            // Assert

            Assert.False(isEqual);
        }

        [Theory]
        [InlineData(Int32.MinValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void Equality_SetValueProperty(int value) {

            // Arrange

            var fake = new SimpleImmutableFake();

            // Act

            var left = fake.SetValueProperty(value);
            var right = fake.SetValueProperty(value);

            // Assert

            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.Equal(left, right);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("foo")]
        public void Equality_SetReferenceProperty(String referenceValue) {

            // Arrange

            var fake = new SimpleImmutableFake();

            // Act

            var left = fake.SetReferenceProperty(referenceValue);
            var right = fake.SetReferenceProperty(referenceValue);

            // Assert

            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.Equal(left, right);
        }

        private class SimpleImmutableFake : ImmutableBase<SimpleImmutableFake> {

            public int ValueProperty { get; }
            public String ReferenceProperty { get; }

            public SimpleImmutableFake(
                int valueProperty = default(int),
                String referenceProperty = default(String)) {

                this.ValueProperty = valueProperty;
                this.ReferenceProperty = referenceProperty;
            }

            public SimpleImmutableFake SetValueProperty(int valueProperty) {
                return this.SetPropertyValueImpl(nameof(ValueProperty), valueProperty);
            }

            public SimpleImmutableFake SetReferenceProperty(String referenceProperty) {
                return this.SetPropertyValueImpl(nameof(ReferenceProperty), referenceProperty);
            }

        }

    }

}
