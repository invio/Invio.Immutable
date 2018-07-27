using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Invio.Extensions.Reflection;

namespace Invio.Immutable {
    /// <summary>
    ///   This class wraps a <see cref="PropertyHandlerProviderBase{TProperty}" /> where a
    ///   <typeparamref name="TProperty" /> is a value type, and creates wrapped
    ///   <see cref="IPropertyHandler" /> that support <see cref="Nullable{TProperty}" />.
    /// </summary>
    /// <typeparam name="TProperty">
    ///   The value type that the inner <see cref="PropertyHandlerProviderBase{TProperty}" />
    ///   supports.
    /// </typeparam>
    public class NullablePropertyHandlerProvider<TProperty> :
        PropertyHandlerProviderBase<TProperty?>
        where TProperty : struct {

        private PropertyHandlerProviderBase<TProperty> innerProvider { get; }

        /// <summary>
        ///   Creates a new instance of <see cref="NullablePropertyHandlerProvider{TProperty}" />
        ///   for the specified <see cref="PropertyHandlerProviderBase{TProperty}" />.
        /// </summary>
        /// <param name="innerProvider">
        ///   The inner <see cref="PropertyHandlerProviderBase{TProperty}" /> to wrap.
        /// </param>
        public NullablePropertyHandlerProvider(
            PropertyHandlerProviderBase<TProperty> innerProvider) {

            this.innerProvider = innerProvider;
        }

        /// <summary>
        ///   Calls <see cref="IsSupportedImpl" /> on the inner
        ///   <see cref="PropertyHandlerProviderBase{TProperty}"/> used to construct this instance
        ///   using a <see cref="PropertyInfo" /> implementation that automatically unwraps
        ///   <see cref="Nullable{TProperty}" /> instances.
        /// </summary>
        /// <inheritdoc />
        protected override bool IsSupportedImpl(PropertyInfo property) {
            return base.IsSupportedImpl(property) &&
                this.innerProvider.IsSupported(new NullableUnwrappingPropertyInfo(property));
        }

        /// <inheritdoc />
        protected override IPropertyHandler CreateImpl(
            PropertyInfo property) {

            return new NullablePropertyHandler<TProperty>(
                this.innerProvider.Create(new NullableUnwrappingPropertyInfo(property)),
                property
            );
        }

        private class NullableUnwrappingPropertyInfo : PropertyInfo {
            private PropertyInfo innerProperty { get; }

            public override PropertyAttributes Attributes => this.innerProperty.Attributes;
            public override Boolean CanRead => this.innerProperty.CanRead;
            public override Boolean CanWrite => this.innerProperty.CanWrite;
            public override Type PropertyType { get; }
            public override Type DeclaringType => this.innerProperty.DeclaringType;
            public override String Name => this.innerProperty.Name;
            public override Type ReflectedType => this.innerProperty.ReflectedType;

            public NullableUnwrappingPropertyInfo(PropertyInfo innerProperty) {
                var underlyingType = Nullable.GetUnderlyingType(innerProperty.PropertyType);
                if (underlyingType == null) {
                    throw new ArgumentException(
                        $"The specified property ('{innerProperty.Name}') does not have a " +
                        $"nullable type: " +
                        $"{innerProperty.PropertyType.GetNameWithGenericParameters()}",
                        nameof(innerProperty)
                    );
                }

                this.innerProperty = innerProperty;
                this.PropertyType = underlyingType;
            }

            public override Object[] GetCustomAttributes(Boolean inherit) {
                return this.innerProperty.GetCustomAttributes(inherit);
            }

            public override Object[] GetCustomAttributes(Type attributeType, Boolean inherit) {
                return this.innerProperty.GetCustomAttributes(attributeType, inherit);
            }

            public override MethodInfo[] GetAccessors(Boolean nonPublic) {
                // return this.GetAccessorsImpl(nonPublic).ToArray();
                return this.innerProperty.GetAccessors(nonPublic);
            }

            public override MethodInfo GetGetMethod(Boolean nonPublic) {
                // Cast from nullable to non-nullable is implicit because the
                // nullable gets boxed first.
                return this.innerProperty.GetGetMethod(nonPublic);
            }

            public override MethodInfo GetSetMethod(Boolean nonPublic) {
                // Cast from non-nullable to nullable is implicit
                return this.innerProperty.GetSetMethod(nonPublic);
            }

            public override ParameterInfo[] GetIndexParameters() {
                return this.innerProperty.GetIndexParameters();
            }

            public override Object GetValue(
                Object obj,
                BindingFlags invokeAttr,
                Binder binder,
                Object[] index,
                CultureInfo culture) {

                var value =
                    (TProperty?)this.innerProperty.GetValue(
                        obj,
                        invokeAttr,
                        binder,
                        index,
                        culture
                    );

                // Raises an exception when null. Null values should be handled before this is
                // accessed.
                return value.Value;
            }

            public override void SetValue(
                Object obj,
                Object value,
                BindingFlags invokeAttr,
                Binder binder,
                Object[] index,
                CultureInfo culture) {

                this.innerProperty.SetValue(
                    obj,
                    (TProperty?)(TProperty)value,
                    invokeAttr,
                    binder,
                    index,
                    culture
                );
            }

            public override Boolean IsDefined(Type attributeType, Boolean inherit) {
                return this.innerProperty.IsDefined(attributeType, inherit);
            }
        }
    }
}