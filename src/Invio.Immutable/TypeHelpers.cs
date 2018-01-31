using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    internal static class TypeHelpers {

        internal static Func<object, object, bool> CreateSetEqualsFunc(this Type type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            } else if (!type.IsDerivativeOf(typeof(ISet<>)) &&
                       !type.IsDerivativeOf(typeof(IImmutableSet<>))) {

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
