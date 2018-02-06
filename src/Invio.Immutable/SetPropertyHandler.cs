using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Hashing;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> to use for properties that are
    ///   of types that implement <see cref="IEnumerable" />. In order for two values
    ///   to be considered equal, the items within the enumerable must be the same in
    ///   quantity and value, but they do not need to be in the same order.
    /// </summary>
    public sealed class SetPropertyHandler : PropertyHandlerBase<IEnumerable> {

        private Func<IEnumerable, IEnumerable, bool> arePropertyValuesEqual { get; }
        private Lazy<Func<object, object, object>> lazySetEqualsFunc { get; }

        /// <summary>
        ///   Creates an instance of <see cref="SetPropertyHandler" /> that
        ///   uses the items. but not those items' orders, found in the
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
        public SetPropertyHandler(PropertyInfo property) : base(property) {
            if (this.PropertyType.IsDerivativeOf(typeof(ISet<>)) ||
                this.PropertyType.IsDerivativeOf(typeof(IImmutableSet<>))) {

                this.lazySetEqualsFunc = new Lazy<Func<object, object, object>>(
                    () =>
                        this.PropertyType
                            .GetMethod(nameof(ISet<Object>.SetEquals))
                            .CreateFunc1()
                );

                this.arePropertyValuesEqual = this.AreSetsEqual;
            } else {
                this.arePropertyValuesEqual = this.AreEnumerablesEquivalent;
            }
        }

        /// <summary>
        ///   Generates a consistent, determinitic hash code using the hash codes of
        ///   items. If two enumerables contain the same items but those items are in
        ///   a different order, they will still return the same hash code.
        /// </summary>
        /// <remarks>
        ///   This implementation does not remove duplicate values when calculating
        ///   the hash code. It assumes that duplicates have been removed ahead of time,
        ///   as traditional set implementations do not allow duplicates to exist,
        ///   and we do not want to inject opinionated definitions of what is a
        ///   duplicate at this point in time.
        /// </remarks>
        protected override int GetPropertyValueHashCodeImpl(IEnumerable propertyValue) {
            return HashCode.FromSet(propertyValue);
        }

        /// <summary>
        ///   Compares the two <see cref="IEnumerable" /> implementations
        ///   stored on the properties for equality. This is done using the
        ///   items, but not those items' orders, found within each
        ///   property value's <see cref="IEnumerable" /> implementation.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The enumerables are considered equal if they each contain the same
        ///     number of items, and that each item in one <see cref="IEnumerable" />
        ///     is equal with an item found within the other <see cref="IEnumerable" />.
        ///     If one of the values is <c>null</c>, they are only considered equal
        ///     if they are both <c>null</c>.
        ///   </para>
        ///   <para>
        ///     This implementation does not remove duplicate values when calcilating
        ///     equality. It assumes that duplicates have been removed ahead of time,
        ///     as traditional set implementations do not allow duplicates to exist,
        ///     and we do not want to inject opinionated definitions of what is a
        ///     duplicate at this point in time.
        ///   </para>
        /// </remarks>
        protected override bool ArePropertyValuesEqualImpl(
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            return this.arePropertyValuesEqual(leftPropertyValue, rightPropertyValue);
        }

        private bool AreSetsEqual(IEnumerable left, IEnumerable right) {
            return (bool)this.lazySetEqualsFunc.Value(left, right);
        }

        private bool AreEnumerablesEquivalent(IEnumerable left, IEnumerable right) {
            var leftElementCounts = GetElementCounts(left, out int leftNullCount);
            var rightElementCounts = GetElementCounts(right, out int rightNullCount);

            if (leftNullCount != rightNullCount ||
                leftElementCounts.Count != rightElementCounts.Count) {

                return false;
            }

            foreach (var kvp in leftElementCounts) {
                var leftElementCount = kvp.Value;

                if (!rightElementCounts.TryGetValue(kvp.Key, out int rightElementCount)) {
                    return false;
                }

                if (leftElementCount != rightElementCount) {
                    return false;
                }
            }

            return true;
        }

        private static IDictionary<object, int> GetElementCounts(
            IEnumerable enumerable,
            out int nullCount) {

            var dictionary = new Dictionary<object, int>();

            nullCount = 0;

            foreach (var element in enumerable) {
                if (element == null) {
                    nullCount++;
                } else {
                    int num;
                    dictionary.TryGetValue(element, out num);
                    num++;
                    dictionary[element] = num;
                }
            }

            return dictionary;
        }

    }

}
