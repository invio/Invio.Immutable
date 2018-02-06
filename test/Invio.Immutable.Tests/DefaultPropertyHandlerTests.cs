using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DefaultPropertyHandlerTests : PropertyHandlerTestsBase {

        private static ISet<PropertyInfo> properties { get; }
        private static Random random { get; }

        static DefaultPropertyHandlerTests() {
            var parent = typeof(FakeImmutable);

            properties = ImmutableHashSet.Create(
                parent.GetProperty(nameof(FakeImmutable.StringProperty)),
                parent.GetProperty(nameof(FakeImmutable.Int32Property)),
                parent.GetProperty(nameof(FakeImmutable.NullableDateTimeProperty))
            );

            random = new Random();
        }

        [Fact]
        public void Constructor_NullProperty() {

            // Arrange

            PropertyInfo property = null;

            // Act

            var exception = Record.Exception(
                () => new DefaultPropertyHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        public static IEnumerable<object[]> GetPropertyValueHashCode_NonNull_Parameters {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.StringProperty),
                    Guid.NewGuid().ToString("N")
                };
                yield return new object[] {
                    nameof(FakeImmutable.Int32Property),
                    random.Next()
                };
                yield return new object[] {
                    nameof(FakeImmutable.NullableDateTimeProperty),
                    new DateTime(2006, 07, 22, 0, 0, 0, DateTimeKind.Utc) // <3
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyValueHashCode_NonNull_Parameters))]
        public void GetPropertyValueHashCode_UsesPropertyValueHashCodeForNonNull(
            String propertyName,
            Object propertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = NextFake().SetPropertyValue(propertyName, propertyValue);

            // Act

            var nativeHashCode = propertyValue.GetHashCode();
            var handlerHashCode = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(nativeHashCode, handlerHashCode);
        }

        [Theory]
        [InlineData(nameof(FakeImmutable.StringProperty))]
        [InlineData(nameof(FakeImmutable.NullableDateTimeProperty))]
        public void GetPropertyValueHashCode_UsesConstantForNull(String propertyName) {

            // Arrange

            // Why 37? No particular reason, other than it's a prime number
            // which means it is less likely to match the hash code of
            // other objects after the use of a modulus operator.

            const int expectedHashCode = 37;

            var handler = this.CreateHandler(propertyName);
            var fake = NextFake().SetPropertyValue(propertyName, null);

            // Act

            var actualHashCode = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(expectedHashCode, actualHashCode);
        }

        public static IEnumerable<object[]> PropertyValues_NonNullParameters {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.StringProperty),
                    Guid.NewGuid().ToString("N")
                };
                yield return new object[] {
                    nameof(FakeImmutable.Int32Property),
                    random.Next()
                };
                yield return new object[] {
                    nameof(FakeImmutable.NullableDateTimeProperty),
                    new DateTime(2006, 07, 22, 0, 0, 0, DateTimeKind.Utc) // <3
                };
            }
        }

        [Theory]
        [MemberData(nameof(PropertyValues_NonNullParameters))]
        public void ArePropertyValuesEqual_NonNullComparisons(
            String propertyName,
            Object propertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);

            var original = NextFake();
            var withNewValue = original.SetPropertyValue(propertyName, propertyValue);

            // Act

            var isEqualWithSameValues = handler.ArePropertyValuesEqual(original, original);
            var isEqualWithNewValue = handler.ArePropertyValuesEqual(original, withNewValue);

            // Assert

            Assert.True(isEqualWithSameValues);
            Assert.False(isEqualWithNewValue);
        }

        [Theory]
        [InlineData(nameof(FakeImmutable.StringProperty))]
        [InlineData(nameof(FakeImmutable.NullableDateTimeProperty))]
        public void ArePropertyValuesEqual_NullComparisons(String propertyName) {

            // Arrange

            var handler = this.CreateHandler(propertyName);

            var original = NextFake();
            var withNull = original.SetPropertyValue(propertyName, null);

            // Act

            var isEqualWithBothNull = handler.ArePropertyValuesEqual(withNull, withNull);
            var isEqualWithOnlyLeftNull = handler.ArePropertyValuesEqual(withNull, original);
            var isEqualWithOnlyRightNull = handler.ArePropertyValuesEqual(original, withNull);

            // Assert

            Assert.True(isEqualWithBothNull);
            Assert.False(isEqualWithOnlyLeftNull);
            Assert.False(isEqualWithOnlyRightNull);
        }

        public static IEnumerable<object[]> GetPropertyValue_Parameters {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.StringProperty),
                    Guid.NewGuid().ToString("N")
                };
                yield return new object[] {
                    nameof(FakeImmutable.StringProperty),
                    null
                };
                yield return new object[] {
                    nameof(FakeImmutable.Int32Property),
                    random.Next()
                };
                yield return new object[] {
                    nameof(FakeImmutable.NullableDateTimeProperty),
                    new DateTime(2006, 07, 22, 0, 0, 0, DateTimeKind.Utc) // <3
                };
                yield return new object[] {
                    nameof(FakeImmutable.NullableDateTimeProperty),
                    null
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyValue_Parameters))]
        public void GetPropertyValue(String propertyName, object expectedValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var parent = NextFake().SetPropertyValue(propertyName, expectedValue);

            // Act

            var actualValue = handler.GetPropertyValue(parent);

            // Assert

            Assert.Equal(expectedValue, actualValue);
        }

        protected override PropertyInfo NextValidPropertyInfo() {
            return
                properties
                    .Skip(random.Next(0, properties.Count))
                    .First();
        }

        protected override object NextParent() {
            return NextFake();
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new DefaultPropertyHandler(property);
        }

        private IPropertyHandler CreateHandler(String propertyName) {
            var property = typeof(FakeImmutable).GetProperty(propertyName);

            return this.CreateHandler(property);
        }

        private static FakeImmutable NextFake() {
            return new FakeImmutable(
                Guid.NewGuid().ToString("N"),
                random.Next(),
                DateTime.UtcNow
            );
        }

        public sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public String StringProperty { get; }
            public Int32 Int32Property { get; }
            public DateTime? NullableDateTimeProperty { get; }

            public FakeImmutable(
                String stringProperty = default(String),
                Int32 int32Property = default(Int32),
                DateTime? nullableDateTimeProperty = default(DateTime?)) {

                this.StringProperty = stringProperty;
                this.Int32Property = int32Property;
                this.NullableDateTimeProperty = nullableDateTimeProperty;
            }

            public FakeImmutable SetPropertyValue(String propertyName, Object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

    }

}
