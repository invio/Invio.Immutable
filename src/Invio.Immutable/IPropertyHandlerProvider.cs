using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This interface exposes the ability to not only create
    ///   <see cref="IPropertyHandler" /> implementations, but
    ///   it also exposes the ability to check whether or not
    ///   an arbitrary <see cref="PropertyInfo" /> can be passed to
    ///   <see cref="IPropertyHandlerFactory.Create" /> without throwing a
    ///   <see cref="NotSupportedException" />.
    /// </summary>
    public interface IPropertyHandlerProvider : IPropertyHandlerFactory {

        /// <summary>
        ///   Checks whether or not the <paramref name="property" /> is a
        ///   <see cref="PropertyInfo" /> that can be used to create a
        ///   <see cref="IPropertyHandler" /> using this
        ///   <see cref="IPropertyHandlerProvider" />.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property that exists
        ///   on a class that derives from <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <returns>
        ///   This returns <c>true</c> when the <paramref name="property" /> can be
        ///   passed to <see cref="IPropertyHandlerFactory.Create" /> in order to create
        ///   a <see cref="IPropertyHandler" />. Otherwise, <c>false</c> is returned.
        /// </returns>
        bool IsSupported(PropertyInfo property);

    }


}
