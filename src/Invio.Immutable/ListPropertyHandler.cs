using System;
using System.Collections;
using System.Reflection;
using Invio.Hashing;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> to use for properties that are
    ///   of type that implements <see cref="IEnumerable" />. In order for two values
    ///   to be considered equal, the items within the enumerable must be the same in
    ///   quantity, value, and order.
    /// </summary>
    public sealed class ListPropertyHandler : PropertyHandlerBase<IEnumerable> {

        /// <summary>
        ///   Creates an instance of <see cref="ListPropertyHandler" /> that
        ///   uses the items, and those items' orders found in the
        ///   <see cref="IEnumerable" /> values stored in the provided
        ///   <see cref="PropertyInfo" /> to determine equality and generate
        ///   hash codes.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="property" /> does not have a
        ///   <see cref="PropertyInfo.PropertyType" /> that implements
        ///   <see cref="IEnumerable" />.
        /// </exception>
        public ListPropertyHandler(PropertyInfo property) :
            base(property) {}

        /// <summary>
        ///   Generates a consistent, determinitic hash code using the items,
        ///   as well as the items' order, in the <see cref="IEnumerable" />
        ///   stored in the property value.
        /// </summary>
        /// <remarks>
        ///   The order is taken into account during generation, so it is
        ///   always possible (and even likely), that the hash code will be
        ///   be different if the items in the property value change their
        ///   order, even if their values do not otherwise change.
        /// </remarks>
        protected override int GetPropertyValueHashCodeImpl(IEnumerable propertyValue) {
            return HashCode.FromList(propertyValue);
        }

        /// <summary>
        ///   Compares the two <see cref="IEnumerable" /> implementations
        ///   stored on the properties for equality. This is done using the
        ///   items, as well as the items' order, found within each
        ///   property value's <see cref="IEnumerable" /> implementation.
        /// </summary>
        /// <remarks>
        ///   The enumerables are considered equal if they each contain the same
        ///   number of items, and that each item in one <see cref="IEnumerable" />
        ///   is equal with the item found at the same position within the
        ///   other <see cref="IEnumerable" />. If one of the values is <c>null</c>,
        ///   they are only considered equal if they are both <c>null</c>.
        /// </remarks>
        protected override bool ArePropertyValuesEqualImpl(
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            var left = leftPropertyValue.GetEnumerator();
            var right = rightPropertyValue.GetEnumerator();

            var leftHasMore = left.MoveNext();
            var rightHasMore = right.MoveNext();

            while (leftHasMore || rightHasMore) {
                if (leftHasMore != rightHasMore) {
                    return false;
                }

                if (Object.ReferenceEquals(left.Current, null)) {
                    if (!Object.ReferenceEquals(right.Current, null)) {
                        return false;
                    }
                } else if (!left.Current.Equals(right.Current)) {
                    return false;
                }

                leftHasMore = left.MoveNext();
                rightHasMore = right.MoveNext();
            }

            return true;
        }

    }

}
