using System;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the base <see cref="IPropertyHandler" /> for all implementations
    ///   to guarantee consistent validation and exception messaging. It defers
    ///   all appropriate implementation-specific behavior to the inheriting class.
    /// </summary>
    /// <typeparam name="TProperty">
    ///   A type that can reliably be assigned to values that are stored in the
    ///   injected property.
    /// </typeparam>
    public abstract class PropertyHandlerBase<TProperty> : IPropertyHandler {

        /// <summary>
        ///   Anytime a <c>null</c> needs to be displayed as a string, this
        ///   is the string used to inform the user that the value is null.
        /// </summary>
        protected const string nullAsString = "null";

        /// <summary>
        ///   The name of the abstracted property that exists on the value object.
        /// </summary>
        public virtual String PropertyName { get; }

        /// <summary>
        ///   The type of the abstracted property that exists on the value object.
        /// </summary>
        public virtual Type PropertyType { get; }

        // This is made lazy because, according to the documentation made available in
        // Invio.Reflection.Extensions, creating these caching delegates is expensive.
        private Lazy<Func<object, object>> getter { get; }

        /// <summary>
        ///   Creates an instance of <see cref="PropertyHandlerBase{TProperty}" /> that
        ///   will use consistent null and type checking on the value objects being
        ///   provided to its methods.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that will potentially
        ///   have its native equality comparison, hash code generation, and string
        ///   representation by the inheriting class.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        protected PropertyHandlerBase(PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            } else if (!property.PropertyType.IsDerivativeOf(typeof(TProperty))) {
                throw new ArgumentException(
                    $"The '{property.Name}' property is not of type " +
                    $"'{typeof(TProperty).GetNameWithGenericParameters()}'.",
                    nameof(property)
                );
            }

            this.PropertyName = property.Name;
            this.PropertyType = property.PropertyType;

            this.getter = new Lazy<Func<object, object>>(
                () => property.CreateGetter()
            );
        }

        /// <summary>
        ///   Determines whether the values for the abstracted property are equal
        ///   on two value objects that contain them.
        /// </summary>
        /// <param name="leftParent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <param name="rightParent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="leftParent" /> or
        ///   <paramref name="rightParent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when either <paramref name="leftParent" /> or
        ///   <paramref name="rightParent" /> does not contains the abstracted property.
        /// </exception>
        /// <returns>
        ///   Whether or not the values for the abstracted property are
        ///   equal for the two value objects provided that contain them.
        /// </returns>
        public virtual bool ArePropertyValuesEqual(object leftParent, object rightParent) {
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

            return this.ArePropertyValuesEqualImpl(leftPropertyValue, rightPropertyValue);
        }

        /// <summary>
        ///   An implementation specific equality comparison that is deferred to the
        ///   class that inherits from this <see cref="PropertyHandlerBase{TProperty}" />
        ///   implementation. By default, this uses the native
        ///   <see cref="Object.Equals(object)" /> implementation.
        /// </summary>
        /// <param name="leftPropertyValue">
        ///   A non-null property value extracted from a value object that contains the
        ///   <see cref="PropertyInfo" /> injected into the constructor.
        /// </param>
        /// <param name="rightPropertyValue">
        ///   A non-null property value extracted from a value object that contains the
        ///   <see cref="PropertyInfo" /> injected into the constructor.
        /// </param>
        /// <returns>
        ///   Whether or not the values provided via <paramref name="leftPropertyValue" />
        ///   and <paramref name="rightPropertyValue" /> should be considered equal.
        /// </returns>
        protected virtual bool ArePropertyValuesEqualImpl(
            TProperty leftPropertyValue,
            TProperty rightPropertyValue) {

            return leftPropertyValue.Equals(rightPropertyValue);
        }

        /// <summary>
        ///   Generates a hash code for the value of the abstracted property using the
        ///   value stored on the value object provided via <paramref name="parent" />.
        /// </summary>
        /// <remarks>
        ///   If the property value is <c>null</c>, attempting to use the native hash
        ///   code generation would result in a <see cref="NullReferenceException" />.
        ///   To avoid this edge case, a constant of <c>37</c> is used instead whenever
        ///   the property's value is <c>null</c>.
        /// </remarks>
        /// <param name="parent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain the abstracted property.
        /// </exception>
        /// <returns>
        ///   An appropriate hash code for the value of the abstracted property
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        public virtual int GetPropertyValueHashCode(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            var propertyValue = this.GetPropertyValueImplOrThrow(parent, nameof(parent));

            if (propertyValue == null) {
                // Arbitrary (but prime!) number for null.

                return 37;
            }

            return this.GetPropertyValueHashCodeImpl((TProperty)propertyValue);
        }

        /// <summary>
        ///   An implementation specific hash code generator that can be
        ///   deferred to the class that inherits from this
        ///   <see cref="PropertyHandlerBase{TProperty}" /> implementation.
        ///   By default, this uses the native <see cref="Object.GetHashCode" />
        ///   implementation.
        /// </summary>
        /// <param name="propertyValue">
        ///   A non-null property value extracted from a value object that contains the
        ///   <see cref="PropertyInfo" /> injected into the constructor.
        /// </param>
        /// <returns>
        ///   An appropriate hash code for the value provided via the
        ///   <paramref name="propertyValue" /> parameter.
        /// </returns>
        protected virtual int GetPropertyValueHashCodeImpl(TProperty propertyValue) {
            return propertyValue.GetHashCode();
        }

        /// <summary>
        ///   Gets the value for the injected <see cref="PropertyInfo" /> off
        ///   of the provided <paramref name="parent" />.
        /// </summary>
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
        ///   The value of the injected <see cref="PropertyInfo" />
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        public virtual object GetPropertyValue(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            return this.GetPropertyValueImplOrThrow(parent, nameof(parent));
        }

        private TProperty GetPropertyValueImplOrThrow(object parent, string propertyName) {
            try {
                return (TProperty)this.getter.Value(parent);
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
        ///   Generates a <see cref="String" /> representation for the abstracted property
        ///   using the value stored on the value object provided via
        ///   <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain the abstracted property.
        /// </exception>
        /// <returns>
        ///   An appropriate string representation for the value of the abstracted property
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        public virtual String GetPropertyValueDisplayString(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            var propertyValue = this.GetPropertyValueImplOrThrow(parent, nameof(parent));

            if (propertyValue == null) {
                return nullAsString;
            }

            return this.GetPropertyValueDisplayStringImpl(propertyValue);
        }

        /// <summary>
        ///   An implementation specific external string representation that
        ///   can be deferred to the class that inherits from this
        ///   <see cref="PropertyHandlerBase{TProperty}" /> implementation. By default,
        ///   this uses the native <see cref="Object.ToString" /> implementation.
        /// </summary>
        /// <param name="propertyValue">
        ///   A non-null property value extracted from a value object that contains the
        ///   <see cref="PropertyInfo" /> injected into the constructor.
        /// </param>
        /// <returns>
        ///   An appropriate string representation for the value provided via the
        ///   <paramref name="propertyValue" /> parameter.
        /// </returns>
        protected virtual String GetPropertyValueDisplayStringImpl(TProperty propertyValue) {
            return propertyValue.ToString();
        }

    }

}
