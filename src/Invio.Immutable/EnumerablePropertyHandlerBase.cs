using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   This is the base <see cref="IPropertyHandler" /> for all implementations
    ///   of <see cref="IEnumerable" />. It provides a consistent, efficient display
    ///   string generation implementation that all other <see cref="IEnumerable" />-
    ///   based <see cref="IPropertyHandler" /> implementations can easily reuse.
    /// </summary>
    public abstract class EnumerablePropertyHandlerBase : PropertyHandlerBase<IEnumerable> {

        /// <summary>
        ///   Creates an instance of <see cref="EnumerablePropertyHandlerBase" />
        ///   that uses the items found in the <see cref="IEnumerable" /> values
        ///   stored in the provided <see cref="PropertyInfo" /> to determine
        ///   equality, generate hash codes, and create external string
        ///   representations.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="property" /> does not have a
        ///   <see cref="PropertyInfo.PropertyType" /> that implements
        ///   <see cref="IEnumerable" />.
        /// </exception>
        protected EnumerablePropertyHandlerBase(PropertyInfo property) :
            base(property) {}

        /// <summary>
        ///   Generates an external, user-friendly representation of the property value
        ///   that implements <see cref="IEnumerable" />. Similar to the JSON array
        ///   implementation, non-null enumerables will use square brackets to denote the
        ///   beginning and end of the array, with the native <see cref="Object.ToString" />
        ///   implementation of each item appearing between them. Each item's string
        ///   representation is seperated from the next by a comma.
        /// </summary>
        /// <remarks>
        ///   Empty enumerables and <c>null</c> references are not rendered in the same way.
        ///   <c>null</c> will be returned as a <see cref="String" /> with the value of
        ///   <c>"null"</c>, while an empty enumerable will be returned a <see cref="String" />
        ///   with the value of <c>"[]"</c>.
        /// </remarks>
        protected override String GetPropertyValueDisplayStringImpl(IEnumerable enumerable) {
            var builder = new StringBuilder("[");
            var any = false;

            foreach (var item in enumerable) {
                if (any) {
                    builder.Append(",");
                }

                builder.Append(" ");
                builder.Append(item ?? nullAsString);

                any = true;
            }

            if (any) {
                builder.Append(" ");
            }

            builder.Append("]");

            return builder.ToString();
        }

    }

}
