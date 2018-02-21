using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This interface provides a single point of entry for creating
    ///   the correct <see cref="IPropertyHandler" /> implementation when
    ///   given a <see cref="PropertyInfo" /> from a class that derives
    ///   from <see cref="ImmutableBase{TImmutable}" />.
    /// </summary>
    public interface IPropertyHandlerFactory {

        /// <summary>
        ///   Creates an appropriate <see cref="IPropertyHandler" />
        ///   for a given <see cref="PropertyInfo" /> that can be used
        ///   to calculate hash codes and determine equality for property
        ///   values stored on instances of
        ///   <see cref="ImmutableBase{TImmutable}" />
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property
        ///   that exists on a class that derives from
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Thrown when <paramref name="property" /> is either of a type,
        ///   or decorated in a style, that prevents it from being able to
        ///   be created from this <see cref="IPropertyHandlerFactory" />.
        /// </exception>
        /// <returns>
        ///   A <see cref="IPropertyHandler" /> that will calculate hash
        ///   codes and determine equality for property values stored
        ///   on instances of <see cref="ImmutableBase{TImmutable}" />.
        /// </returns>
        IPropertyHandler Create(PropertyInfo property);

    }

}
