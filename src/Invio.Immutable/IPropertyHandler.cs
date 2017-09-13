using System;

namespace Invio.Immutable {

    /// <summary>
    ///   This interfaces abstracts the management of a specific property on a value object.
    /// </summary>
    /// <remarks>
    ///   A value object is an instance of a class whose equality is not represented
    ///   by referential identity, but by the value of each of its properties.
    ///   The <see cref="IPropertyHandler" /> interface aims to abstract how each
    ///   property's value should be utilized when performing tasks such as equality
    ///   comparison, hash code generation, and string representation for the value
    ///   objects that contain them.
    /// </remarks>
    public interface IPropertyHandler {

        /// <summary>
        ///   The name of the abstracted property that exists on the value object.
        /// </summary>
        String PropertyName { get; }

        /// <summary>
        ///   The type of the abstracted property that exists on the value object.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        ///   Determines whether the values for the abstracted property are equal
        ///   on two value objects that contain them.
        /// </summary>
        /// <param name="leftParent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <param name="rightParent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="leftParent" /> or
        ///   <paramref name="rightParent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when either <paramref name="leftParent" /> or
        ///   <paramref name="rightParent" /> does not contains the abstracted property.
        /// </exception>
        /// <returns>
        ///   Whether or not the values for the abstracted property are
        ///   equal for the two value objects provided that contain them.
        /// </returns>
        bool ArePropertyValuesEqual(object leftParent, object rightParent);

        /// <summary>
        ///   Generates a hash code for the value of the abstracted property using the
        ///   value stored on the value object provided via <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain the abstracted property.
        /// </exception>
        /// <returns>
        ///   An appropriate hash code for the value of the abstracted property
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        int GetPropertyValueHashCode(object parent);

        /// <summary>
        ///   Gets the value for the abstracted property off of the
        ///   provided <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain the abstracted property.
        /// </exception>
        /// <returns>
        ///   The value of the abstracted property found on
        ///   the value object provided via <paramref name="parent" />.
        /// </returns>
        Object GetPropertyValue(object parent);

        /// <summary>
        ///   Generates a <see cref="String" /> representation for the abstracted property
        ///   using the value stored on the value object provided via
        ///   <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">
        ///   A value object that contains the abstracted property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown when <paramref name="parent" /> does not contain the abstracted property.
        /// </exception>
        /// <returns>
        ///   An appropriate string representation for the value of the abstracted property
        ///   found on the value object provided via <paramref name="parent" />.
        /// </returns>
        String GetPropertyValueDisplayString(object parent);

    }

}
