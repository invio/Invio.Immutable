using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> to use for properties that are of
    ///   type <see cref="DateTime" /> or <see cref="Nullable{DateTime}" />. Other than
    ///   guarding against <c>null</c>, this implementation uses the native equality
    ///   comparison and hash code generation for values of type <see cref="DateTime" />.
    ///   In all situations, however, it alters the string representations of the property
    ///   values such that non-null values are presented in the ISO-8601 timestamp format.
    /// </summary>
    public sealed class DateTimePropertyHandler : PropertyHandlerBase<object> {

        /// <summary>
        ///   Creates an instance of <see cref="DateTimePropertyHandler" /> that will use
        ///   the native equality comparison and hash code generation for the provided
        ///   <see cref="PropertyInfo" />. However, the external string representation
        ///   will be encapsulated in quotation marks for non-null values.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that is of type
        ///   <see cref="DateTime" /> or <see cref="Nullable{DateTime}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="property" /> does not have a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="DateTime" />
        ///   nor of type <see cref="Nullable{DateTime}" />.
        /// </exception>
        public DateTimePropertyHandler(PropertyInfo property) : base(property) {
            var propertyType = property.PropertyType;

            if (propertyType != typeof(DateTime) && propertyType != typeof(DateTime?)) {
                throw new ArgumentException(
                    $"The '{property.Name}' is not of type '{nameof(DateTime)}' " +
                    $"nor 'Nullable<{nameof(DateTime)}>'.",
                    nameof(property)
                );
            }
        }

        /// <summary>
        ///   Generates a <see cref="String" /> representation for the provided
        ///   <paramref name="propertyValue" />, which will always be a non-null
        ///   value of type <see cref="DateTime" />. The output will be in the
        ///   ISO-8601 internationally standardized format for timestamps.
        /// </summary>
        /// <remarks>
        ///   Since <see cref="PropertyHandlerBase{TProperty}" /> handles the
        ///   <c>null</c> case, the consumer can be sure that all values of
        ///   <paramref name="propertyValue" /> will be non-null values,
        ///   regardless the type on the <see cref="PropertyInfo" /> that was
        ///   injected into the constructor.
        /// </remarks>
        /// <param name="propertyValue">
        ///   A non-null property value extracted from a value object that will
        ///   always be of type <see cref="DateTime" />.
        /// </param>
        /// <returns>
        ///   A <see cref="String" /> representation of the <see cref="DateTime" />
        ///   in the ISO-8601 timestamp format,
        /// </returns>
        protected override String GetPropertyValueDisplayStringImpl(object propertyValue) {
            return $"{propertyValue:o}";
        }

    }

}
