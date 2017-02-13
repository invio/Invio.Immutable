using System;
using Xunit;

namespace Invio.Immutable {

    public class SimpleImmutableTests {

        private Random random { get; }

        public SimpleImmutableTests() {
            this.random = new Random();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("foo")]
        public void SetStringProperty(String newStringValue) {

            // Arrange

            var fake = this.NextFake();
            var defaultValue = fake.StringProperty;

            // Act

            var updated = fake.SetStringProperty(newStringValue);

            // Assert

            Assert.Equal(defaultValue, fake.StringProperty);
            Assert.Equal(newStringValue, updated.StringProperty);
        }

        [Theory]
        [InlineData(Int32.MinValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void SetValueProperty(int newValue) {

            // Arrange

            var fake = this.NextFake();
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

            var fake = this.NextFake();

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

            var fake = this.NextFake();

            // Act

            var left = fake.SetValueProperty(value);
            var right = fake.SetValueProperty(value);

            // Assert

            AssertFakesEqual(left, right);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("foo")]
        public void Equality_SetStringProperty(String stringValue) {

            // Arrange

            var fake = this.NextFake();

            // Act

            var left = fake.SetStringProperty(stringValue);
            var right = fake.SetStringProperty(stringValue);

            // Assert

            AssertFakesEqual(left, right);
        }

        [Fact]
        public void Equality_SetReferenceProperty_NonNull() {

            // Arrange

            var fake = this.NextFake();
            var value = new object();

            // Act

            var left = fake.SetReferenceProperty(value);
            var right = fake.SetReferenceProperty(value);

            // Assert

            AssertFakesEqual(left, right);
        }

        [Fact]
        public void Equality_SetReferenceProperty_Null() {

            // Arrange

            var fake = this.NextFake();
            object value = null;

            // Act

            var left = fake.SetReferenceProperty(value);
            var right = fake.SetReferenceProperty(value);

            // Assert

            AssertFakesEqual(left, right);
        }

        [Fact]
        public void Inequality_SetReferenceProperty_NonNull() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var left = fake.SetReferenceProperty(new object());
            var right = fake.SetReferenceProperty(new object());

            // Assert

            AssertFakesNotEqual(left, right);
        }

        [Fact]
        public void Inequality_SetRefernceProperty_Null() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var left = fake.SetReferenceProperty(new object());
            var right = fake.SetReferenceProperty(null);

            // Assert

            AssertFakesNotEqual(left, right);
        }

        [Fact]
        public void Equality_SameImmutableObjectReference() {

            // Assert

            var fake = this.NextFake();

            // Assert

            AssertFakesEqual(fake, fake);
        }

        private static void AssertFakesEqual(
            SimpleImmutableFake left,
            SimpleImmutableFake right) {

            Assert.Equal(left, right);
            Assert.Equal((Object)left, (Object)right);
            Assert.Equal(right, left);
            Assert.Equal((Object)right, (Object)left);

            if (left != null && right != null) {
                Assert.Equal(left.GetHashCode(), right.GetHashCode());
            }
        }

        private static void AssertFakesNotEqual(
            SimpleImmutableFake left,
            SimpleImmutableFake right) {

            Assert.NotEqual(left, right);
            Assert.NotEqual((Object)left, (Object)right);
        }

        private SimpleImmutableFake NextFake() {
            return new SimpleImmutableFake(
                this.random.Next(),
                Guid.NewGuid().ToString("N"),
                this.random.Next(0, 1) == 1 ? new object() : null
            );
        }

        private class SimpleImmutableFake : ImmutableBase<SimpleImmutableFake> {

            public int ValueProperty { get; }
            public String StringProperty { get; }
            public Object ReferenceProperty { get; }

            public SimpleImmutableFake(
                int valueProperty,
                String stringProperty,
                Object referenceProperty) {

                this.ValueProperty = valueProperty;
                this.StringProperty = stringProperty;
                this.ReferenceProperty = referenceProperty;
            }

            public SimpleImmutableFake SetValueProperty(int valueProperty) {
                return this.SetPropertyValueImpl(nameof(ValueProperty), valueProperty);
            }

            public SimpleImmutableFake SetStringProperty(String stringProperty) {
                return this.SetPropertyValueImpl(nameof(StringProperty), stringProperty);
            }

            public SimpleImmutableFake SetReferenceProperty(Object referenceProperty) {
                return this.SetPropertyValueImpl(nameof(ReferenceProperty), referenceProperty);
            }

        }

    }

}
