using System;
using Xunit;

namespace Invio.Immutable {

    public class TypeInitializationTests {

        [Fact]
        public void ThrowsOnDuplicatePropertyNames() {

            // Act

            var exception = Record.Exception(
                () => new DuplicatePropertyNames("Foo", "Bar")
            );

            // Assert

            Assert.IsType<TypeInitializationException>(exception);

            Assert.NotNull(exception.InnerException);
            var inner = Assert.IsType<NotSupportedException>(exception.InnerException);

            Assert.Equal(
                $"ImmutableBase<DuplicatePropertyNames> requires property " +
                $"names to be unique regardless of case, but two or more " +
                $"properties share the name of 'foo'.",
                inner.Message,
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

    }

}
