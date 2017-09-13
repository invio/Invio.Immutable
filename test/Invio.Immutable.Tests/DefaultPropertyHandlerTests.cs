using System;
using System.Collections.Generic;
using System.Reflection;
using Invio.Hashing;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DefaultPropertyHandlerTests {

        private static Random random { get; }

        static DefaultPropertyHandlerTests() {
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

        public static IEnumerable<object[]> AllPropertyNames {
            get {
                yield return new object[] { nameof(FakeImmutable.StringProperty) };
                yield return new object[] { nameof(FakeImmutable.Int32Property) };
                yield return new object[] { nameof(FakeImmutable.NullableDateTimeProperty) };
            }
        }

        [Theory]
        [MemberData(nameof(AllPropertyNames))]
        public void PropertyName(String expectedPropertyName) {

            // Arrange

            var handler = CreateHandler(expectedPropertyName);

            // Act

            var actualPropertyName = handler.PropertyName;

            // Assert

            Assert.Equal(expectedPropertyName, actualPropertyName);
        }

        [Theory]
        [InlineData(nameof(FakeImmutable.StringProperty), typeof(String))]
        [InlineData(nameof(FakeImmutable.Int32Property), typeof(Int32))]
        [InlineData(nameof(FakeImmutable.NullableDateTimeProperty), typeof(DateTime?))]
        public void PropertyType(String propertyName, Type expectedPropertyType) {

            // Arrange

            var handler = CreateHandler(propertyName);

            // Act

            var actualPropertyType = handler.PropertyType;

            // Assert

            Assert.Equal(expectedPropertyType, actualPropertyType);
        }

        [Fact]
        public void GetPropertyValueHashCode_NullParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueHashCode(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValueHashCode_InvalidParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));
            var invalidParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueHashCode(invalidParent)
            );

            // Assert

            Assert.Equal(
                "The value object provided is of type Object, " +
                "which does not contains the StringProperty property." +
                Environment.NewLine + "Parameter name: parent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
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

            var handler = CreateHandler(propertyName);
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
            // other objects.

            const int expectedHashCode = 37;

            var handler = CreateHandler(propertyName);
            var fake = NextFake().SetPropertyValue(propertyName, null);

            // Act

            var actualHashCode = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(expectedHashCode, actualHashCode);
        }

        [Fact]
        public void ArePropertyValuesEqual_NullParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(null, null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_InvalidParent_Parameters {
            get {
                yield return new object[] { NextFake(), new object(), "rightParent" };
                yield return new object[] { new object(), NextFake(), "leftParent" };
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_InvalidParent_Parameters))]
        public void ArePropertyValuesEqual_InvalidParent(
            Object leftParent,
            Object rightParent,
            String parameterName) {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(leftParent, rightParent)
            );

            // Assert

            Assert.Equal(
                "The value object provided is of type Object, " +
                "which does not contains the StringProperty property." +
                Environment.NewLine + "Parameter name: " + parameterName,
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
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

            var handler = CreateHandler(propertyName);

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

            var handler = CreateHandler(propertyName);

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

        [Fact]
        public void GetPropertyValue_NullParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValue(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValue_InvalidParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));
            var invalidParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValue(invalidParent)
            );

            // Assert

            Assert.Equal(
                "The value object provided is of type Object, " +
                "which does not contains the StringProperty property." +
                Environment.NewLine + "Parameter name: parent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
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

            var handler = CreateHandler(propertyName);
            var parent = NextFake().SetPropertyValue(propertyName, expectedValue);

            // Act

            var actualValue = handler.GetPropertyValue(parent);

            // Assert

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetPropertyValueDisplayString_NullParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueDisplayString(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValueDisplayString_InvalidParent() {

            // Arrange

            var handler = CreateHandler(nameof(FakeImmutable.StringProperty));
            var invalidParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueDisplayString(invalidParent)
            );

            // Assert

            Assert.Equal(
                "The value object provided is of type Object, " +
                "which does not contains the StringProperty property." +
                Environment.NewLine + "Parameter name: parent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
        }

        [Theory]
        [MemberData(nameof(PropertyValues_NonNullParameters))]
        public void GetPropertyValueDisplayString_NonNullValues(
            String propertyName,
            object propertyValue) {

            // Arrange

            var handler = CreateHandler(propertyName);
            var parent = NextFake().SetPropertyValue(propertyName, propertyValue);
            var expectedString = propertyValue.ToString();

            // Act

            var actualString = handler.GetPropertyValueDisplayString(parent);

            // Assert

            Assert.Equal(expectedString, actualString);
        }

        [Theory]
        [InlineData(nameof(FakeImmutable.StringProperty))]
        [InlineData(nameof(FakeImmutable.NullableDateTimeProperty))]
        public void GetPropertyValueDisplayString_NullValues(String propertyName) {

            // Arrange

            const string expectedString = "null";

            var handler = CreateHandler(propertyName);
            var parent = NextFake().SetPropertyValue(propertyName, null);

            // Act

            var actualString = handler.GetPropertyValueDisplayString(parent);

            // Assert

            Assert.Equal(expectedString, actualString);
        }

        private static IPropertyHandler CreateHandler(String propertyName) {
            var property = typeof(FakeImmutable).GetProperty(propertyName);

            return new DefaultPropertyHandler(property);
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
