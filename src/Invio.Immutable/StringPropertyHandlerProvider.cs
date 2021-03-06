using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This <see cref="IPropertyHandlerProvider" /> will create an appropriately
    ///   configured <see cref="StringPropertyHandler" /> if the <see cref="PropertyInfo" />
    ///   provided has a <see cref="PropertyInfo.PropertyType" /> of type
    ///   <see cref="String" />.
    /// </summary>
    public sealed class StringPropertyHandlerProvider : PropertyHandlerProviderBase<String> {

        private static IDictionary<StringComparison, StringComparer> comparers { get; }

        static StringPropertyHandlerProvider() {
            comparers =
                ImmutableDictionary<StringComparison, StringComparer>
                    .Empty
                    .Add(StringComparison.CurrentCulture, StringComparer.CurrentCulture)
                    .Add(StringComparison.CurrentCultureIgnoreCase, StringComparer.CurrentCultureIgnoreCase)
                    .Add(StringComparison.InvariantCulture, StringComparer.InvariantCulture)
                    .Add(StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase)
                    .Add(StringComparison.Ordinal, StringComparer.Ordinal)
                    .Add(StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///   Creates an appropriately configured instance of
        ///   <see cref="StringPropertyHandler" /> whenever the
        ///   <see cref="PropertyInfo.PropertyType" /> on the
        ///   <paramref name="property" /> provided is of type <see cref="String" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property
        ///   that exists on a class that derives from
        ///   <see cref="ImmutableBase{TImmutable}" /> that has a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="String" />
        /// </param>
        /// <returns>
        ///   A <see cref="StringPropertyHandler" /> that will calculate hash
        ///   codes and determine equality for strings stored <paramref name="property" />
        ///   within instances of <see cref="ImmutableBase{TImmutable}" />.
        /// </returns>
        protected override IPropertyHandler CreateImpl(PropertyInfo property) {
            StringComparer comparer;

            if (TryGetAttribute(property, out comparer)) {
                return new StringPropertyHandler(property, comparer);
            }

            if (TryGetAttribute(property.ReflectedType.GetTypeInfo(), out comparer)) {
                return new StringPropertyHandler(property, comparer);
            }

            return new StringPropertyHandler(property);
        }

        private static bool TryGetAttribute(MemberInfo member, out StringComparer comparer) {
            var comparison =
                member
                    .GetCustomAttributes(typeof(StringComparisonAttribute), inherit: true)
                    .Cast<StringComparisonAttribute>()
                    .SingleOrDefault()?
                    .Comparison;

            if (!comparison.HasValue) {
                comparer = null;
            } else if (!comparers.TryGetValue(comparison.Value, out comparer)) {
                throw new ArgumentException(
                    $"The {nameof(StringComparisonAttribute)} found on {member.Name} " +
                    $"references a {nameof(StringComparison)}, '{comparison.Value:G}', " +
                    $"that does not have a corresponding {nameof(StringComparer)}.",
                    nameof(comparison)
                );
            }

            return comparer != null;
        }

    }

}
