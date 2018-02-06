using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public abstract class EnumerablePropertyHandlerTestsBase : PropertyHandlerTestsBase {

        private static IEnumerable<String> propertyNames { get; }

        static EnumerablePropertyHandlerTestsBase() {
            propertyNames = ImmutableList.Create(
                nameof(FakeImmutable.Strings),
                nameof(FakeImmutable.Enumerable)
            );
        }

        public static IEnumerable<object[]> GetPropertyHandlerDisplayString_Empty_MemberData {
            get {
                yield return new object[] { nameof(FakeImmutable.Strings), new string[0] };
                yield return new object[] { nameof(FakeImmutable.Enumerable), new string[0] };
                yield return new object[] { nameof(FakeImmutable.Enumerable), new object[0] };
            }
        }

        [Fact]
        public void Constructor_NullProperty() {

            // Act

            var exception = Record.Exception(
                () => this.CreateHandler(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NonEnumerableProperty() {

            // Arrange

            var propertyName = nameof(FakeImmutable.NonEnumerable);
            var property = typeof(FakeImmutable).GetProperty(propertyName);

            // Act

            var exception = Record.Exception(
                () => this.CreateHandler(property)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                $"The '{propertyName}' property is not of type 'IEnumerable'." +
                Environment.NewLine + "Parameter name: property",
                exception.Message
            );
        }

        private IPropertyHandler CreateHandler(string propertyName) {
            return this.CreateHandler(typeof(FakeImmutable).GetProperty(propertyName));
        }

        private FakeImmutable NextFake() {
            return new FakeImmutable(
                ImmutableList.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                ImmutableHashSet.Create(Guid.NewGuid().ToString())
            );
        }

        private class FakeImmutable : ImmutableBase<FakeImmutable> {

            public IEnumerable<string> Strings { get; }
            public IEnumerable Enumerable { get; }
            public object NonEnumerable { get; }

            public FakeImmutable(
                IEnumerable<string> strings = default(IEnumerable<string>),
                IEnumerable enumerable = default(IEnumerable),
                object nonEnumerable = default(object)) {

                this.Strings = strings;
                this.Enumerable = enumerable;
                this.NonEnumerable = nonEnumerable;
            }

            public FakeImmutable SetPropertyValue(String propertyName, Object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

    }

}
