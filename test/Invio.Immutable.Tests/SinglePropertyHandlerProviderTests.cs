using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class SinglePropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Fact]
        public void IsSupported_SingleProperty() {

            // Arrange

            var property =
                ReflectionHelper<Annotated>.GetProperty(obj => obj.UsesPropertyAttribute);
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.True(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void IsSupported_NonSingleProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.False(isSupported);
        }

        [Fact]
        public void Create_SingleProperty() {

            // Arrange

            var property =
                ReflectionHelper<Annotated>.GetProperty(obj => obj.UsesPropertyAttribute);
            var provider = this.CreateSingleProvider();

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<SinglePropertyHandler>(handler);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void Create_NonSingleProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var exception = Record.Exception(
                () => provider.Create(property)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);
        }

        private static PropertyInfo UsesPropertyAttributeProperty { get; } =
            ReflectionHelper<Annotated>.GetProperty(obj => obj.UsesPropertyAttribute);

        private static PropertyInfo AncestorUsesClassAttribute { get; } =
            ReflectionHelper<Ancestor>.GetProperty(obj => obj.UsesClassAttribute);
        private static PropertyInfo AncestorVirtualUsesClassAttribute { get; } =
            ReflectionHelper<Ancestor>.GetProperty(obj => obj.VirtualUsesClassAttribute);
        private static PropertyInfo AncestorUsesPropertyAttribute { get; } =
            ReflectionHelper<Ancestor>.GetProperty(obj => obj.UsesPropertyAttribute);
        private static PropertyInfo AncestorVirtualUsesPropertyAttribute { get; } =
            ReflectionHelper<Ancestor>.GetProperty(obj => obj.VirtualUsesPropertyAttribute);

        // ReflectionHelper does not get the correct PropertyInfo for virtual properties
        private static PropertyInfo DescendantUsesClassAttribute { get; } =
            typeof(Descendant).GetProperty(nameof(Descendant.UsesClassAttribute));
        private static PropertyInfo DescendantVirtualUsesClassAttribute { get; } =
            typeof(Descendant).GetProperty(nameof(Descendant.VirtualUsesClassAttribute));
        private static PropertyInfo DescendantUsesPropertyAttribute { get; } =
            typeof(Descendant).GetProperty(nameof(Descendant.UsesPropertyAttribute));
        private static PropertyInfo DescendantVirtualUsesPropertyAttribute { get; } =
            typeof(Descendant).GetProperty(nameof(Descendant.VirtualUsesPropertyAttribute));

        public static IEnumerable<Object[]> Create_PropertyHandler_MemberData { get; } =
            ImmutableList.Create(
                new Object[] { nameof(Annotated), UsesPropertyAttributeProperty, 5, PrecisionStyle.DecimalPlaces },
                new Object[] { nameof(Ancestor), AncestorUsesClassAttribute, 8, PrecisionStyle.DecimalPlaces },
                new Object[] { nameof(Ancestor), AncestorVirtualUsesClassAttribute, 8, PrecisionStyle.DecimalPlaces },
                new Object[] { nameof(Ancestor), AncestorUsesPropertyAttribute, 6, PrecisionStyle.SignificantFigures },
                new Object[] { nameof(Ancestor), AncestorVirtualUsesPropertyAttribute, 4, PrecisionStyle.SignificantFigures },
                new Object[] { nameof(Descendant), DescendantUsesClassAttribute, 10, PrecisionStyle.SignificantFigures },
                new Object[] { nameof(Descendant), DescendantVirtualUsesClassAttribute, 10, PrecisionStyle.SignificantFigures },
                new Object[] { nameof(Descendant), DescendantUsesPropertyAttribute, 6, PrecisionStyle.SignificantFigures },
                new Object[] { nameof(Descendant), DescendantVirtualUsesPropertyAttribute, 5, PrecisionStyle.DecimalPlaces }
            );

        [Theory]
        [MemberData(nameof(Create_PropertyHandler_MemberData))]
        public void VerifyPropertyHandlerSettings(
            String declaringType,
            PropertyInfo property,
            Int32 expectedPrecision,
            PrecisionStyle expectedPrecisionStyle) {

            // Arrange

            var provider = this.CreateSingleProvider();

            // Act

            var handler = provider.Create(property);

            // Assert

            var singleHandler = Assert.IsType<SinglePropertyHandler>(handler);
            Assert.Equal(expectedPrecision, singleHandler.Precision);
            Assert.Equal(expectedPrecisionStyle, singleHandler.PrecisionStyle);
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return this.CreateSingleProvider();
        }

        private SinglePropertyHandlerProvider CreateSingleProvider() {
            return new SinglePropertyHandlerProvider();
        }

        public abstract class SettableImmutableBase<TImmutable> : ImmutableBase<TImmutable>
            where TImmutable : SettableImmutableBase<TImmutable> {

            public TImmutable SetPropertyValue(string propertyName, object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

        private sealed class Fake : SettableImmutableBase<Fake> {

            public Object ObjectProperty { get; }
            public Int32 Int32Property { get; }

            public Fake(
                Object objectProperty = default(Object),
                Int32 int32Property = default(Int32)) {

                this.ObjectProperty = objectProperty;
                this.Int32Property = int32Property;
            }

        }

        private sealed class Annotated {
            [SingleComparison(5, PrecisionStyle = PrecisionStyle.DecimalPlaces)]
            public Single UsesPropertyAttribute { get; }

            public Single UsesDefaultHandler { get; }

            public Annotated(
                Single usesPropertyAttribute = default(Single),
                Single usesDefaultHandler = default(Single)) {

                this.UsesPropertyAttribute = usesPropertyAttribute;
                this.UsesDefaultHandler = usesDefaultHandler;
            }

        }

        [SingleComparison(8)]
        private class Ancestor {

            public Single UsesClassAttribute { get; }

            public virtual Single VirtualUsesClassAttribute { get; }

            [SingleComparison(6, PrecisionStyle.SignificantFigures)]
            public Single UsesPropertyAttribute { get; }

            [SingleComparison(4, PrecisionStyle.SignificantFigures)]
            public virtual Single VirtualUsesPropertyAttribute { get; }

            public Ancestor(
                Single usesClassAttribute = default(Single),
                Single virtualUsesClassAttribute = default(Single),
                Single usesPropertyAttribute = default(Single),
                Single virtualUsesPropertyAttribute = default(Single)) {

                this.UsesClassAttribute = usesClassAttribute;
                this.VirtualUsesClassAttribute = virtualUsesClassAttribute;
                this.UsesPropertyAttribute = usesPropertyAttribute;
                this.VirtualUsesPropertyAttribute = virtualUsesPropertyAttribute;
            }

        }

        [SingleComparison(10, PrecisionStyle.SignificantFigures)]
        private sealed class Descendant : Ancestor {
            public override Single VirtualUsesClassAttribute => base.VirtualUsesClassAttribute;

            [SingleComparison(5)]
            public override Single VirtualUsesPropertyAttribute => base.VirtualUsesPropertyAttribute;

            public Descendant(
                Single usesClassAttribute = default(Single),
                Single virtualUsesClassAttribute = default(Single),
                Single usesPropertyAttribute = default(Single),
                Single virtualUsesPropertyAttribute = default(Single)) :
                    base(usesClassAttribute, virtualUsesClassAttribute, usesPropertyAttribute, virtualUsesPropertyAttribute) {
            }

        }

    }

}
