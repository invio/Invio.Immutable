using System;
using System.Diagnostics;
using System.Reflection;

namespace Invio.Immutable {
    /// <summary>
    ///   Wraps an <see cref="IPropertyHandler" /> that handles value type properties
    ///   to support properties of type <see cref="Nullable{TProperty}" />
    /// </summary>
    /// <typeparam name="TProperty">
    ///   The type that the inner <see cref="IPropertyHandler" /> supports.
    /// </typeparam>
    internal class NullablePropertyHandler<TProperty> :
        PropertyHandlerBase<TProperty?> where TProperty : struct {

        private IPropertyHandler innerPropertyHandler { get; }

        /// <summary>
        ///   Creates an instance of <see cref="NullablePropertyHandler{TProperty}" /> that
        ///   unwraps Nullable property values and delegates to an inner
        ///   <see cref="IPropertyHandler" />.
        /// </summary>
        /// <param name="innerPropertyHandler">
        ///   The inner <see cref="IPropertyHandler" /> implementation used to handle
        ///   non-nullable property values.
        /// </param>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that will potentially
        ///   have its native equality comparison and hash code generation by the
        ///   inheriting class.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="innerPropertyHandler" /> is null.
        /// </exception>
        internal NullablePropertyHandler(
            IPropertyHandler innerPropertyHandler,
            PropertyInfo property) : base(property) {
            if (innerPropertyHandler == null) {
                throw new ArgumentNullException(nameof(innerPropertyHandler));
            }

            this.innerPropertyHandler = innerPropertyHandler;
        }

        /// <remarks>
        ///   This override handles nulls and then dispatches to the inner
        ///   <see cref="IPropertyHandler"/> used to construct this instance.
        /// </remarks>
        /// <inheritdoc />
        public override bool ArePropertyValuesEqual(object leftParent, object rightParent) {
            if (leftParent == null) {
                throw new ArgumentNullException(nameof(leftParent));
            } else if (rightParent == null) {
                throw new ArgumentNullException(nameof(rightParent));
            }

            var leftPropertyValue =
                this.GetPropertyValueImplOrThrow(leftParent, nameof(leftParent));
            var rightPropertyValue =
                this.GetPropertyValueImplOrThrow(rightParent, nameof(rightParent));

            // Handle nulls

            if (Object.ReferenceEquals(leftPropertyValue, null)) {
                return Object.ReferenceEquals(rightPropertyValue, null);
            } else if (Object.ReferenceEquals(rightPropertyValue, null)) {
                return false;
            }

            return this.innerPropertyHandler.ArePropertyValuesEqual(leftParent, rightParent);
        }

        /// <remarks>
        ///   This override handles nulls and then dispatches to the inner
        ///   <see cref="IPropertyHandler"/> used to construct this instance.
        /// </remarks>
        /// <inheritdoc />
        public override int GetPropertyValueHashCode(object parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            var propertyValue = this.GetPropertyValueImplOrThrow(parent, nameof(parent));

            if (propertyValue == null) {
                // Arbitrary (but prime!) number for null.

                return NullValueHashCode;
            }

            return this.innerPropertyHandler.GetPropertyValueHashCode(parent);
        }

        /// <summary>
        ///   This method is not supported.
        /// </summary>
        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        ///   This method is not supported.
        /// </exception>
        protected override bool ArePropertyValuesEqualImpl(TProperty? leftValue, TProperty? rightValue) {
            throw new NotSupportedException();
        }

        /// <summary>
        ///   This method is not supported.
        /// </summary>
        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        ///   This method is not supported.
        /// </exception>
        protected override int GetPropertyValueHashCodeImpl(TProperty? propertyValue) {
            throw new NotSupportedException();
        }
    }
}