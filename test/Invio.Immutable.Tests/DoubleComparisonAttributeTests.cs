using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DoubleComparisonAttributeTests  {

        [Theory]
        [InlineData(Int32.MinValue, PrecisionStyle.SignificantFigures)]
        [InlineData(Int32.MinValue, PrecisionStyle.DecimalPlaces)]
        [InlineData(-1, PrecisionStyle.SignificantFigures)]
        [InlineData(-1, PrecisionStyle.DecimalPlaces)]
        [InlineData(0, PrecisionStyle.SignificantFigures)]
        [InlineData(16, PrecisionStyle.DecimalPlaces)]
        [InlineData(16, PrecisionStyle.SignificantFigures)]
        public void Constructor_PrecisionOutOfRange(Int32 precision, PrecisionStyle precisionStyle) {
            var exception = Record.Exception(
                () => new DoubleComparisonAttribute(precision, precisionStyle)
            );

            var outOfRangeException = Assert.IsType<ArgumentOutOfRangeException>(exception);
            Assert.Equal("precision", outOfRangeException.ParamName);
            Assert.Equal(precision, outOfRangeException.ActualValue);
        }

        [Theory]
        [InlineData(1, PrecisionStyle.SignificantFigures)]
        [InlineData(0, PrecisionStyle.DecimalPlaces)]
        [InlineData(4, PrecisionStyle.SignificantFigures)]
        [InlineData(4, PrecisionStyle.DecimalPlaces)]
        [InlineData(15, PrecisionStyle.SignificantFigures)]
        [InlineData(15, PrecisionStyle.DecimalPlaces)]
        public void Constructor_SupportedPrecisions(Int32 precision, PrecisionStyle precisionStyle) {

            // Act

            var attribute = new DoubleComparisonAttribute(precision, precisionStyle);

            // Assert

            Assert.Equal(precision, attribute.Precision);
            Assert.Equal(precisionStyle, attribute.PrecisionStyle);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(15)]
        public void Constructor_DefaultDecimalPlaces(Int32 precision) {

            // Act

            var attribute = new DoubleComparisonAttribute(precision);

            // Assert

            Assert.Equal(precision, attribute.Precision);
            Assert.Equal(PrecisionStyle.DecimalPlaces, attribute.PrecisionStyle);
        }

    }

}
