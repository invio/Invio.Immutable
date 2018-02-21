using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This <see cref="IPropertyHandlerProvider" /> will create a baseline
    ///   <see cref="IPropertyHandler" /> for all property types. This is
    ///   because it uses the property type's inherent hash code generation
    ///   and equality comparison implementations.
    /// </summary>
    public sealed class DefaultPropertyHandlerProvider : PropertyHandlerProviderBase {

        /// <summary>
        ///   Creates the baseline <see cref="IPropertyHandler" /> that uses
        ///   the default hash code generation and equality comparison
        ///   implementations using the <see cref="PropertyInfo.PropertyType" />
        ///   found on the provided <paramref name="property" />.
        /// </summary>
        protected override IPropertyHandler CreateImpl(PropertyInfo property) {
            return new DefaultPropertyHandler(property);
        }

    }

}
