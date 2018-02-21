using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   A base <see cref="IPropertyHandlerProvider" /> implementation that
    ///   standardizes validation and exception messaging that cannot otherwise
    ///   be eliminated at compile time.
    /// </summary>
    public abstract class PropertyHandlerProviderBase : IPropertyHandlerProvider {

        /// <summary>
        ///   Performs basic validation of the provided <see cref="PropertyInfo" />
        ///   before passing it on to the real, inheriting class's implementation.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property
        ///   that exists on a class that derives from
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <returns>
        ///   This returns <c>true</c> when the <paramref name="property" /> can be
        ///   passed to <see cref="IPropertyHandlerFactory.Create" /> in order to create
        ///   a <see cref="IPropertyHandler" />. Otherwise, <c>false</c> is returned.
        /// </returns>
        public bool IsSupported(PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }

            return this.IsSupportedImpl(property);
        }

        /// <summary>
        ///   The real implementation defined on the inheriting class that
        ///   determines whether or not a given <see cref="PropertyInfo" />
        ///   is supported by the <see cref="Create" /> method.
        /// </summary>
        protected virtual bool IsSupportedImpl(PropertyInfo property) {
            return true;
        }

        /// <summary>
        ///   Performs basic validation of the provided <see cref="PropertyInfo" />
        ///   before passing it on to the real, inheriting class's implementation.
        /// </summary>
        /// <param name="property">
        ///   A <see cref="PropertyInfo" /> that references a property
        ///   that exists on a class that derives from
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="property" /> is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Thrown when <paramref name="property" /> is either of a type,
        ///   or decorated in a style, that prevents it from being able to
        ///   be created from this <see cref="IPropertyHandlerFactory" />.
        /// </exception>
        /// <returns>
        ///   A <see cref="IPropertyHandler" /> that will calculate hash
        ///   codes and determine equality for property values stored
        ///   on instances of <see cref="ImmutableBase{TImmutable}" />.
        /// </returns>
        public IPropertyHandler Create(PropertyInfo property) {
            if (!this.IsSupported(property)) {
                throw new NotSupportedException(
                    $"The property '{property.Name}' is not supported."
                );
            }

            return this.CreateImpl(property);
        }

        /// <summary>
        ///   The real implementation defined on the inheriting class that
        ///   determines how to create an implementation of
        ///   <see cref="IPropertyHandler" /> from an instance of
        ///   <see cref="PropertyInfo" />.
        /// </summary>
        protected abstract IPropertyHandler CreateImpl(PropertyInfo property);

    }

}
