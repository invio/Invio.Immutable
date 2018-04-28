using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DefaultStringFormatterTests {

        [Fact]
        public void Format_Null() {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var output = formatter.Format(null);

            // Assert

            Assert.Equal("null", output);
        }

        public static IEnumerable<object[]> Format_DateTime_MemberData {
            get {
                yield return new object[] {
                    new DateTime(2013, 5, 11, 20, 22, 33, DateTimeKind.Utc),
                    @"^2013-05-11T20:22:33.0000000Z$"
                };

                yield return new object[] {
                    new DateTime(2015, 5, 27, 5, 13, 16, DateTimeKind.Local),
                    @"^2015-05-27T05:13:16.0000000[-+]\d{2}:\d{2}$"
                };

                yield return new object[] {
                    new DateTime(1984, 2, 2, 0, 59, 59, 222, DateTimeKind.Utc),
                    @"^1984-02-02T00:59:59.2220000Z$"
                };

                yield return new object[] {
                    new DateTime(1983, 12, 03, 11, 41, 22, 432, DateTimeKind.Unspecified),
                    @"^1983-12-03T11:41:22.4320000$"
                };
            }
        }

        [Theory]
        [MemberData(nameof(Format_DateTime_MemberData))]
        public void Format_DateTime(DateTime value, String expected) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var actual = formatter.Format(value);

            // Assert

            Assert.Matches(expected, actual);
        }

        public static IEnumerable<object[]> Format_NullableDateTime_MemberData {
            get {
                yield return new object[] {
                    new DateTime(2013, 5, 11, 20, 22, 33, DateTimeKind.Utc),
                    @"^2013-05-11T20:22:33.0000000Z$"
                };

                yield return new object[] {
                    new DateTime(2015, 5, 27, 5, 13, 16, DateTimeKind.Local),
                    @"^2015-05-27T05:13:16.0000000[-+]\d{2}:\d{2}$"
                };

                yield return new object[] {
                    new DateTime(1984, 2, 2, 0, 59, 59, 222, DateTimeKind.Utc),
                    @"^1984-02-02T00:59:59.2220000Z$"
                };

                yield return new object[] {
                    new DateTime(1983, 12, 03, 11, 41, 22, 432, DateTimeKind.Unspecified),
                    @"^1983-12-03T11:41:22.4320000$"
                };

                yield return new object[] { null, "^null$" };
            }
        }

        [Theory]
        [MemberData(nameof(Format_NullableDateTime_MemberData))]
        public void Format_NullableDateTime(DateTime? value, String expected) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var actual = formatter.Format(value);

            // Assert

            Assert.Matches(expected, actual);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("BAR")]
        [InlineData("")]
        [InlineData(" ")]
        public void Format_String(String value) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var output = formatter.Format(value);

            // Assert

            Assert.Equal(String.Concat("\"", value, "\""), output);
        }

        public static IEnumerable<object[]> Format_Enums_MemberData {
            get {
                yield return new object[] { DateTimeKind.Utc, "Utc" };
                yield return new object[] { (DateTimeKind)10, "10" };
                yield return new object[] {
                    BindingFlags.Public | BindingFlags.Instance,
                    "Instance, Public"
                };
                yield return new object[] { (BindingFlags)Int32.MaxValue, "2147483647" };
            }
        }

        [Theory]
        [MemberData(nameof(Format_Enums_MemberData))]
        public void Format_Enums<TEnum>(TEnum value, String expected) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var actual = formatter.Format(value);

            // Assert

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> Format_IEnumerable_Empty_MemberData {
            get {
                yield return new object[] { ImmutableHashSet<Guid>.Empty };
                yield return new object[] { new object[0] };
                yield return new object[] { new List<string>() };
            }
        }

        [Theory]
        [MemberData(nameof(Format_IEnumerable_Empty_MemberData))]
        public void Format_IEnumerable_Empty(IEnumerable value) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var output = formatter.Format(value);

            // Assert

            Assert.Equal("[]", output);
        }

        public static IEnumerable<object[]> Format_IEnumerable_Many_MemberData { get; } =
            ImmutableList<IEnumerable>
                .Empty
                .Add(ImmutableList.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
                .Add(new string[] { null, "FOO", "bar", "biz", "gah", String.Empty })
                .Add(ImmutableList.Create<Guid?>(Guid.NewGuid(), null, Guid.NewGuid()))
                .Add(new object[] { "foo", Guid.NewGuid(), null, 35m, DateTime.UtcNow })
                .Select(enumerable => new object[] { enumerable })
                .ToImmutableList();

        [Theory]
        [MemberData(nameof(Format_IEnumerable_Many_MemberData))]
        public void Format_IEnumerable_Many(IEnumerable value) {

            // Arrange

            var formatter = new DefaultStringFormatter();

            // Act

            var output = formatter.Format(value);

            // Assert

            Assert.StartsWith("[ ", output);
            Assert.Contains(", ", output);
            Assert.EndsWith(" ]", output);

            foreach (var item in value) {
                Assert.Contains(formatter.Format(item), output);
            }
        }

    }

}
