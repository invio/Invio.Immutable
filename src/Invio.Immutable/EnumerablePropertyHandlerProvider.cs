using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This <see cref="IPropertyHandlerProvider" /> will create a
    ///   <see cref="ListPropertyHandler" /> or <see cref="SetPropertyHandler" />
    ///   if the <see cref="PropertyInfo" /> provided has a
    ///   <see cref="PropertyInfo.PropertyType" /> of type <see cref="IEnumerable" />.
    /// </summary>
    public sealed class EnumerablePropertyHandlerProvider : PropertyHandlerProviderBase {

        /// <summary>
        ///  Checks to see if the <see cref="PropertyInfo" /> has a
        ///  <see cref="PropertyInfo.PropertyType" /> that implements <see cref="IEnumerable" />
        ///  to determine if the <see cref="PropertyInfo" /> can be
        ///  used to create a <see cref="SetPropertyHandler" /> or a
        ///  <see cref="ListPropertyHandler" />.
        /// </summary>
        protected override bool IsSupportedImpl(PropertyInfo property) {
            return property.PropertyType.IsDerivativeOf(typeof(IEnumerable));
        }

        /// <summary>
        ///   Creates an instance of <see cref="SetPropertyHandler" /> whenever the
        ///   <see cref="PropertyInfo.PropertyType" /> on <paramref name="property" />
        ///   is of type <see cref="IEnumerable" /> and looks like a set. Otherwise,
        ///   it returns a <see cref="ListPropertyHandler" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property
        ///   that exists on a class that derives from
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <returns>
        ///   A <see cref="IPropertyHandler" /> that will calculate hash
        ///   codes and determine equality for property values stored
        ///   on instances of <see cref="ImmutableBase{TImmutable}" />
        /// </returns>
        protected override IPropertyHandler CreateImpl(PropertyInfo property) {
            if (IsSet(property.PropertyType)) {
                return new SetPropertyHandler(property);
            }

            return new ListPropertyHandler(property);
        }

        private static bool IsSet(Type type) {
            return type.IsDerivativeOf(typeof(ISet<>))
                || type.IsDerivativeOf(typeof(IImmutableSet<>));
        }

    }

}
