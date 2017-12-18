using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class StringPropertyHandlerTests : PropertyHandlerTestsBase {

        private static PropertyInfo stringProperty { get; }

        static StringPropertyHandlerTests() {
            var propertyName = nameof(FakeImmutable.StringProperty);

            stringProperty = typeof(FakeImmutable).GetProperty(propertyName);
        }

        [Fact]
        public void Constructor_NullProperty() {

            // Arrange

            PropertyInfo property = null;

            // Act

            var exception = Record.Exception(
                () => new StringPropertyHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NonStringProperty() {

            // Arrange

            var property =
                typeof(FakeImmutable)
                    .GetProperty(nameof(FakeImmutable.NullableDateTimeProperty));

            // Act

            var exception = Record.Exception(
                () => new StringPropertyHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{property.Name}' property is not of type 'String'." +
                Environment.NewLine + "Parameter name: property",
                exception.Message
            );
        }

        [Fact]
        public void Constructor_NullComparer() {

            // Arrange

            StringComparer comparer = null;

            // Act

            var exception = Record.Exception(
                () => new StringPropertyHandler(stringProperty, comparer)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [InlineData("foo", "foo")]
        [InlineData("foo", "bar")]
        [InlineData("FOO", "foo")]
        public void ArePropertyValuesEqual_OrdinalByDefault(string left, string right) {

            // Arrange

            var handler = this.CreateHandler(stringProperty);
            var leftFake = this.NextFake().SetStringProperty(left);
            var rightFake = this.NextFake().SetStringProperty(right);

            // Act

            var comparerResult = StringComparer.Ordinal.Equals(left, right);
            var handlerResult = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.Equal(comparerResult, handlerResult);
        }

        public static IEnumerable<object[]> GetPropertyValueHashCode_NullValues_MemberData {
            get {
                yield return new object[] { StringComparer.Ordinal };
                yield return new object[] { StringComparer.OrdinalIgnoreCase };
                yield return new object[] { StringComparer.CurrentCulture };
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyValueHashCode_NullValues_MemberData))]
        public void GetPropertyValueHashCode_NullValues(StringComparer comparer) {

            // Arrange

            var handler = this.CreateHandler(stringProperty, comparer);
            var fake = this.NextFake().SetStringProperty(null);

            // Act

            var hashCode = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(37, hashCode);

            // 37 is the universal default hash code for "null" that is
            // defined in the abstract PropertyHandlerBase implementation.
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("Foo")]
        [InlineData("FOO")]
        public void GetPropertyValueHashCode_OrdinalByDefault(string stringValue) {

            // Arrange

            var handler = this.CreateHandler(stringProperty);
            var fake = this.NextFake().SetStringProperty(stringValue);

            // Act

            var comparerResult = StringComparer.Ordinal.GetHashCode(stringValue);
            var handlerResult = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(comparerResult, handlerResult);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_MemberData { get; } =
            ImmutableList.Create<object[]>(
                new object[] { "foo", "foo", StringComparer.Ordinal },
                new object[] { "FOO", "foo", StringComparer.Ordinal },
                new object[] { "foo", "bar", StringComparer.Ordinal },
                new object[] { null, "foo", StringComparer.Ordinal },
                new object[] { "foo", null, StringComparer.Ordinal },
                new object[] { null, null, StringComparer.Ordinal },
                new object[] { "foo", "foo", StringComparer.OrdinalIgnoreCase },
                new object[] { "FOO", "foo", StringComparer.OrdinalIgnoreCase },
                new object[] { "foo", "bar", StringComparer.OrdinalIgnoreCase },
                new object[] { null, "foo", StringComparer.OrdinalIgnoreCase },
                new object[] { "foo", null, StringComparer.OrdinalIgnoreCase },
                new object[] { null, null, StringComparer.OrdinalIgnoreCase }
            );

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_MemberData))]
        public void ArePropertyValuesEqual_MatchesComparer(
            string left,
            string right,
            StringComparer comparer) {

            // Arrange

            var handler = this.CreateHandler(stringProperty, comparer);
            var leftFake = this.NextFake().SetStringProperty(left);
            var rightFake = this.NextFake().SetStringProperty(right);

            // Act

            var comparerResult = comparer.Equals(left, right);
            var handlerResult = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.Equal(comparerResult, handlerResult);
        }

        public static IEnumerable<object[]> GetPropertyValueHashCode_MemberData { get; } =
            ImmutableList.Create<object[]>(
                new object[] { "foo", StringComparer.Ordinal },
                new object[] { "FOO", StringComparer.Ordinal },
                new object[] { "foo", StringComparer.OrdinalIgnoreCase },
                new object[] { "FOO", StringComparer.OrdinalIgnoreCase }
            );

        [Theory]
        [MemberData(nameof(GetPropertyValueHashCode_MemberData))]
        public void GetPropertyValueHashCode_MatchesComparer(
            String stringValue,
            StringComparer comparer) {

            // Arrange

            var handler = this.CreateHandler(stringProperty, comparer);
            var fake = this.NextFake().SetStringProperty(stringValue);

            // Act

            var comparerResult = comparer.GetHashCode(stringValue);
            var handlerResult = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(comparerResult, handlerResult);
        }

        [Fact]
        public void GetPropertyValueDisplayString_NullValueDoesNotWrapQuotes() {

            // Arrange

            var handler = this.CreateHandler(stringProperty);
            var fake = this.NextFake().SetStringProperty(null);

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Equal("null", displayString);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("BAR")]
        [InlineData("")]
        [InlineData(" ")]
        public void GetPropertyValueDisplayString_NonNullWrapsWithQuotes(String value) {

            // Arrange

            var handler = this.CreateHandler(stringProperty);
            var fake = this.NextFake().SetStringProperty(value);

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Equal(String.Concat("\"", value, "\""), displayString);
        }

        protected override PropertyInfo NextValidPropertyInfo() {
            return stringProperty;
        }

        protected override object NextParent() {
            return this.NextFake();
        }

        private FakeImmutable NextFake() {
            return new FakeImmutable(Guid.NewGuid().ToString(), DateTime.UtcNow);
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new StringPropertyHandler(property);
        }

        private IPropertyHandler CreateHandler(PropertyInfo property, StringComparer comparer) {
            return new StringPropertyHandler(property, comparer);
        }

        public sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public String StringProperty { get; }
            public DateTime? NullableDateTimeProperty { get; }
            public Guid UnrelatedGuid { get; }

            public FakeImmutable(
                String stringProperty = default(String),
                DateTime? nullableDateTimeProperty = default(DateTime?),
                Guid unrelatedGuid = default(Guid)) {

                this.StringProperty = stringProperty;
                this.NullableDateTimeProperty = nullableDateTimeProperty;
                this.UnrelatedGuid = unrelatedGuid;
            }

            public FakeImmutable SetStringProperty(String stringProperty) {
                return this.SetPropertyValueImpl(nameof(StringProperty), stringProperty);
            }

        }

    }

}
