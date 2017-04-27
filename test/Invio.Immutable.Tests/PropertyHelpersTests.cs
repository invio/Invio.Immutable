using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Invio.Immutable {

    public class PropertyHelpersTests {

        [Fact]
        public void GetPropertyMap_Simple() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<Simple>();

            // Assert

            Assert.Equal(propertyMap.Count, 2);
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

            Assert.Equal(propertyMap.Count, 1);
            AssertContainsProperty(propertyMap, nameof(PrivateGetter.Foo));
        }

        public class PrivateGetter : ImmutableBase<PrivateGetter> {

            private String hidden { get; }
            public String Foo { get; }

            public PrivateGetter(String foo) {
                this.Foo = foo;
                this.hidden = (foo ?? String.Empty) + (foo ?? String.Empty);
            }

        }

        [Fact]
        public void GetPropertyMap_IgnoreExclusiveSetters() {

            // Arrange & Act

            var propertyMap = PropertyHelpers.GetPropertyMap<PrivateGetter>();

            // Assert

            Assert.Equal(propertyMap.Count, 1);
            AssertContainsProperty(propertyMap, nameof(ExclusiveSetter.Foo));
        }

        public class ExclusiveSetter : ImmutableBase<ExclusiveSetter> {

            public String hidden { set {} }
            public String Foo { get; }

            public ExclusiveSetter(String foo) {
                this.Foo = foo;
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
