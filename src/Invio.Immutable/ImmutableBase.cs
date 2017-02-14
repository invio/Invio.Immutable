using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Hashing;

namespace Invio.Immutable {

    public abstract class ImmutableBase<TImmutable> : IEquatable<TImmutable>
        where TImmutable : class {

        private static Func<object[], TImmutable> createImmutable { get; }
        private static ImmutableArray<PropertyInfo> properties { get; }
        private static ImmutableArray<Func<Object, Object>> getters { get; }

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

    }

}
