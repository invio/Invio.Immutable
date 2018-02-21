using System;
using System.Collections;
using System.Text;

namespace Invio.Immutable {

    /// <summary>
    ///   An implementaton that "does the needful" for the most common types
    ///   found on <see cref="ImmutableBase{TImmutable}" /> properties.
    /// </summary>
    public sealed class DefaultStringFormatter : IStringFormatter {

        /// <summary>
        ///   Anytime a <c>null</c> value needs to be displayed as a string,
        ///   this is the string used to inform the user that the value is,
        ///   in fact, <c>null</c>.
        /// </summary>
        private const string nullAsString = "null";

        /// <summary>
        ///   Renders the <paramref name="value" /> as a <see cref="String" />
        ///   in a user-friendly way.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     If the <paramref name="value" /> is equal to <c>null</c>,
        ///     the <see cref="String" /> <c>"null"</c> is returned.
        ///   </para>
        ///   <para>
        ///     If there is not a type-specific way to render the
        ///     <paramref name="value" /> as a string, the default
        ///     <see cref="Object.ToString()" /> implementation for
        ///     the value is used.
        ///   </para>
        /// </remarks>
        /// <param name="value">
        ///   Any value that the consumer wishes to render into a
        ///   user-friendly <see cref="String" /> representation.
        /// </param>
        /// <returns>
        ///   A non-null <see cref="String" /> that can be shown to
        ///   a user who wishes to understand its value.
        /// </returns>
        public string Format(object value) {
            if (value == null) {
                return nullAsString;
            }

            if (value is DateTime) {
                return $"{value:o}";
            }

            if (value is String) {
                return $@"""{value}""";
            }

            if (value is IEnumerable enumerable) {
                return this.FormatEnumerable(enumerable);
            }

            return value.ToString();
        }

        private string FormatEnumerable(IEnumerable enumerable) {
            var builder = new StringBuilder("[");
            var any = false;

            foreach (var item in enumerable) {
                if (any) {
                    builder.Append(",");
                }

                builder.Append(" ");
                builder.Append(this.Format(item));

                any = true;
            }

            if (any) {
                builder.Append(" ");
            }

            builder.Append("]");

            return builder.ToString();
        }

    }

}
