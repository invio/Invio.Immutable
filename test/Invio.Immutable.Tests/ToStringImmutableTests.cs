using System;
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
        public void ToString_OneProperty_DateTime() {

            // Arrange

            var timestamp = new DateTime(2016, 1, 1);
            var dated = new OneProperty<DateTime>(timestamp);

            // Act

            var output = dated.ToString();

            // Assert

            Assert.Equal("{ Property: 2016-01-01T00:00:00.0000000 }", output);
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

        [Theory]
        [InlineData(42)]
        [InlineData(42.2)]
        [InlineData("foo")]
        public void ToString_OneProperty<TProperty>(TProperty value) {

            // Arrange

            var oneProperty = new OneProperty<TProperty>(value);

            // Act

            var output = oneProperty.ToString();

            // Assert

            Assert.Equal($"{{ Property: {value} }}", output);
        }

        [Fact]
        public void ToString_MultipleProperties() {

            // Arrange

            var timestamp = new DateTime(2016, 3, 17, 12, 30, 55, 111);
            var multiple = new MultipleProperties("foo", 42, timestamp);

            // Act

            var output = multiple.ToString();

            // Assert

            Assert.Equal(
                "{ " +
                "StringProperty: foo, " +
                "IntProperty: 42, " +
                "DateTimeProperty: 2016-03-17T12:30:55.1110000 }",
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

            public MultipleProperties(
                string stringProperty,
                int intProperty,
                DateTime dateTimeProperty) {

                this.StringProperty = stringProperty;
                this.IntProperty = intProperty;
                this.DateTimeProperty = dateTimeProperty;
            }

        }


    }

}
