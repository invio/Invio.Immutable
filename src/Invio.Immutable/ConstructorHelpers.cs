using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

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
    internal static class ConstructorHelpers {

        /// <summary>
        ///   Gets the constructor that will be used to create new instances of
        ///   <typeparamref name="TImmutable" /> whenever
        ///   <see cref="ImmutableBase.SetPropertyValueImpl" /> is called.
        /// </summary>
        /// <typeparam name="TImmutable">
        ///   An implementation of <see cref="ImmutableBase{TImmutable}" />
        ///   from which the caller wants to inspect for matching constructors.
        /// </typeparam>
        /// <param name="propertiesByName">
        ///   A collection of <see cref="PropertyInfo" /> objects whose names
        ///   and types will be used to find a matching constructor on
        ///   <typeparamref name="TImmutable" />. If no matching constructor is
        ///   found, a suitable <see cref="ArgumentException" /> will be thrown.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="propertiesByName" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   Thrown when either two or more constructors on <typeparamref name="TImmutable" />
        ///   are decorated with <see cref="ImmutableSetterConstructorAttribute />, or if the
        ///   single constructor decorated with
        ///   <see cref="ImmutableSetterConstructorAttribute" /> is incompatible with the
        ///   <paramref name="propertiesByName" /> provided.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Thrown when the <paramref name="propertiesByName" /> provided
        ///   do not map to the types and names of any constructor found on
        ///   <typeparamref name="TImmutable" />.
        /// </exception>
        /// <returns>
        ///   The first constructor found that has a constructor parameter which
        ///   matches the name and type of the properties provided via
        ///   <see cref="IList{PropertyInfo}" /> accessible via public getters.
        /// </returns>
        internal static ConstructorInfo GetImmutableSetterConstructor<TImmutable>(
            IDictionary<String, PropertyInfo> propertiesByName)
                where TImmutable : ImmutableBase<TImmutable> {

            if (propertiesByName == null) {
                throw new ArgumentNullException(nameof(propertiesByName));
            }

            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var constructors = typeof(TImmutable).GetConstructors(flags);

            var decoratedConstructors =
                constructors
                    .Where(HasImmutableSetterAttribute)
                    .ToImmutableList();

            if (decoratedConstructors.Count > 1) {
                throw new InvalidOperationException(
                    $"The {typeof(TImmutable).Name} class has {decoratedConstructors.Count} " +
                    $"constructors decorated with the " +
                    $"{nameof(ImmutableSetterConstructorAttribute)}. " +
                    $"Only one constructor is allowed to have this attribute at a time."
                );
            }

            if (decoratedConstructors.Count == 1) {
                var decoratedConstructor = decoratedConstructors.Single();

                if (!IsMatchingConstructor(decoratedConstructor, propertiesByName)) {
                    throw new InvalidOperationException(
                        $"The {typeof(TImmutable).Name} class has a constructor decorated " +
                        $"with {nameof(ImmutableSetterConstructorAttribute)} that is " +
                        $"incompatible with the following signature: " +
                        $"{ToSignature<TImmutable>(propertiesByName)}."
                    );
                }

                return decoratedConstructor;
            }

            var constructor =
                constructors
                    .OrderBy(c => c.IsPublic ? 0 : 1)
                    .FirstOrDefault(c => IsMatchingConstructor(c, propertiesByName));

            if (constructor == null) {
                throw new NotSupportedException(
                    $"The {typeof(TImmutable).Name} class lacks a constructor which " +
                    $"is compatible with the following signature: " +
                    $"{ToSignature<TImmutable>(propertiesByName)}."
                );
            }

            return constructor;
        }

        private static bool HasImmutableSetterAttribute(ConstructorInfo constructor) {
            return
                constructor
                    .GetCustomAttributes(typeof(ImmutableSetterConstructorAttribute))
                    .Any();
        }

        private static bool IsMatchingConstructor(
            ConstructorInfo constructor,
            IDictionary<String, PropertyInfo> propertiesByName) {

            var parameters = ImmutableList.Create(constructor.GetParameters());

            if (propertiesByName.Count != parameters.Count) {
                return false;
            }

            foreach (var parameter in parameters) {
                PropertyInfo property;

                if (!propertiesByName.TryGetValue(parameter.Name, out property)) {
                    return false;
                }

                if (property.PropertyType != parameter.ParameterType) {
                    return false;
                }
            }

            return true;
        }

        private static String ToSignature<TImmutable>(
            IDictionary<String, PropertyInfo> propertiesByName)
                where TImmutable : ImmutableBase<TImmutable> {

            var builder = new StringBuilder($"{typeof(TImmutable).Name}(");
            builder.Append(String.Join(", ", propertiesByName.Values.Select(ToSignature)));
            builder.Append(")");

            return builder.ToString();
        }

        private static String ToSignature(PropertyInfo property) {
            return $"{property.PropertyType.Name} {property.Name}";
        }

    }

}
