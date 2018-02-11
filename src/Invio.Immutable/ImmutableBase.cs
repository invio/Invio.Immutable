using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Invio.Extensions.Reflection;
using Invio.Hashing;

namespace Invio.Immutable {

    /// <summary>
    ///   A reuseable base class that allows for the definition of classes that
    ///   encapsulate a collection of related immutable properties.
    /// </summary>
    /// <typeparam name="TImmutable">
    ///   The class which inherits from <see cref="ImmutableBase{TImmutable}" />
    ///   that encapsulates a collection of related immutable properties.
    /// </typeparam>
    public abstract class ImmutableBase<TImmutable> : IEquatable<TImmutable>
        where TImmutable : ImmutableBase<TImmutable> {

        private static StringComparer ordinalIgnoreCase { get; }
        private static Func<object[], TImmutable> createImmutable { get; }
        private static ImmutableArray<IPropertyHandler> handlers { get; }
        private static ImmutableDictionary<String, IPropertyHandler> handlersByName { get; }
        private static IStringFormatter stringFormatter { get; }

        static ImmutableBase() {
            ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            var propertiesByName = PropertyHelpers.GetPropertyMap<TImmutable>();

            var constructor =
                ConstructorHelpers
                    .GetImmutableSetterConstructor<TImmutable>(propertiesByName);

            createImmutable = constructor.CreateArrayFunc<TImmutable>();

            var properties =
                constructor
                    .GetParameters()
                    .Select(parameter => propertiesByName[parameter.Name])
                    .ToImmutableArray();

            var handlersBuilder = ImmutableArray.CreateBuilder<IPropertyHandler>();
            var handlersByNameBuilder =
                ImmutableDictionary.CreateBuilder<String, IPropertyHandler>(ordinalIgnoreCase);

            foreach (var property in properties) {
                var type = property.PropertyType;

                IPropertyHandler handler;

                if (type == typeof(String)) {
                    handler = new StringPropertyHandler(property);
                } else if (type.IsDerivativeOf(typeof(ISet<>)) ||
                           type.IsDerivativeOf(typeof(IImmutableSet<>))) {
                    handler = new SetPropertyHandler(property);
                } else if (type.IsDerivativeOf(typeof(IEnumerable))) {
                    handler = new ListPropertyHandler(property);
                } else {
                    handler = new DefaultPropertyHandler(property);
                }

                handlersBuilder.Add(handler);
                handlersByNameBuilder.Add(handler.PropertyName, handler);
            }

            handlers = handlersBuilder.ToImmutable();
            handlersByName = handlersByNameBuilder.ToImmutable();
            stringFormatter = new DefaultStringFormatter();
        }

        /// <summary>
        ///   Gets the current value for the publicly accessible property found on
        ///   <typeparamref name="TImmutable" /> that could be uniquely identified
        ///   by <paramref name="propertyName" />.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method is intentionally left as 'protected' so that
        ///     <see cref="ImmutableBase{TImmutable}" /> leaves it up to the implementer as
        ///     to whether or not he or she wishes to expose the generic retrieval of values.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">
        ///   The name of a publicly accessible property on <typeparamref name="TImmutable" />
        ///   that will have its value returned via the invocation of this method. The approach
        ///   used to locate a property with a matching name uses ordinal case-insensitivity.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="propertyName" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when no public property that has the same name as that which was
        ///   provided via <paramref name="propertyName" /> was found.
        /// </exception>
        /// <returns>
        ///   The value of the property identified by <paramref name="propertyName" />.
        /// </returns>
        protected object GetPropertyValueImpl(String propertyName) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            IPropertyHandler handler;

            if (!handlersByName.TryGetValue(propertyName, out handler)) {
                throw new ArgumentException(
                    $"The '{propertyName}' property was not found.",
                    nameof(propertyName)
                );
            }

            return handler.GetPropertyValue(this);
        }

        /// <summary>
        ///   Takes all of the current values for all of the publicly accessible properties
        ///   found on <typeparamref name="TImmutable" /> and uses them to creates a new
        ///   instance of <typeparamref name="TImmutable" /> after replacing the current value
        ///   for the property identified by <paramref name="propertyName" /> with the
        ///   value provided by <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method leaves the current instance of <typeparamref name="TImmutable" />
        ///     unaffected. Only the new instance returned will have the property identified
        ///     by <paramref name="propertyName" /> updated to the provided
        ///     <paramref name="value" />.
        ///   </para>
        ///   <para>
        ///     This method is intentionally left as 'protected' with an '*Impl' suffix so
        ///     that it can be left up to the implementer to decide if the inheriting class
        ///     should expose the ability to set values by any given property name.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">
        ///   The name of the property that exists on <typeparamref name="TImmutable" />
        ///   that will be updated to the provided <paramref name="value" />. The approach
        ///   used to locate a property with a matching name is ordinal case-insensitivity.
        /// </param>
        /// <param name="value">
        ///   The new value for the property identified via <paramref name="propertyName" />
        ///   that will be updated on the new instance of <typeparamref name="TImmutable" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="propertyName" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when no public property that has the same name as that which was provided
        ///   via <paramref name="propertyName" /> was found, or if the value provided via
        ///   <paramref name="value" /> is incompatible with the identified property's type.
        /// </exception>
        /// <returns>
        ///   A new instance of <typeparamref name="TImmutable" /> which has all of the
        ///   same values for all of its public properties with the exception of the
        ///   property identified by <paramref name="propertyName" />. That property
        ///   will have its value updated to <paramref name="value" />.
        /// </returns>
        protected TImmutable SetPropertyValueImpl(String propertyName, object value) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var isPropertyFound = false;
            var values = new object[handlers.Length];

