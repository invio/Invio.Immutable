using System;
using System.Reflection;

namespace Invio.Immutable {

    /// <summary>
    ///   <see cref="Attribute" /> to indicate that the decorated
    ///   constructor is the one that should be used via
    ///   <see cref="ImmutableBase{TImmutable}.SetPropertyValueImpl" />
    ///   to create instances of an objects that inherit from
    ///   <see cref="ImmutableBase{TImmutable}" />.
    /// </summary>
    /// <remarks>
    ///   If this is placed on a static constructor, it will be ignored.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class ImmutableSetterConstructorAttribute : Attribute {}

}
