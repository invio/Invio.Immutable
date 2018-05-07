using System;
using System.Collections.Generic;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class StringPropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Fact]
        public void IsSupported_StringProperty() {

            // Arrange

            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.True(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void IsSupported_NonStringProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.False(isSupported);
        }

        [Fact]
        public void Create_StringProperty() {

            // Arrange

            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));
            var provider = this.CreateStringProvider();

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<StringPropertyHandler>(handler);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void Create_NonStringProperties(String propertyName) {

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

        [Theory]
        [InlineData("Foo", "Foo")]
        [InlineData("FOO", "Foo")]
        public void Create_DefaultComparerIsOrdinal(String leftValue, String rightValue) {

            // Arrange

            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));
            var provider = this.CreateStringProvider();
            var handler = provider.Create(property);

            var comparer = StringComparer.Ordinal;

            var leftImmutable = new Fake(stringProperty: leftValue);
            var rightImmutable = new Fake(stringProperty: rightValue);

            // Act

            var leftValueHashCode = handler.GetPropertyValueHashCode(leftImmutable);
            var rightValueHashCode = handler.GetPropertyValueHashCode(rightImmutable);
            var equality = handler.ArePropertyValuesEqual(leftImmutable, rightImmutable);

            // Assert

            Assert.Equal(comparer.GetHashCode(leftValue), leftValueHashCode);
            Assert.Equal(comparer.GetHashCode(rightValue), rightValueHashCode);
            Assert.Equal(comparer.Equals(leftValue, rightValue), equality);
        }

        public static IEnumerable<object[]> Create_CustomComparer_MemberData {
            get {
                var properties = new List<Tuple<PropertyInfo, StringComparer, object>> {
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Annotated).GetProperty(nameof(Annotated.UsesClassAttribute)),
                        StringComparer.OrdinalIgnoreCase,
                        new Annotated()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Annotated).GetProperty(nameof(Annotated.UsesPropertyAttribute)),
                        StringComparer.CurrentCulture,
                        new Annotated()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Child).GetProperty(nameof(Child.UsesParentClassAttribute)),
                        StringComparer.OrdinalIgnoreCase,
                        new Child()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Child).GetProperty(nameof(Child.UsesChildPropertyAttribute)),
                        StringComparer.CurrentCultureIgnoreCase,
                        new Child()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Descendant).GetProperty(nameof(Descendant.UsesDescendantClassAttribute)),
                        StringComparer.Ordinal,
                        new Descendant()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Descendant).GetProperty(nameof(Descendant.UsesAncestorPropertyAttribute)),
                        StringComparer.CurrentCulture,
                        new Descendant()
                    ),
                    Tuple.Create<PropertyInfo, StringComparer, object>(
                        typeof(Descendant).GetProperty(nameof(Descendant.UsesDescendantPropertyAttribute)),
                        StringComparer.CurrentCultureIgnoreCase,
                        new Descendant()
                    )
                };

                foreach (var property in properties) {
                    yield return new object[] {
                        property.Item1,  // PropertyInfo
                        property.Item2,  // StringComparer
                        property.Item3,  // base SettableImmutableBase<TImmutable>
                        "Foo",
                        "FOO"
                    };

                    yield return new object[] {
                        property.Item1,  // PropertyInfo
                        property.Item2,  // StringComparer
                        property.Item3,  // base SettableImmutableBase<TImmutable>
                        "foo",
                        "foo"
                    };

                    yield return new object[] {
                        property.Item1,  // PropertyInfo
                        property.Item2,  // StringComparer
                        property.Item3,  // base SettableImmutableBase<TImmutable>
                        "Strasse",
                        "Straße"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Create_CustomComparer_MemberData))]
        public void CheckWithExpectedComparer<TImmutable>(
            PropertyInfo property,
            StringComparer comparer,
            TImmutable original,
            string leftValue,
            string rightValue) where TImmutable : SettableImmutableBase<TImmutable> {

            // Arrange

            var provider = this.CreateStringProvider();
            var handler = provider.Create(property);

            var leftImmutable = original.SetPropertyValue(property.Name, leftValue);
            var rightImmutable = original.SetPropertyValue(property.Name, rightValue);

            // Act

            var leftValueHashCode = handler.GetPropertyValueHashCode(leftImmutable);
            var rightValueHashCode = handler.GetPropertyValueHashCode(rightImmutable);
            var equality = handler.ArePropertyValuesEqual(leftImmutable, rightImmutable);

            // Assert

            Assert.Equal(comparer.GetHashCode(leftValue), leftValueHashCode);
            Assert.Equal(comparer.GetHashCode(rightValue), rightValueHashCode);
            Assert.Equal(comparer.Equals(leftValue, rightValue), equality);
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return this.CreateStringProvider();
        }

        private StringPropertyHandlerProvider CreateStringProvider() {
            return new StringPropertyHandlerProvider();
        }

        public abstract class SettableImmutableBase<TImmutable> : ImmutableBase<TImmutable>
            where TImmutable : SettableImmutableBase<TImmutable> {

            public TImmutable SetPropertyValue(string propertyName, object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

        private sealed class Fake : SettableImmutableBase<Fake> {

            public String StringProperty { get; }
            public Object ObjectProperty { get; }
            public Int32 Int32Property { get; }

            public Fake(
                String stringProperty = default(String),
                Object objectProperty = default(Object),
                Int32 int32Property = default(Int32)) {

                this.StringProperty = stringProperty;
                this.ObjectProperty = objectProperty;
                this.Int32Property = int32Property;
            }

        }

        [StringComparison(StringComparison.OrdinalIgnoreCase)]
        private sealed class Annotated : SettableImmutableBase<Annotated> {

            public String UsesClassAttribute { get; }

            [StringComparison(StringComparison.CurrentCulture)]
            public String UsesPropertyAttribute { get; }

            public Annotated(
                String usesClassAttribute = default(String),
                String usesPropertyAttribute = default(String)) {

                this.UsesClassAttribute = usesClassAttribute;
                this.UsesPropertyAttribute = usesPropertyAttribute;
            }

        }

        [StringComparison(StringComparison.OrdinalIgnoreCase)]
        private abstract class Parent<TChild> : SettableImmutableBase<TChild>
            where TChild : Parent<TChild> {

            public virtual String UsesParentClassAttribute { get; }

            [StringComparison(StringComparison.CurrentCulture)]
            public virtual String UsesChildPropertyAttribute { get; }

            protected Parent(
                String usesParentClassAttribute = default(String),
                String usesChildPropertyAttribute = default(String)) {

                this.UsesParentClassAttribute = usesParentClassAttribute;
                this.UsesChildPropertyAttribute = usesChildPropertyAttribute;
            }

        }

        private sealed class Child : Parent<Child> {

            [StringComparison(StringComparison.CurrentCultureIgnoreCase)]
            public override String UsesChildPropertyAttribute { get; }

            public Child(
                String usesParentClassAttribute = default(String),
                String usesChildPropertyAttribute = default(String)) :
                    base(usesParentClassAttribute, usesChildPropertyAttribute) {

                this.UsesChildPropertyAttribute = usesChildPropertyAttribute;
            }

        }

        [StringComparison(StringComparison.OrdinalIgnoreCase)]
        private abstract class Ancestor<TDescendant> : SettableImmutableBase<TDescendant>
            where TDescendant : Ancestor<TDescendant> {

            public String UsesDescendantClassAttribute { get; }

            [StringComparison(StringComparison.CurrentCulture)]
            public String UsesAncestorPropertyAttribute { get; }

            public Ancestor(
                String usesDescendantClassAttribute = default(String),
                String usesAncestorPropertyAttribute = default(String)) {

                this.UsesDescendantClassAttribute = usesDescendantClassAttribute;
                this.UsesAncestorPropertyAttribute = usesAncestorPropertyAttribute;
            }

        }

        [StringComparison(StringComparison.Ordinal)]
        private sealed class Descendant : Ancestor<Descendant> {

            [StringComparison(StringComparison.CurrentCultureIgnoreCase)]
            public String UsesDescendantPropertyAttribute { get; }

            public Descendant(
                String usesDescendantClassAttribute = default(String),
                String usesAncestorPropertyAttribute = default(String),
                String usesDescendantPropertyAttribute = default(String)) :
                    base(usesDescendantClassAttribute, usesAncestorPropertyAttribute) {

                this.UsesDescendantPropertyAttribute = usesDescendantPropertyAttribute;
            }

        }

    }

}
