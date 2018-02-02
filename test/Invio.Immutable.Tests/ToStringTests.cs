using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class ToStringImmutableTests {

        [Fact]
        public void ToString_NoProperties() {

            // Arrange

            var propertyless = new Propertyless();

            // Act

            var output = propertyless.ToString();

            // Assert

            Assert.Equal("{}", output);
        }

        [Fact]
        public void ToString_OneProperty_Null() {

            // Arrange

            var nulled = new OneProperty<String>(null);

            // Act

            var output = nulled.ToString();

            // Assert

            Assert.Equal("{ Property: null }", output);
        }

        public static IEnumerable<object[]> ToString_OneProperty_MemberData { get;} =
            ImmutableList.Create<object[]>(
                new object[] { 42, 42.ToString() },
                new object[] { 42.2, 42.2.ToString() },
                new object[] { "foo", "\"foo\"" },
                new object[] { new DateTime(2016, 1, 1), "2016-01-01T00:00:00.0000000" }
            );

        [Theory]
        [MemberData(nameof(ToString_OneProperty_MemberData))]
        public void ToString_OneProperty<TProperty>(TProperty value, string expectedValue) {

            // Arrange

            var oneProperty = new OneProperty<TProperty>(value);

            // Act

            var actual = oneProperty.ToString();

            // Assert

            Assert.Equal("{ Property: " + expectedValue + " }", actual);
        }

        [Fact]
        public void ToString_MultipleProperties() {

            // Arrange

            var timestamp = new DateTime(2016, 3, 17, 12, 30, 55, 111);
            var nullableTimestamp = new DateTime(2006, 7, 22, 15, 30, 0); // <3
            var multiple = new MultipleProperties("foo", 42, timestamp, nullableTimestamp);

            // Act

            var output = multiple.ToString();

            // Assert

            Assert.Equal(
                "{ " +
                "StringProperty: \"foo\", " +
                "IntProperty: 42, " +
                "DateTimeProperty: 2016-03-17T12:30:55.1110000, " +
                "NullableDateTimeProperty: 2006-07-22T15:30:00.0000000 }",
                output
            );
        }

        private class Propertyless : ImmutableBase<Propertyless> {}

        private class OneProperty<TProperty> : ImmutableBase<OneProperty<TProperty>> {

            public TProperty Property { get; }

            public OneProperty(TProperty property) {
                this.Property = property;
            }

        }

        private class MultipleProperties : ImmutableBase<MultipleProperties> {

            public String StringProperty { get; }
            public int IntProperty { get; }
            public DateTime DateTimeProperty { get; }
            public Nullable<DateTime> NullableDateTimeProperty { get; }

            public MultipleProperties(
                string stringProperty,
                int intProperty,
                DateTime dateTimeProperty,
                DateTime? nullableDateTimeProperty) {

                this.StringProperty = stringProperty;
                this.IntProperty = intProperty;
                this.DateTimeProperty = dateTimeProperty;
                this.NullableDateTimeProperty = nullableDateTimeProperty;
            }

        }


    }

}
