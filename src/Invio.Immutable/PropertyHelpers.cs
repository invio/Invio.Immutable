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
    ///     assumptions and logic out of <see cref="ImmutableBase{TImmutable}" /> so
    ///     they can be tested in isolation.
    ///   </para>
    /// </remarks>
    internal static class PropertyHelpers {

        /// <summary>
        ///   Gets the properties that will be have their values carried forward
        ///   to new instances of <typeparamref name="TImmutable" /> whenever
        ///   <see cref="ImmutableBase{TImmutable}.SetPropertyValueImpl" /> is called.
        /// </summary>
        /// <typeparam name="TImmutable">
        ///   An implementation of <see cref="ImmutableBase{TImmutable}" />
        ///   from which the caller wants to fetch its immutable properties.
        /// </typeparam>
        /// <exception cref="NotSupportedException">
        ///   Thrown when the properties found on <typeparamref name="TImmutable" />
        ///   do not have unique names when accounting for case insensitivity.
        /// </exception>
        /// <returns>
        ///   A collection of <see cref="PropertyInfo" /> members that have data
        ///   accessible via public getters on <typeparamref name="TImmutable" />
        ///   in a case-insensitive dictionary keyed by each property's name.
        /// </returns>
        internal static IDictionary<String, PropertyInfo> GetPropertyMap<TImmutable>()
            where TImmutable : ImmutableBase<TImmutable> {

            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy;

            var properties =
                typeof(TImmutable)
                    .GetProperties(flags)
                    .Where(property => property.GetGetMethod() != null)
                    .ToImmutableList();

            var duplicatePropertyName =
                properties
                    .Select(property => property.Name)
                    .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .FirstOrDefault();

            if (duplicatePropertyName != null) {
                throw new NotSupportedException(
                    $"ImmutableBase<{typeof(TImmutable).Name}> requires property " +
                    $"names to be unique regardless of case, but two or more " +
                    $"properties share the name of '{duplicatePropertyName}'."
                );
            }

            return properties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }

    }

}
