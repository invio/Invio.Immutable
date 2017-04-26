using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   Various methods that are helpful for ImmutableBase to build immutable objects.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The goal with this and other *-Helpers classes is to extract some of the
    ///     assumptions and logic out of ImmutableBase so they can be tests in isolation.
    ///   </para>
    /// </remarks>
    internal static class PropertyHelpers {

        /// <summary>
        ///   Gets the properties that will be have their values carried forward
        ///   to new instances of <typeparamref name="TImmutable" /> whenever
        ///   <see cref="ImmutableBase.SetPropertyValueImpl" /> is called.
        /// </summary>
        /// <typeparam name="TImmutable">
        ///   An implementation of <see cref="ImmutableBase{TImmutable}" />
        ///   from which the caller wants to fetch its immutable properties.
        /// </typeparam>
        /// <returns>
        ///   A collection of <see cref="PropertyInfo" /> members that have data
        ///   accessible via public getters on <typeparamref name="TImmutable" />.
        /// </returns>
        internal static IList<PropertyInfo> GetProperties<TImmutable>()
            where TImmutable : ImmutableBase<TImmutable> {

            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy;

            return
                typeof(TImmutable)
                    .GetProperties(flags)
                    .Where(property => property.GetGetMethod() != null)
                    .ToImmutableList();
        }

    }

}
