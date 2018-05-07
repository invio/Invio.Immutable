using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class StringComparisonAttributeTests  {

        [Fact]
        public void Constructor_UnsupportedStringComparison() {

            // Arrange

            var comparison = (StringComparison)99;

            // Act

            var exception = Record.Exception(
                () => new StringComparisonAttribute(comparison)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The {nameof(comparison)} provided, '99', " +
                $"is not defined on the {nameof(StringComparison)} enum." +
                Environment.NewLine + "Parameter name: comparison",
                exception.Message
            );
        }

        [Theory]
        [InlineData(StringComparison.Ordinal)]
        [InlineData(StringComparison.OrdinalIgnoreCase)]
        [InlineData(StringComparison.InvariantCulture)]
        [InlineData(StringComparison.InvariantCultureIgnoreCase)]
        [InlineData(StringComparison.CurrentCulture)]
        [InlineData(StringComparison.CurrentCultureIgnoreCase)]
        public void Constructor_SupportedStringComparisons(StringComparison comparison) {

            // Act

            var attribute = new StringComparisonAttribute(comparison);

            // Assert

            Assert.Equal(comparison, attribute.Comparison);
        }

    }

}
