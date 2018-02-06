using System;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> to use for properties that are
    ///   of type <see cref="String" />. Other than guarding against <c>null</c>,
    ///   this implementation uses the native value, equality comparison, and hash code
    ///   generation for strings unless that behavior has been overridden with a
    ///   specific <see cref="StringComparer" /> implementation during instantiation.
    /// </summary>
    public sealed class StringPropertyHandler : PropertyHandlerBase<string> {

        private static StringComparer defaultComparer { get; }

        static StringPropertyHandler() {
            // If the consumer doesn't include a custom StringComparer
            // implementation, the least surprising default for a value
            // object is a case-sensitive ordinal-based comparison.

            defaultComparer = StringComparer.Ordinal;
        }

        private StringComparer comparer { get; }

        /// <summary>
        ///   Creates an instance of <see cref="StringPropertyHandler" /> that will use
        ///   the native equality comparison and hash code generation for the provided
        ///   <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that is of type
        ///   <see cref="String" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="property" /> does not have a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="String" />.
        /// </exception>
        public StringPropertyHandler(PropertyInfo property) :
            this(property, defaultComparer) {}

        /// <summary>
        ///   Creates an instance of <see cref="StringPropertyHandler" /> that will use
        ///   the provided <paramref name="comparer" /> to perform equality comparison
        ///   and hash code generation for the provided <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that is of type
        ///   <see cref="String" />.
        /// </param>
        /// <param name="comparer">
        ///   The <see cref="StringComparer" /> that will be used to perform equality
        ///   comparisons and hash code generation for values extracted from
        ///   <paramref name="property" /> for the relevant value objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> or <paramref name="comparer" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="property" /> does not have a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="String" />.
        /// </exception>
        public StringPropertyHandler(PropertyInfo property, StringComparer comparer) :
            base(property) {

            if (comparer == null) {
                throw new ArgumentNullException(nameof(comparer));
            }

            this.comparer = comparer;
        }

        /// <summary>
        ///   Determines whether two property values are considered equal by using the
        ///   <see cref="StringComparer" /> selected during the instantiaion of the
        ///   <see cref="StringPropertyHandler" />.
        /// </summary>
        protected override bool ArePropertyValuesEqualImpl(
            string leftPropertyValue,
            string rightPropertyValue) {

            return this.comparer.Equals(leftPropertyValue, rightPropertyValue);
        }

        /// <summary>
        ///   Generates a hash code using the <see cref="StringComparer" /> selected
        ///   during the instantiaion of the <see cref="StringPropertyHandler" />.
        /// </summary>
        protected override int GetPropertyValueHashCodeImpl(string propertyValue) {
            return this.comparer.GetHashCode(propertyValue);
        }

    }

}
