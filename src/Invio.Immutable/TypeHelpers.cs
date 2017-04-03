using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    internal static class TypeHelpers {

        private static ConcurrentDictionary<Tuple<Type, Type>, bool> cache { get; }

        static TypeHelpers() {
            cache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
        }

        internal static bool IsImplementingOpenGenericInterface(
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

        internal static Func<object, object, bool> CreateSetEqualsFunc(this Type type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            } else if (!type.IsImplementingOpenGenericInterface(typeof(ISet<>))) {
                throw new ArgumentException(
                    $"The '{nameof(type)}' provided does not implement ISet<>.",
                    nameof(type)
                );
            }

            var func =
                type.GetMethod(nameof(ISet<Object>.SetEquals))
                    .CreateFunc1();

            return new Func<object, object, bool>(
                (left, right) => {
                    if (left == null || right == null) {
                        return left == null && right == null;
                    }

                    return (bool)func(left, right);
                }
            );
        }

        internal static Func<object, object, bool> CreateEnumerableEqualsFunc(this Type type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            } else if (type != typeof(IEnumerable) &&
                       !type.GetInterfaces().Contains(typeof(IEnumerable))) {
                throw new ArgumentException(
                    $"The '{nameof(type)}' provided does not implement IEnumerable.",
                    nameof(type)
                );
            }

            return new Func<object, object, bool>(
                (leftSource, rightSource) => {
                    if (leftSource == null || rightSource == null) {
                        return leftSource == null && rightSource == null;
                    }

                    var left = ((IEnumerable)leftSource).GetEnumerator();
                    var right = ((IEnumerable)rightSource).GetEnumerator();

                    var leftHasMore = left.MoveNext();
                    var rightHasMore = right.MoveNext();

                    while (leftHasMore || rightHasMore) {
                        if (leftHasMore != rightHasMore) {
                            return false;
                        }

                        if (Object.ReferenceEquals(left.Current, null)) {
                            if (!Object.ReferenceEquals(right.Current, null)) {
                                return false;
                            }
                        } else if (!left.Current.Equals(right.Current)) {
                            return false;
                        }

                        leftHasMore = left.MoveNext();
                        rightHasMore = right.MoveNext();
                    }

                    return true;
                }
            );
        }

    }

}
