using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   Aggregates several <see cref="IPropertyHandlerProvider" /> implementations
    ///   together where the first <see cref="IPropertyHandlerProvider" /> that
    ///   supports a given <see cref="PropertyInfo" /> can be used to hydrate a
    ///   <see cref="IPropertyHandler" />.
    /// </summary>
    public sealed class AggregatePropertyHandlerProvider : PropertyHandlerProviderBase {

        private IList<IPropertyHandlerProvider> providers { get; }

        /// <summary>
        ///   Creates an instance of <see cref="AggregatePropertyHandlerProvider" />
        ///   that will use the first injected <see cref="IPropertyHandlerFactory" />
        ///   that supports a provided <see cref="PropertyInfo" /> to create an instance
        ///   of <see cref="IPropertyHandler" />.
        /// </summary>
        /// <remarks>
        ///   In the event that two or more <see cref="IPropertyHandlerProvider" />
        ///   implementations support the same <see cref="PropertyInfo" />, the first
        ///   found in <paramref name="providers" /> will be used.
        /// </remarks>
        /// <param name="providers">
        ///   A collection of <see cref="IPropertyHandlerProvider" />
        ///   implementations that will be checked in order to see if any
        ///   of them support a given <see cref="PropertyInfo" /> for use
        ///   in creating an <see cref="IPropertyHandler" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="providers" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when one or more of the <see cref="IPropertyHandlerProvider" />
        ///   implementations within <paramref name="providers" /> is null.
        /// </exception>
        public AggregatePropertyHandlerProvider(IList<IPropertyHandlerProvider> providers) {
            if (providers == null) {
                throw new ArgumentNullException(nameof(providers));
            } else if (providers.Any(provider => provider == null)) {
                throw new ArgumentException(
                    $"One or more of the {nameof(providers)} was null.",
                    nameof(providers)
                );
            }

            this.providers = providers.ToImmutableList();
        }

        /// <summary>
        ///   Creates an instance of <see cref="AggregatePropertyHandlerProvider" />
        ///   that uses the collection of <see cref="IPropertyHandlerProvider" />
        ///   implementations injected into it to determine if it supports the creation of a
        ///   <see cref="IPropertyHandler" /> from a particular <see cref="PropertyInfo" />.
        /// </summary>
        /// <param name="providers">
        ///   A collection of <see cref="IPropertyHandlerProvider" />
        ///   implementations that will be checked in order to see if any
        ///   of them support a given <see cref="PropertyInfo" /> for use
        ///   in creating an <see cref="IPropertyHandler" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="providers" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when one or more of the <see cref="IPropertyHandlerProvider" />
        ///   implementations within <paramref name="providers" /> is null.
        /// </exception>
        public static AggregatePropertyHandlerProvider Create(
            params IPropertyHandlerProvider[] providers) {

            return new AggregatePropertyHandlerProvider(ImmutableList.Create(providers));
        }

        /// <summary>
        ///  Checks to see if the <see cref="PropertyInfo" /> is supported by any
        ///  of the <see cref="IPropertyHandlerProvider" /> instances injected
        ///  into this instance of <see cref="AggregatePropertyHandlerProvider" />.
        /// </summary>
        protected override bool IsSupportedImpl(PropertyInfo property) {
            return this.providers.Any(provider => provider.IsSupported(property));
        }

        /// <summary>
        ///   Creates an instance of <see cref="IPropertyHandler" /> using the
        ///   first <see cref="IPropertyHandlerProvider" /> found that supports is.
        /// </summary>
        protected override IPropertyHandler CreateImpl(PropertyInfo property) {
            return this.providers
                       .First(provider => provider.IsSupported(property))
                       .Create(property);
        }

    }

}
