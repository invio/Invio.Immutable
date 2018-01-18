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

        private static Func<object[], TImmutable> createImmutable { get; }
        private static ImmutableArray<PropertyInfo> properties { get; }
        private static ImmutableArray<Func<Object, Object>> getters { get; }
        private static ImmutableDictionary<String, Func<Object, Object>> gettersByName { get; }
        private static ImmutableArray<Func<Object, Object>> getHashCodeValues { get; }
        private static ImmutableArray<Func<Object, Object, bool>> areEqualFuncs { get; }

        static ImmutableBase() {
            var propertiesByName = PropertyHelpers.GetPropertyMap<TImmutable>();

            var constructor =
                ConstructorHelpers
                    .GetImmutableSetterConstructor<TImmutable>(propertiesByName);

            createImmutable = constructor.CreateArrayFunc<TImmutable>();

            properties =
                constructor
                    .GetParameters()
                    .Select(parameter => propertiesByName[parameter.Name])
                    .ToImmutableArray();

            getters =
                properties
                    .Select(property => property.CreateGetter())
                    .ToImmutableArray();

            gettersByName =
                properties.ToImmutableDictionary(
                    property => property.Name,
                    property => property.CreateGetter()
                );

            var getHashCodeValuesBuilder = ImmutableArray.CreateBuilder<Func<Object, Object>>();
            var areEqualFuncsBuilder = ImmutableArray.CreateBuilder<Func<Object, Object, bool>>();

            foreach (var property in properties) {
                var type = property.PropertyType;
                var getter = gettersByName[property.Name];

                Func<object, object, bool> areEqual;
                Func<object, object> getHashCodeValue;

                if (type.IsDerivativeOf(typeof(ISet<>))) {
                    areEqual = type.CreateSetEqualsFunc();
                    getHashCodeValue =
                        (instance) => HashCode.FromSet((IEnumerable)getter(instance));
                } else if (type != typeof(String) &&
                           type.GetInterfaces().Contains(typeof(IEnumerable))) {
                    areEqual = type.CreateEnumerableEqualsFunc();
                    getHashCodeValue =
                        (instance) => HashCode.FromList((IEnumerable)getter(instance));
                } else {
                    areEqual = (left, right) => left.Equals(right);
                    getHashCodeValue = getter;
                }

                getHashCodeValuesBuilder.Add(getHashCodeValue);
                areEqualFuncsBuilder.Add(areEqual);
            }

            getHashCodeValues = getHashCodeValuesBuilder.ToImmutable();
            areEqualFuncs = areEqualFuncsBuilder.ToImmutable();
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

            Func<object, object> getter;

            if (!gettersByName.TryGetValue(propertyName, out getter)) {
                throw new ArgumentException(
                    $"The '{propertyName}' property was not found.",
                    nameof(propertyName)
                );
            }

            return getter(this);
        }

        /// <summary>
        ///   Takes all of the current values for all of the publicly accessible properties
        ///   found on <typeparamref name="TImmutable" /> and uses them to creates a new
        ///   instance of <typeparamref name="TImmutable" /> after replacing the current value
        ///   for the property identified by <paramref name="propertyName" /> with the
        ///   provided <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method leaves the current instance of <typeparamref name="TImmutable" />
        ///     unaffected. Only the new instance returned from this method will have the
        ///     property identified by <paramref name="propertyName" /> changed to the
        ///     provided <paramref name="value" />.
        ///   </para>
        ///   <para>
        ///     This method is intentionally left as 'protected' so that
        ///     <see cref="ImmutableBase{TImmutable}" /> leaves it up to the implementer as
        ///     to whether or not he or she wishes to expose the generic setting of values.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">
        ///   The name of the property that exists on <typeparamref name="TImmutable" />
        ///   that is to have its value updated to the provided <paramref name="value" />
        ///   on the new instance of <typeparamref name="TImmutable" />. The approach used
        ///   to locate a property with a matching name uses ordinal case-insensitivity.
        /// </param>
        /// <param name="value">
        ///   The new value for the property identified via <paramref name="propertyName" />
        ///   that will be used on the new instance of <typeparamref name="TImmutable" />.
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
        ///   will have the value provided via <paramref name="value" />.
        /// </returns>
        protected TImmutable SetPropertyValueImpl(String propertyName, object value) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var isPropertyFound = false;
            var values = new object[properties.Length];

            for (var index = 0; index < values.Length; index++) {
                var property = properties[index];

                if (property.Name.Equals(propertyName)) {
                    if (!IsAssignable(property.PropertyType, value)) {
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
                    values[index] = getters[index](this);
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
            return HashCode.From(getHashCodeValues.Select(getter => getter(this)));
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

            for (var index = 0; index < getters.Length; index++) {
                var thisValue = getters[index](this);
                var thatValue = getters[index](that);
                var areEqual = areEqualFuncs[index];

                if (Object.ReferenceEquals(thisValue, null)) {
                    if (!Object.ReferenceEquals(thatValue, null)) {
                        return false;
                    }
                } else if (!areEqual(thisValue, thatValue)) {
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
        ///   A end-user friendly string representation of the current instance as
        ///   a bag of immutable key-value pairs.
        /// </returns>
        public override String ToString() {
            if (!properties.Any()) {
                return "{}";
            }

            var output = new StringBuilder("{ ");

            output.Append(ToString(properties.First()));

            foreach (var property in properties.Skip(1)) {
                output.Append(", ");
                output.Append(ToString(property));
            }

            output.Append(" }");

            return output.ToString();
        }

        private String ToString(PropertyInfo property) {
            var value = this.GetPropertyValueImpl(property.Name);

            if (value == null) {
                return $"{property.Name}: null";
            }

            var propertyType =
                Nullable.GetUnderlyingType(property.PropertyType) ??
                property.PropertyType;

            if (propertyType == typeof(DateTime)) {
                return $"{property.Name}: {((DateTime)value):o}";
            }

            return $"{property.Name}: {value}";
        }

    }

}