            for (var index = 0; index < values.Length; index++) {
                var handler = handlers[index];

                if (ordinalIgnoreCase.Equals(handler.PropertyName, propertyName)) {
                    if (!IsAssignable(handler.PropertyType, value)) {
                        var formattedValue = value?.ToString() ?? "null";

                        throw new ArgumentException(
                            $"Unable to assign the value ({formattedValue}) " +
                            $"to the '{propertyName}' property.",
                            nameof(value)
                        );
                    }

                    isPropertyFound = true;
                    values[index] = value;
                } else {
                    values[index] = handler.GetPropertyValue(this);
                }
            }

            if (!isPropertyFound) {
                throw new ArgumentException(
                    $"The '{propertyName}' property was not found.",
                    nameof(propertyName)
                );
            }

            return createImmutable(values);
        }
        /// <summary>
        ///   Takes all of the current values for all of the publicly accessible properties
        ///   found on <typeparamref name="TImmutable" /> and uses them to creates a new
        ///   instance of <typeparamref name="TImmutable" /> after replacing the current values
        ///   for the properties identified in the <paramref name="propertyValues" />
        ///   collection.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method leaves the current instance of <typeparamref name="TImmutable" />
        ///     unaffected. Only the new instance returned will have its properties updated
        ///     based upon the property names and values provided via the
        ///     <paramref name="propertyValues" /> parameter.
        ///   </para>
        ///   <para>
        ///     This method is intentionally left as 'protected' with an '*Impl' suffix so
        ///     that it can be left up to the implementer to decide if the inheriting class
        ///     should expose the ability to set values by any given property name.
        ///   </para>
        /// </remarks>
        /// <param name="propertyValues">
        ///   An <see cref="IDictionary{String, Object}" /> that represents a mapping between
        ///   the names of properties that exist on <typeparamref name="TImmutable" /> to
        ///   updated values that should be assigned on the newly created instance of
        ///   <typeparamref name="TImmutable" />. This implementation ignores the
        ///   <see cref="IEqualityComparer{String}" /> used to identify distinct keys in
        ///   <paramref name="propertyValues" />. Instead, the approach used to identify
        ///   properties with matching names is ordinal case-insensitivity.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="propertyValues" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when no public property has the same name as one or more of the keys
        ///   provided within <paramref name="propertyValues" />, if the value provided for
        ///   one of the identified properties is incompatible with that property's type,
        ///   or if the same property is identified as a matching property for two or more
        ///   of the provided values.
        /// </exception>
        /// <returns>
        ///   A new instance of <typeparamref name="TImmutable" /> which has all of the
        ///   same values for all of its public properties with the exception of the
        ///   properties identified by <paramref name="propertyValues" />. Those properties
        ///   will have their values updated to those provided as values in the
        ///   <paramref name="propertyValues" /> parameter.
        /// </returns>
        protected TImmutable SetPropertyValuesImpl(IDictionary<string, object> propertyValues) {
            if (propertyValues == null) {
                throw new ArgumentNullException(nameof(propertyValues));
            }

            var mappings = new Dictionary<string, KeyValuePair<string, object>>();

            foreach (var kvp in propertyValues) {
                if (handlersByName.TryGetValue(kvp.Key, out IPropertyHandler handler)) {
                    try {
                        mappings.Add(handler.PropertyName, kvp);
                    } catch (ArgumentException innerException) {
                        throw new ArgumentException(
                            $"The '{kvp.Key}' property was specified more than once.",
                            nameof(propertyValues),
                            innerException
                        );
                    }
                } else {
                    throw new ArgumentException(
                        $"The '{kvp.Key}' property was not found.",
                        nameof(propertyValues)
                    );
                }
            }

            var values = new object[handlers.Length];

            for (var index = 0; index < values.Length; index++) {
                var handler = handlers[index];

                KeyValuePair<string, object> propertyValueMapping;

                if (mappings.TryGetValue(handler.PropertyName, out propertyValueMapping)) {
                    var propertyName = propertyValueMapping.Key;
                    var propertyValue = propertyValueMapping.Value;

                    if (!IsAssignable(handler.PropertyType, propertyValue)) {
                        var formattedValue = propertyValue?.ToString() ?? "null";

                        throw new ArgumentException(
                            $"Unable to assign the value ({formattedValue}) " +
                            $"to the '{propertyName}' property.",
                            nameof(propertyValues)
                        );
                    }

                    values[index] = propertyValue;
                } else {
                    values[index] = handler.GetPropertyValue(this);
                }
            }

            return createImmutable(values);
        }

        private static bool IsAssignable(Type type, object value) {
            if (value == null) {
                return (!type.GetTypeInfo().IsValueType)
                    || (Nullable.GetUnderlyingType(type) != null);
            }

            return type.IsAssignableFrom(value.GetType());
        }

        /// <summary>
        ///   Generates a consistent hash code based upon the values of each of
        ///   the publically accessible properties on this instance of
        ///   <typeparamref name="TImmutable" />. If a non-String property
        ///   implements <see cref="IEnumerable" />, its items' hash codes
        ///   are combined to generate a hash code for that property.
        /// </summary>
        /// <remarks>
        ///   For properties that implement <see cref="IEnumerable" />, the
        ///   hash codes of the items are combined to represent the hash code
        ///   of the property's value. If the property also implements the
        ///   <see cref="ISet{T}" /> interface, the order of the items is
        ///   ignored with regard to hash code generation. If the property
        ///   does not implement <see cref="ISet{T}" />, order is respected.
        /// </remarks>
        /// <returns>
        ///   A consistent hash code based upon the values of each of the
        ///   publically accessibly properties.
        /// </returns>
        public override int GetHashCode() {
            return HashCode.From(
                handlers
                    .Select(handler => handler.GetPropertyValueHashCode(this))
                    .Cast<object>()
            );
        }

        /// <summary>
        ///   Determines whether or not this instance of <typeparamref name="TImmutable" />
        ///   is equal to <paramref name="that" />. The two instances will only be
        ///   considered equal if <paramref name="that" /> is an instance of
        ///   <typeparamref name="TImmutable" /> which is considered equal to this instance
        ///   when passed to the <see cref="Equals(TImmutable)" /> overload of this method.
        /// </summary>
        /// <param name="that">
        ///   An object that, if it is an instance of <typeparamref name="TImmutable" />,
        ///   will be compared against this instance for equality via
        ///   <see cref="Equals(TImmutable)" />.
        /// </param>
        /// <returns>
        ///   Whether or not this instance of <typeparamref name="TImmutable" />
        ///   is equal to <paramref name="that" />.
        /// </returns>
        public override bool Equals(Object that) {
            return this.Equals(that as TImmutable);
        }

        /// <summary>
        ///   Determines whether or not this instance of <typeparamref name="TImmutable" />
        ///   is equal to <paramref name="that" /> based upon a property by property
        ///   value comparison. If a non-String property implements
        ///   <see cref="IEnumerable" />, the items of each instance's properties
        ///   are compared to determine equality.
        /// </summary>
        /// <remarks>
        ///   For properties that implement <see cref="IEnumerable" />, the
        ///   the items of each enumerable are compared to determine if the
        ///   two properties are equal. If the property also implements the
        ///   <see cref="ISet{T}" /> interface, the order of the items is
        ///   ignored with regard to equality. If the property does not
        ///   implement <see cref="ISet{T}" />, order is respected.
        /// </remarks>
        /// <param name="that">
        ///   An object that will be compared against this instance
        ///   for equality based upon a property by property comparison.
        /// </param>
        /// <returns>
        ///   Whether or not this instance of <typeparamref name="TImmutable" />
        ///   is equal to <paramref name="that" />.
        /// </returns>
        public virtual bool Equals(TImmutable that) {
            if (Object.ReferenceEquals(that, null)) {
                return false;
            }

            if (Object.ReferenceEquals(this, that)) {
                return true;
            }

            foreach (var handler in handlers) {
                if (!handler.ArePropertyValuesEqual(this, that)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///   Converts the current instance of <typeparamref name="TImmutable" />
        ///   string using JSON-inspired formatting. Each property is a key, and
        ///   each property value is a value.
        /// </summary>
        /// <returns>
        ///   An end-user friendly string representation of the current instance as
        ///   a bag of immutable key-value pairs.
        /// </returns>
        public override String ToString() {
            if (handlers.IsEmpty) {
                return "{}";
            }

            var output = new StringBuilder("{ ");

            output.Append(this.GetHandlerDisplayString(handlers.First()));

            foreach (var handler in handlers.Skip(1)) {
                output.Append(", ");
                output.Append(this.GetHandlerDisplayString(handler));
            }

            output.Append(" }");

            return output.ToString();
        }

        private String GetHandlerDisplayString(IPropertyHandler handler) {
            return String.Format(
                "{0}: {1}",
                handler.PropertyName,
                stringFormatter.Format(handler.GetPropertyValue(this))
            );
        }

    }

}
