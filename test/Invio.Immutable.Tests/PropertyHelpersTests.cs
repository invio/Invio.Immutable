using System;
using Xunit;

namespace Invio.Immutable {

    public class PropertyHelpersTests {

        [Fact]
        public void GetProperties_Simple() {

            // Arrange & Act

            var properties = PropertyHelpers.GetProperties<Simple>();

            // Assert

            Assert.Equal(properties.Count, 2);

            Assert.Contains(
                properties,
                property => property.Name == nameof(Simple.Foo)
            );

            Assert.Contains(
                properties,
                property => property.Name == nameof(Simple.Bar)
            );
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
        public void GetProperties_IgnorePrivateGetter() {

            // Arrange & Act

            var properties = PropertyHelpers.GetProperties<PrivateGetter>();

            // Act

            Assert.Equal(properties.Count, 1);

            Assert.Contains(
                properties,
                property => property.Name == nameof(PrivateGetter.Foo)
            );
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
        public void GetProperties_IgnoreExclusiveSetters() {

            // Arrange & Act

            var properties = PropertyHelpers.GetProperties<PrivateGetter>();

            // Act

            Assert.Equal(properties.Count, 1);

            Assert.Contains(
                properties,
                property => property.Name == nameof(ExclusiveSetter.Foo)
            );
        }

        public class ExclusiveSetter : ImmutableBase<ExclusiveSetter> {

            public String hidden { set {} }
            public String Foo { get; }

            public ExclusiveSetter(String foo) {
                this.Foo = foo;
            }

        }

    }

}
