using System;

namespace Invio.Immutable {

    /// <summary>
    ///   This interface encapsulates the logic for how to user-friendly
    ///   display <see cref="String" /> for a given object.
    /// </summary>
    public interface IStringFormatter {

        /// <summary>
        ///   Converts the value provided via the <paramref name="value" />
        ///   parameter into a user-friendly display <see cref="String" />.
        /// </summary>
        /// <remarks>
        ///   This is intended for use on property values on instances of
        ///   <see cref="ImmutableBase{TImmutable}" />.
        /// </remarks>
        /// <param name="value">
        ///   The value of an object that needs to converted to a user-friendly
        ///   display <see cref="String" />.
        /// </param>
        /// <returns>
        ///   A non-null <see cref="String" /> that can be shown to
        ///   a user who wishes to understand its value.
        /// </returns>
        string Format(object value);


    }

}
