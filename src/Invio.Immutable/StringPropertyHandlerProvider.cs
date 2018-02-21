using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This <see cref="IPropertyHandlerProvider" /> will create an appropriately
    ///   configured <see cref="StringPropertyHandler" /> if the <see cref="PropertyInfo" />
    ///   provided has a <see cref="PropertyInfo.PropertyType" /> of type
    ///   <see cref="String" />.
    /// </summary>
    public sealed class StringPropertyHandlerProvider : PropertyHandlerProviderBase {

        /// <summary>
        ///  Checks to see if the <see cref="PropertyInfo" /> has a
        ///  <see cref="PropertyInfo.PropertyType" /> of type <see cref="String" />
        ///  to determine if the <see cref="PropertyInfo" /> can be
        ///  use to create a <see cref="StringPropertyHandler" />.
        /// </summary>
        protected override bool IsSupportedImpl(PropertyInfo property) {
            return property.PropertyType == typeof(string);
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
            return new StringPropertyHandler(property);
        }

    }

}
