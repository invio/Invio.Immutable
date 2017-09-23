using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the <see cref="IPropertyHandler" /> for use when there is no special
    ///   equality, hash code generation, or string representation for a particular type.
    ///   Other than guarding against <c>null</c>, this implementation uses the native
    ///   equality comparison, hash code generation and string representation for the
    ///   <see cref="PropertyInfo.PropertyType" /> found on the injected
    ///   <see cref="PropertyInfo" />.
    /// </summary>
    public sealed class DefaultPropertyHandler : PropertyHandlerBase<object> {

        /// <summary>
        ///   Creates an instance of <see cref="DefaultPropertyHandler" /> that
        ///   will use the native equality comparison, hash code generation and
        ///   string representation for the provided <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> on a value object that lacks any special
        ///   equality comparison, hash code generation, or string representation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        public DefaultPropertyHandler(PropertyInfo property) :
            base(property) {}

    }

}
