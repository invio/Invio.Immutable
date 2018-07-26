using System;

namespace Invio.Immutable {
    /// <summary>
    ///   Indicates that the decorated property should be compared based on a specified level of
    ///   precision.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DoubleComparisonAttribute : Attribute {
        /// <summary>
        ///   The level of precision to use when comparing floating point values.
        /// </summary>
        public Int32 Precision { get; }

        /// <summary>
        ///   Indicates whether precision should be interpreted as the number of significant
        ///   figures, or as the number of digits to the right of the decimal place.
        /// </summary>
        public PrecisionStyle PrecisionStyle { get; set; }

        /// <summary>
        ///   Creates a new instance of DoubleComparisonAttribute.
        /// </summary>
        /// <param name="precision">See: <see cref="Precision" />.</param>
        /// <param name="precisionStyle">See: <see cref="PrecisionStyle" /></param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Thrown when <paramref name="precisionStyle" /> is
        ///   <see cref="Invio.Immutable.PrecisionStyle.DecimalPlaces" /> and
        ///   <paramref name="precision" /> is either less than <c>0</c> or greater than <c>15</c>,
        ///   or when <paramref name="precisionStyle" /> is
        ///   <see cref="Invio.Immutable.PrecisionStyle.DecimalPlaces" /> and
        ///   <paramref name="precision" /> less than <c>1</c> or greater than <c>15</c>.
        /// </exception>
        public DoubleComparisonAttribute(
            Int32 precision,
            PrecisionStyle precisionStyle = PrecisionStyle.DecimalPlaces) {
            if (precisionStyle == PrecisionStyle.SignificantFigures && precision < 1) {
                throw new ArgumentOutOfRangeException(
                    nameof(precision),
                    precision,
                    $"When {nameof(precisionStyle)} is " +
                    $"{nameof(PrecisionStyle.SignificantFigures)}, the value of " +
                    $"{nameof(precision)} must be greater than or equal to 1."
                );
            } else if (precision < 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(precision),
                    precision,
                    $"The value of {nameof(precision)} must be greater than or equal to 0."
                );
            } else if (precision > 15) {
                throw new ArgumentOutOfRangeException(
                    nameof(precision),
                    precision,
                    $"The value of {nameof(precision)} must be less than or equal to 15."
                );
            }

            this.Precision = precision;
            this.PrecisionStyle = precisionStyle;
        }
    }
}