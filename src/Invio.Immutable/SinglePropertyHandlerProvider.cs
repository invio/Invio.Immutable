using System;
using System.Linq;
using System.Reflection;

namespace Invio.Immutable {
    /// <summary>
    ///   This <see cref="IPropertyHandlerProvider" /> will create an appropriately configured
    ///   <see cref="SinglePropertyHandler" /> if the <see cref="PropertyInfo" /> provided has a
    ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="Single" />.
    /// </summary>
    public class SinglePropertyHandlerProvider : PropertyHandlerProviderBase<Single> {
        /// <summary>
        ///   Checks to see if the <see cref="PropertyInfo" /> has a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="Single" /> to determine
        ///   if the <see cref="PropertyInfo" /> can be use to create a
        ///   <see cref="SinglePropertyHandler" />.
        /// </summary>
        /// <inheritdoc />
        protected override bool IsSupportedImpl(PropertyInfo property) {
            return base.IsSupportedImpl(property) &&
                (TryGetAttribute(property, out _) ||
                    TryGetAttribute(property.ReflectedType.GetTypeInfo(), out _));
        }

        /// <summary>
        ///   Creates an appropriately configured instance of <see cref="SinglePropertyHandler" />
        ///   whenever the <see cref="PropertyInfo.PropertyType" /> on the
        ///   <paramref name="property" /> provided is of type <see cref="Single" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property that exists on a class that
        ///   derives from <see cref="ImmutableBase{TImmutable}" /> that has a
        ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="Single" />.
        /// </param>
        /// <returns>
        ///   A <see cref="SinglePropertyHandler" /> that will calculate hash codes and determine
        ///   equality for Single stored <paramref name="property" /> within instances of
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </returns>
        protected override IPropertyHandler CreateImpl(PropertyInfo property) {
            if (TryGetAttribute(property, out var attribute)) {
                return new SinglePropertyHandler(
                    property,
                    attribute.Precision,
                    attribute.PrecisionStyle
                );
            } else if (TryGetAttribute(property.ReflectedType.GetTypeInfo(), out attribute)) {
                return new SinglePropertyHandler(
                    property,
                    attribute.Precision,
                    attribute.PrecisionStyle
                );
            } else {
                // Should never happen. See IsSupportedImpl
                throw new NotSupportedException(
                    $"The specified property ('{property.Name}') does not have the expected " +
                    $"attribute applied."
                );
            }
        }

        private static Boolean TryGetAttribute(
            MemberInfo member,
            out SingleComparisonAttribute attribute) {

            attribute =
                member
                    .GetCustomAttributes(typeof(SingleComparisonAttribute), inherit: true)
                    .Cast<SingleComparisonAttribute>()
                    .SingleOrDefault();

            return attribute != null;
        }
    }
}