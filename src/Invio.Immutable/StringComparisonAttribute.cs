using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Invio.Immutable {

    /// <summary>
    ///   An <see cref="Attribute" /> to indicate that a decorated property of type
    ///   <see cref="String" />, or the <see cref="String" /> properties on a decorated
    ///   class, should use a specific <see cref="StringComparison" /> when calculating
    ///   hash codes and determining equality for their values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If placed on a class, only the properties of type <see cref="String" />
    ///     will have their hash code generation and equality comparisons be affected.
    ///   </para>
    ///   <para>
    ///     If this is placed on a non-<see cref="String" /> property, or on a class
    ///     that lacks any <see cref="String" /> properties, it will do nothing.
    ///   </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class StringComparisonAttribute : Attribute {

        /// <summary>
        ///   The <see cref="StringComparison" /> that will be used when generating
        ///   hash codes or performing equality comparisons for the values of
        ///   properties decorated with this <see cref="Attribute" />.
        /// </summary>
        public StringComparison Comparison { get; }

        /// <summary>
        ///   Creates an instance of <see cref="StringComparisonAttribute" /> that uses
        ///   the <paramref name="comparison" /> parameter to define how an instance
        ///   of <see cref="ImmutableBase{TImmutable}" /> will generate hash codes and
        ///   perform checks of equality on properties of type <see cref="String" />.
        /// </summary>
        /// <param name="comparison">
        ///   The <see cref="StringComparison" /> that will be used when comparison
        ///   properties of type <see cref="String" /> that exists on instances of
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   Thrown when the <paramref name="comparison" /> provided does not map to
        ///   a known implementation of <see cref="StringComparer" />.
        /// </exception>
        public StringComparisonAttribute(StringComparison comparison) {
            if (!Enum.IsDefined(typeof(StringComparison), comparison)) {
                throw new ArgumentException(
                    $"The {nameof(comparison)} provided, '{comparison:G}', " +
                    $"is not defined on the {nameof(StringComparison)} enum.",
                    nameof(comparison)
                );
            }

            this.Comparison = comparison;
        }

    }

}
