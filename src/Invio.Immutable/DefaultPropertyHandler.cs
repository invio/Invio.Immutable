using System;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> for use when there is no special
    ///   equality, hash code generation, or string representation for a particular type.
    ///   Other than guarding against <c>null</c>, this implementation uses the native
    ///   value, equality comparison, hash code generation and string representation
    ///   for the <see cref="PropertyInfo.PropertyType" /> found on the injected
    ///   <see cref="PropertyInfo" />.
    /// </summary>
    public sealed class DefaultPropertyHandler : IPropertyHandler {

        /// <summary>
        ///   The <see cref="MemberInfo.Name" /> of the injected <see cref="PropertyInfo" />
        ///   that can found on the value object for which this particular
        ///   <see cref="DefaultPropertyHandler" /> has been created.
        /// </summary>
        /// <remarks>
        ///   This will never be <c>null</c>.
        /// </remarks>
        public String PropertyName { get; }

        /// <summary>
        ///   The <see cref="PropertyInfo.PropertyType" /> of the injected
        ///   <see cref="PropertyInfo" />  that can found on the value object for which
        ///   this particular <see cref="DefaultPropertyHandler" /> has been created.
        /// </summary>
        /// <remarks>
        ///   This will never be <c>null</c>.
        /// </remarks>
        public Type PropertyType { get; }

        // This is made lazy because, according to the documentation made available in
        // Invio.Reflection.Extensions, creating these caching delegates is expensive.
        private Lazy<Func<object, object>> getter { get; }

        /// <summary>
        ///   Creates an instance of <see cref="DefaultPropertyHandler" /> that
        ///   will use the native equality comparion, hash code generation and
        ///   string representation for the provided <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that lacks any special
        ///   equality comparison, hash code generation, or string representation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        public DefaultPropertyHandler(PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }

            this.PropertyName = property.Name;
            this.PropertyType = property.PropertyType;

            this.getter = new Lazy<Func<object, object>>(
                () => property.CreateGetter()
            );
        }

        /// <summary>
        ///   Determines whether two value objects that contain the injected
        ///   <see cref="PropertyInfo" /> possess the same value when using the native
        ///   <see cref="Object.Equals(object)" /> implementation on the property values.
        /// </summary>
        /// <remarks>
        ///   If either property value is a reference type, the property values
        ///   are considered equal if they are both <c>null</c>, and unequal if one is
        ///   null and the other is not.
        /// </remarks>
        /// <param name="leftParent">
        ///   A value object that contains the injected <see cref="PropertyInfo" />.
        /// </param>
        /// <param name="rightParent">
        ///   A value object that contains the injected <see cref="PropertyInfo" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="leftParent" /> or <paramref name="rightParent" />
        ///   is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when neither <paramref name="leftParent" /> nor
        ///   <paramref name="rightParent" /> contains the injected <see cref="PropertyInfo" />.
        /// </exception>
        /// <returns>
        ///   Wheather or not the values for the injected <see cref="PropertyInfo" /> are
        ///   equal for the two provided value objects that contain them.
        /// </returns>
        public bool ArePropertyValuesEqual(object leftParent, object rightParent) {
            if (leftParent == null) {
                throw new ArgumentNullException(nameof(leftParent));
            } else if (rightParent == null) {
                throw new ArgumentNullException(nameof(rightParent));
            }

            var leftPropertyValue =
                this.GetPropertyValueImplOrThrow(leftParent, nameof(leftParent));
            var rightPropertyValue =
                this.GetPropertyValueImplOrThrow(rightParent, nameof(rightParent));

            if (Object.ReferenceEquals(leftPropertyValue, null)) {
                return Object.ReferenceEquals(rightPropertyValue, null);
            }

            if (Object.ReferenceEquals(rightPropertyValue, null)) {
                return false;
            }

            return leftPropertyValue.Equals(rightPropertyValue);
        }

        /// <summary>
        ///   Generates a hash code for the injected <see cref="PropertyInfo" /> using
        ///   the value stored on the value object provided via <paramref name="parent" />.
        ///   This implementation uses the native <see cref="Object.GetHashCode" />
        ///   method to generate a hash code.
        /// </summary>
        /// <remarks>
        ///   If the property value is <c>null</c>, attempting to use the native hash
        ///   code generation would result in a <see cref="NullReferenceException" />.
        ///   To avoid this edge case, a constant of <c>37</c> is used instead whenever
        ///   the property's value is <c>null</c>.
        /// </remarks>
        /// <param name="parent">
        ///   A value object that contains the injected <see cref="PropertyInfo" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain
        ///   the injected <see cref="PropertyInfo" />.
        /// </exception>
        /// <returns>
        ///   An appropriate hash code for the value of the injected <see cref="PropertyInfo" />
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        public int GetPropertyValueHashCode(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            var propertyValue = this.GetPropertyValueImplOrThrow(parent, nameof(parent));

            if (propertyValue == null) {
                // Arbitrary (but prime!) number for null.

                return 37;
            }

            return propertyValue.GetHashCode();
        }

        /// <summary>
        ///   Gets the value for the injected <see cref="PropertyInfo" /> off of the
        ///   provided <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">
        ///   A value object that ocntains the injected <see cref="PropertyInfo" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain
        ///   the injected <see cref="PropertyInfo" />.
        /// </exception>
        /// <returns>
        ///   The value of the injected <see cref="PropertyInfo" />
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        public object GetPropertyValue(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            return this.GetPropertyValueImplOrThrow(parent, nameof(parent));
        }

        private object GetPropertyValueImplOrThrow(object parent, string propertyName) {
            try {
                return this.getter.Value(parent);
            } catch (InvalidCastException exception) {
                throw new ArgumentException(
                    $"The value object provided is of type {parent.GetType().Name}, " +
                    $"which does not contains the {this.PropertyName} property.",
                    propertyName,
                    exception
                );
            }
        }

        /// <summary>
        ///   Generates a <see cref="String" /> representation for the injected
        ///   <see cref="PropertyInfo" /> using the value stored on the value object
        ///   provided via <paramref name="parent" />. This implementation uses the native
        ///   <see cref="Object.ToString" /> method to generate the <see cref="String" />.
        /// </summary>
        /// <remarks>
        ///   If the property value is <c>null</c>, attempting to use the native
        ///   string representation would result in a <see cref="NullReferenceException" />.
        ///   To avoid this edge case, the string <c>"null"</c> is used whenever the
        ///   property's value is <c>null</c>.
        /// </remarks>
        /// <param name="parent">
        ///   A value object that contains the injected <see cref="PropertyInfo" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain
        ///   the injected <see cref="PropertyInfo" />.
        /// </exception>
        /// <returns>
        ///   An appropriate string representation for the value of the injected
        ///   <see cref="PropertyInfo" /> found on the value object provided via
        ///   <paramref name="parent" />.
        /// </returns>
        public String GetPropertyValueDisplayString(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            var propertyValue = this.GetPropertyValueImplOrThrow(parent, nameof(parent));

            if (propertyValue == null) {
                return "null";
            }

            return propertyValue.ToString();
        }

    }

}
