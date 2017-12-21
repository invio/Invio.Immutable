using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DateTimePropertyHandlerTests : PropertyHandlerTestsBase {

        private static ISet<PropertyInfo> properties { get; }
        private static ThreadLocal<Random> random { get; }

        static DateTimePropertyHandlerTests() {
            var fakeType = typeof(FakeImmutable);

            properties = ImmutableHashSet.Create<PropertyInfo>(
                fakeType.GetProperty(nameof(FakeImmutable.DateTimeProperty)),
                fakeType.GetProperty(nameof(FakeImmutable.NullableProperty))
            );

            random = new ThreadLocal<Random>(() => new Random());
        }

        [Fact]
        public void Constructor_NullProperty() {

            // Arrange

            PropertyInfo property = null;

            // Act

            var exception = Record.Exception(
                () => new DateTimePropertyHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NonDateTimeProperty() {

            // Arrange

            var propertyName = nameof(FakeImmutable.UnrelatedGuid);
            var property = typeof(FakeImmutable).GetProperty(propertyName);

            // Act

            var exception = Record.Exception(
                () => new DateTimePropertyHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{propertyName}' is not of type 'DateTime' nor 'Nullable<DateTime>'." +
                Environment.NewLine + "Parameter name: property",
                exception.Message
            );
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_NonNullValues_MemberData {
            get {
                foreach (var property in properties) {
                    yield return new object[] {
                        property.Name,
                        new DateTime(2016, 5, 9, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2016, 5, 9, 0, 0, 0, DateTimeKind.Utc)
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(2013, 5, 11, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2013, 5, 11, 0, 0, 0, DateTimeKind.Local)
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(2015, 5, 27, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2013, 5, 27, 0, 0, 0, DateTimeKind.Utc),
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_NonNullValues_MemberData))]
        public void ArePropertyValuesEqual_NonNullValues(
            String propertyName,
            DateTime left,
            DateTime right) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, left);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, right);

            // Act

            var handlerEquality = handler.ArePropertyValuesEqual(leftFake, rightFake);
            var nativeEquality = left.Equals(right);

            // Assert

            Assert.Equal(handlerEquality, nativeEquality);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_NullValues_MemberData {
            get {
                yield return new object[] { new DateTime(2016, 5, 9), null, false };
                yield return new object[] { null, new DateTime(2016, 5, 9), false };
                yield return new object[] { null, null, true };
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_NullValues_MemberData))]
        public void ArePropertyValuesEqual_NullValues(
            DateTime? left,
            DateTime? right,
            bool expectedAreEqual) {

            // Arrange

            var handler = this.CreateHandler(nameof(FakeImmutable.NullableProperty));
            var leftFake = this.NextFake().SetNullableProperty(left);
            var rightFake = this.NextFake().SetNullableProperty(right);

            // Act

            var actualAreEqual = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.Equal(expectedAreEqual, actualAreEqual);
        }

        public static IEnumerable<object[]> GetPropertyValueHashCode_NonNullValues_MemberData {
            get {
                foreach (var property in properties) {
                    yield return new object[] {
                        property.Name,
                        new DateTime(2016, 5, 9, 0, 0, 0, DateTimeKind.Utc)
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(2013, 5, 11, 0, 0, 0, DateTimeKind.Local)
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyValueHashCode_NonNullValues_MemberData))]
        public void GetPropertyValueHashCode_NonNullValues(
            String propertyName,
            DateTime value) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = this.NextFake().SetPropertyValue(propertyName, value);

            // Act

            var handlerHashCode = handler.GetPropertyValueHashCode(fake);
            var nativeHashCode = value.GetHashCode();

            // Assert

            Assert.Equal(handlerHashCode, nativeHashCode);
        }

        [Fact]
        public void GetPropertyValueHashCode_NullValues() {

            // Arrange

            var handler = this.CreateHandler(nameof(FakeImmutable.NullableProperty));
            var fake = this.NextFake().SetNullableProperty(null);

            // Act

            var hashCode = handler.GetPropertyValueHashCode(fake);

            // Assert

            Assert.Equal(37, hashCode);

            // 37 is the universal default hash code for "null" that is
            // defined in the abstract PropertyHandlerBase implementation.
        }

        public static IEnumerable<object[]> GetPropertyValueDisplayString_Iso8601_MemberData {
            get {
                foreach (var property in properties) {
                    yield return new object[] {
                        property.Name,
                        new DateTime(2013, 5, 11, 20, 22, 33, DateTimeKind.Utc),
                        @"^2013-05-11T20:22:33.0000000Z$"
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(2015, 5, 27, 5, 13, 16, DateTimeKind.Local),
                        @"^2015-05-27T05:13:16.0000000[-+]\d{2}:\d{2}$"
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(1984, 2, 2, 0, 59, 59, 222, DateTimeKind.Utc),
                        @"^1984-02-02T00:59:59.2220000Z$"
                    };

                    yield return new object[] {
                        property.Name,
                        new DateTime(1983, 12, 03, 11, 41, 22, 432, DateTimeKind.Unspecified),
                        @"^1983-12-03T11:41:22.4320000$"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyValueDisplayString_Iso8601_MemberData))]
        public void GetPropertyValueDisplayString_Iso8601(
            String propertyName,
            DateTime value,
            String expectedDisplayStringRegex) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = this.NextFake().SetPropertyValue(propertyName, value);

            // Act

            var actualDisplayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Matches(expectedDisplayStringRegex, actualDisplayString);
        }

        [Fact]
        public void GetPropertyValueDisplayString_NullIsNotFormatted() {

            // Arrange

            var handler = this.CreateHandler(nameof(FakeImmutable.NullableProperty));
            var fake = this.NextFake().SetNullableProperty(null);

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Equal("null", displayString);
        }

        protected override PropertyInfo NextValidPropertyInfo() {
            return
                properties
                    .Skip(random.Value.Next(0, properties.Count))
                    .First();
        }

        private IPropertyHandler CreateHandler(string propertyName) {
            return this.CreateHandler(typeof(FakeImmutable).GetProperty(propertyName));
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new DateTimePropertyHandler(property);
        }

        protected override object NextParent() {
            return this.NextFake();
        }

        private FakeImmutable NextFake() {
            return new FakeImmutable(
                Guid.NewGuid().ToString(),
                Guid.NewGuid(),
                new DateTime(random.Value.Next()),
                new DateTime(random.Value.Next())
            );
        }

        public sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public String StringProperty { get; }
            public Guid UnrelatedGuid { get; }
            public DateTime DateTimeProperty { get; }
            public DateTime? NullableProperty { get; }

            public FakeImmutable(
                String stringProperty = default(String),
                Guid unrelatedGuid = default(Guid),
                DateTime dateTimeProperty = default(DateTime),
                DateTime? nullableProperty = default(DateTime?)) {

                this.StringProperty = stringProperty;
                this.UnrelatedGuid = unrelatedGuid;
                this.DateTimeProperty = dateTimeProperty;
                this.NullableProperty = nullableProperty;
            }

            public FakeImmutable SetPropertyValue(String propertyName, object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

            public FakeImmutable SetDateTimeProperty(DateTime dateTimeProperty) {
                return this.SetPropertyValueImpl(nameof(DateTimeProperty), dateTimeProperty);
            }

            public FakeImmutable SetNullableProperty(DateTime? nullableProperty) {
                return this.SetPropertyValueImpl(nameof(NullableProperty), nullableProperty);
            }

        }

    }


}
