using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Invio.Extensions.Reflection;
using Invio.Hashing;

namespace Invio.Immutable {

    public abstract class ImmutableBase<TImmutable> : IEquatable<TImmutable>
        where TImmutable : class {

        private static Func<object[], TImmutable> createImmutable { get; }
        private static ImmutableArray<PropertyInfo> properties { get; }
        private static ImmutableArray<Func<Object, Object>> getters { get; }
        private static ImmutableDictionary<String, Func<Object, Object>> gettersByName { get; }

        static ImmutableBase() {
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.IgnoreCase |
                BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            var constructor = typeof(TImmutable).GetConstructors().Single();

            createImmutable = constructor.CreateArrayFunc<TImmutable>();

            properties =
                constructor
                    .GetParameters()
                    .Select(parameter => typeof(TImmutable).GetProperty(parameter.Name, flags))
                    .ToImmutableArray();

            getters =
                properties
                    .Select(property => property.CreateGetter())
                    .ToImmutableArray();

            gettersByName =
                properties.ToImmutableDictionary(
                    property => property.Name,
                    property => property.CreateGetter(),
                    StringComparer.OrdinalIgnoreCase
                );
        }

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

        public override int GetHashCode() {
            return HashCode.From(getters.Select(getter => getter(this)));
        }

        public override bool Equals(Object that) {
            return this.Equals(that as TImmutable);
        }

        public virtual bool Equals(TImmutable that) {
            if (Object.ReferenceEquals(that, null)) {
                return false;
            }

            if (Object.ReferenceEquals(this, that)) {
                return true;
            }

            foreach (var getter in getters) {
                var thisValue = getter(this);
                var thatValue = getter(that);

                if (Object.ReferenceEquals(thisValue, null)) {
                    if (!Object.ReferenceEquals(thatValue, null)) {
                        return false;
                    }
                } else if (!thisValue.Equals(thatValue)) {
                    return false;
                }
            }

            return true;
        }

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
