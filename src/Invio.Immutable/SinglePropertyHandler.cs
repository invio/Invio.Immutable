using System;
using System.Reflection;

namespace Invio.Immutable {
    /// <summary>
    ///   Performs limited precision comparisons and hash code generation for properties of type
    ///   <see cref="Single" />.
    /// </summary>
    public sealed class SinglePropertyHandler : PropertyHandlerBase<Single> {
        /// <summary>
        ///   The level of precision to use when comparing floating point values.
        /// </summary>
        public Int32 Precision { get; }

        /// <summary>
        ///   Indicates whether precision should be interpreted as the number of significant
        ///   figures, or as the number of digits to the right of the decimal place.
        /// </summary>
        public PrecisionStyle PrecisionStyle { get; }

        /// <summary>
        ///   Create a new instance of <see cref="SinglePropertyHandler" /> with the specified
        ///   precision settings.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> for a property that is of type <see cref="Single" />.
        /// </param>
        /// <param name="precision">See: <see cref="Precision" />.</param>
        /// <param name="precisionStyle">See: <see cref="PrecisionStyle" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Thrown when <paramref name="precisionStyle" /> is
        ///   <see cref="Invio.Immutable.PrecisionStyle.DecimalPlaces" /> and
        ///   <paramref name="precision" /> is either less than <c>0</c> or greater than <c>15</c>,
        ///   or when <paramref name="precisionStyle" /> is
        ///   <see cref="Invio.Immutable.PrecisionStyle.DecimalPlaces" /> and
        ///   <paramref name="precision" /> less than <c>1</c> or greater than <c>15</c>.
        /// </exception>
        public SinglePropertyHandler(
            PropertyInfo property,
            Int32 precision,
            PrecisionStyle precisionStyle = PrecisionStyle.DecimalPlaces) :
            base(property) {

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

        /// <summary>
        ///   Calculates if two floating point values are equal out to a certain precision or number
        ///   of significant digits. See <see cref="Precision" /> and
        ///   <see cref="PrecisionStyle" />.
        /// </summary>
        /// <inheritdoc />
        protected override Boolean ArePropertyValuesEqualImpl(
            Single leftPropertyValue,
            Single rightPropertyValue) {

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (this.PrecisionStyle == PrecisionStyle.SignificantFigures) {
                return RoundToSignificantDigits(leftPropertyValue, this.Precision) ==
                    RoundToSignificantDigits(rightPropertyValue, this.Precision);
            } else {
                return Math.Round(leftPropertyValue, this.Precision) ==
                    Math.Round(rightPropertyValue, this.Precision);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///   Calculates a consistent hash code for a floating point number based on a certain
        ///   precision or number of significant digits. See <see cref="Precision" /> and
        ///   <see cref="PrecisionStyle" />.
        /// </summary>
        /// <inheritdoc />
        protected override Int32 GetPropertyValueHashCodeImpl(Single propertyValue) {
            return this.PrecisionStyle == PrecisionStyle.SignificantFigures ?
                RoundToSignificantDigits(propertyValue, this.Precision).GetHashCode() :
                Math.Round(propertyValue, this.Precision).GetHashCode();
        }

        private static Single RoundToSignificantDigits(Single value, Int32 precision){
            var scale = (Single)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * (Single)Math.Round(value / scale, precision);
        }
    }
}