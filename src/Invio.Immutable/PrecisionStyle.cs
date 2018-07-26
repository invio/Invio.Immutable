namespace Invio.Immutable {
    /// <summary>
    ///   Enumeration of options for limiting the precision of floating point numbers.
    /// </summary>
    public enum PrecisionStyle {
        /// <summary>
        ///   Limits precision by limiting the number of digits to the right of the decimal place.
        /// </summary>
        DecimalPlaces = 0,

        /// <summary>
        ///   Limits precision by limiting the number of digits starting from first non zero digit.
        /// </summary>
        /// <remarks>
        ///   For example 123000, 1.23, and 0.0000123 all have 3 significant figures.
        /// </remarks>
        SignificantFigures
    }
}