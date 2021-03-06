using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class SimpleImmutableTests {

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
            var originalValue = fake.StringProperty;

            // Act

            var updated = fake.SetStringProperty(newStringValue);

            // Assert

            Assert.Equal(originalValue, fake.StringProperty);
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
            var originalValue = fake.ValueProperty;

            // Act

            var updated = fake.SetValueProperty(newValue);

            // Assert

            Assert.Equal(originalValue, fake.ValueProperty);
            Assert.Equal(newValue, updated.ValueProperty);
        }

        [Fact]
        public void SetReferenceProperty_Null() {

            // Arrange

            var fake = this.NextFake();

            // Act

            var updated = fake.SetReferenceProperty(null);

            // Assert

            Assert.Null(fake.ReferenceProperty);
        }

        [Fact]
        public void SetReferenceProperty_NonNull() {

            // Arrange

            var fake = this.NextFake();
            var value = new object();

            // Act

            var updated = fake.SetReferenceProperty(value);

            // Assert

            Assert.Equal(value, updated.ReferenceProperty);
        }

        [Fact]
        public void Inequality_NullObject() {

            // Arrange

            var fake = this.NextFake();
            object other = null;

            // Act

            var isEqual = fake.Equals(other);

            // Assert

            Assert.False(isEqual);
        }

        [Fact]
        public void Inequality_NonNullTypeMismatch() {

            // Arrange

            var fake = this.NextFake();
            var other = new object();

            // Act

            var isEqual = fake.Equals(other);

            // Assert

            Assert.False(isEqual);
        }

        [Fact]
        public void Inequality_Null_MatchingImmutableType() {

            // Arrange

            var fake = this.NextFake();
            SimpleImmutableFake other = null;

            // Act

            var isEqual = fake.Equals(other);

            // Assert

            Assert.False(isEqual);
            AssertFakesNotEqual(fake, other);
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
        public void Inequality_SetReferenceProperty_Null() {

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

        [Fact]
        public void MultipleConstructors_ValidImmutableSetterDecoration() {

            // Arrange

            var original = new SimpleDecoratedFake("Foo", Guid.NewGuid());

            // Act

            const string updatedFoo = "Updated";
            var updated = original.SetFoo(updatedFoo);

            // Assert

            Assert.Equal(updatedFoo, updated.Foo);
            Assert.Equal(original.Bar, updated.Bar);
        }

        [Fact]
        public void EqualityOverride_Nulls() {

            // Arrange

            SimpleImmutableFake left = null;
            SimpleImmutableFake right = null;

            // Act

            var isEqual = (left == right);

            // Assert

            Assert.True(isEqual);
        }

        [Fact]
        public void InequalityOverride_Nulls() {

            // Arrange

            SimpleImmutableFake left = null;
            SimpleImmutableFake right = null;

            // Act

            var isNotEqual = (left != right);

            // Assert

            Assert.False(isNotEqual);
        }

        private static void AssertFakesEqual(
            SimpleImmutableFake left,
            SimpleImmutableFake right) {

            Assert.Equal(left, right);
            Assert.Equal((Object)left, (Object)right);
            Assert.Equal(right, left);
            Assert.Equal((Object)right, (Object)left);
            Assert.True(left == right);
            Assert.False(left != right);

            if (left != null && right != null) {
                Assert.Equal(left.GetHashCode(), right.GetHashCode());
            }
        }

        private static void AssertFakesNotEqual(
            SimpleImmutableFake left,
            SimpleImmutableFake right) {

            Assert.NotEqual(left, right);
            Assert.NotEqual((Object)left, (Object)right);
            Assert.False(left == right);
            Assert.True(left != right);
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

        private class SimpleDecoratedFake : ImmutableBase<SimpleDecoratedFake> {

            public String Foo { get; }
            public Guid Bar { get; }

            public SimpleDecoratedFake(Tuple<String, Guid> tuple) :
                this(tuple.Item1, tuple.Item2) {}

            [ImmutableSetterConstructor]
            public SimpleDecoratedFake(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar;
            }

            public SimpleDecoratedFake SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

    }

}
