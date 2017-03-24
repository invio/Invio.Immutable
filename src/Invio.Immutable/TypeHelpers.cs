using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Invio.Immutable {

    internal static class TypeHelpers {

        private static ConcurrentDictionary<Tuple<Type, Type>, bool> cache { get; }

        static TypeHelpers() {
            cache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
        }

        public static bool IsImplementingOpenGenericInterface(
            this Type type,
            Type openGenericInterface) {

            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            } else if (openGenericInterface == null) {
                throw new ArgumentNullException(nameof(openGenericInterface));
            }

            return cache.GetOrAdd(
                Tuple.Create(type, openGenericInterface),
                _ => IsImplementingOpenGenericInterfaceImpl(type, openGenericInterface)
            );
        }

        private static bool IsImplementingOpenGenericInterfaceImpl(
            Type type,
            Type openGenericInterface) {

            if (!IsInterface(openGenericInterface)) {
                throw new ArgumentException(
                    $"The '{nameof(openGenericInterface)}' argument " +
                    $"'{openGenericInterface.Name}' is not an interface.",
                    nameof(openGenericInterface)
                );
            }

            if (!IsGenericType(openGenericInterface)) {
                throw new ArgumentException(
                    $"The '{nameof(openGenericInterface)}' argument " +
                    $"'{openGenericInterface.Name}' is not a generic {nameof(Type)}.",
                    nameof(openGenericInterface)
                );
            }

            if (openGenericInterface.GetGenericTypeDefinition() != openGenericInterface) {
                throw new ArgumentException(
                    $"The '{nameof(openGenericInterface)}' argument " +
                    $"'{openGenericInterface.Name}' is not an open generic {nameof(Type)}.",
                    nameof(openGenericInterface)
                );
            }

            var isInterfaceImplemented = false;

            foreach (var implementation in type.GetInterfaces().Concat(new [] { type })) {
                if (!IsGenericType(implementation)) {
                    continue;
                }

                if (implementation.GetGenericTypeDefinition() == openGenericInterface) {
                    isInterfaceImplemented = true;
                    break;
                }
            }

            return isInterfaceImplemented;
        }

        private static bool IsGenericType(Type type) {
            bool isGenericType;

#if NETSTANDARD1_3
            isGenericType = type.GetTypeInfo().IsGenericType;
#else
            isGenericType = type.IsGenericType;
#endif

            return isGenericType;
        }

        private static bool IsInterface(Type type) {
            bool isInterface;

#if NETSTANDARD1_3
            isInterface = type.GetTypeInfo().IsInterface;
#else
            isInterface = type.IsInterface;
#endif

            return isInterface;
        }

    }

}
