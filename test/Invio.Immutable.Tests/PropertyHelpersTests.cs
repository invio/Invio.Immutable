using System;
using System.Collections.Generic;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class PropertyHelpersTests {

        [Fact]
        public void GetPropertyMap_Simple() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<Simple>();

            // Assert

            Assert.Equal(2, propertyMap.Count);
            AssertContainsProperty(propertyMap, nameof(Simple.Foo));
            AssertContainsProperty(propertyMap, nameof(Simple.Bar));
        }

        public class Simple : ImmutableBase<Simple> {

            public String Foo { get; }
            public Guid Bar { get; }

            public Simple(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar;
            }

        }

        [Fact]
        public void GetPropertyMap_IgnorePrivateGetter() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<PrivateGetter>();

            // Assert

            Assert.Equal(1, propertyMap.Count);
            AssertContainsProperty(propertyMap, nameof(PrivateGetter.Foo));
        }

        public class PrivateGetter : ImmutableBase<PrivateGetter> {

            private String hidden { get; }
            public String Foo { get; }

            public PrivateGetter(String foo) {
                this.Foo = foo;
                this.hidden = Guid.NewGuid().ToString();
            }

        }

        [Fact]
        public void GetPropertyMap_IgnoreExclusiveSetters() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<ExclusiveSetter>();

            // Assert

            Assert.Equal(1, propertyMap.Count);
            AssertContainsProperty(propertyMap, nameof(ExclusiveSetter.Foo));
        }

        public class ExclusiveSetter : ImmutableBase<ExclusiveSetter> {

            public String Hidden { set {} }
            public String Foo { get; }

            public ExclusiveSetter(String foo) {
                this.Foo = foo;
            }

        }

        [Fact]
        public void GetPropertyMap_IgnoreCustomGetters() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<NonAutoProperties>();

            // Assert

            Assert.Equal(2, propertyMap.Count);
            AssertContainsProperty(propertyMap, nameof(NonAutoProperties.ValidGetterOnly));
            AssertContainsProperty(propertyMap, nameof(NonAutoProperties.ValidWithSetter));
        }

        public sealed class NonAutoProperties : ImmutableBase<NonAutoProperties> {

            public String ValidGetterOnly { get; }
            public String ValidWithSetter { get; set; }

            public String CustomGetter { get { return String.Empty; } }
            public bool CustomLambda => this.ValidGetterOnly == null;

            public int WrappedField {
                get { return this.wrapped; }
                set { this.wrapped = value;}
            }

            private int wrapped;

            public NonAutoProperties(String validGetterOnly, String validWithSetter) {
                this.ValidGetterOnly = validGetterOnly;
                this.ValidWithSetter = validWithSetter;
            }
        }

        [Fact]
        public void GetPropertyMap_ThrowsOnDuplicatePropertyNames() {

            // Act

            var exception = Record.Exception(
                () => PropertyHelpers.GetPropertyMap<DuplicatePropertyNames>()
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);

            Assert.Equal(
                $"ImmutableBase<DuplicatePropertyNames> requires property " +
                $"names to be unique regardless of case, but two or more " +
                $"properties share the name of 'foo'.",
                exception.Message,
                ignoreCase: true
            );
        }

        private class DuplicatePropertyNames : ImmutableBase<DuplicatePropertyNames> {

            public String Foo { get; }
            public String FOO { get; }

            public DuplicatePropertyNames(String foo, String fOO) {
                this.Foo = foo;
                this.FOO = fOO;
            }

        }

        private static void AssertContainsProperty(
            IDictionary<String, PropertyInfo> propertyMap,
            String propertyName) {

            Assert.NotNull(propertyMap);
            Assert.NotNull(propertyName);

            Assert.True(
                propertyMap.ContainsKey(propertyName),
                String.Format(
                    "The expected propertyName '{0}' was not found in propertyMap [ {1} ].",
                    propertyName,
                    String.Join(", ", propertyMap.Keys)
                )
            );

            Assert.Equal(propertyName, propertyMap[propertyName].Name);

            Assert.True(
                propertyMap.ContainsKey(propertyName.ToLower()),
                String.Format(
                    "The expected propertyName '{0}' was not found in propertyMap [ {1} ].",
                    propertyName.ToLower(),
                    String.Join(", ", propertyMap.Keys)
                )
            );

            Assert.True(
                propertyMap.ContainsKey(propertyName.ToUpper()),
                String.Format(
                    "The expected propertyName '{0}' was not found in propertyMap [ {1} ].",
                    propertyName.ToUpper(),
                    String.Join(", ", propertyMap.Keys)
                )
            );
        }

    }

}
