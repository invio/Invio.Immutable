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
                    .Select(p => p.CreateGetter())
                    .ToImmutableArray();
        }

        protected TImmutable SetPropertyValueImpl(String propertyName, object value) {
            var values = new object[properties.Length];

            for (var index = 0; index < values.Length; index++) {
                var property = properties[index];

                if (property.Name.Equals(propertyName)) {
                    values[index] = value;
                } else {
                    values[index] = getters[index](this);
                }
            }

            return createImmutable(values);
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
                if (Object.ReferenceEquals(getter(this), null)) {
                    if (!Object.ReferenceEquals(getter(that), null)) {
                        return false;
                    }
                } else if (!getter(this).Equals(getter(that))) {
                    return false;
                }
            }

            return true;
        }

    }

}
