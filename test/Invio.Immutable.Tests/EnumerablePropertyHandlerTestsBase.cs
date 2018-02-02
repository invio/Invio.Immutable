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

        [Theory]
        [MemberData(nameof(GetPropertyHandlerDisplayString_Empty_MemberData))]
        public void GetPropertyHandlerDisplayString_Empty(
            String propertyName,
            IEnumerable propertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = this.NextFake().SetPropertyValue(propertyName, propertyValue);

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Equal("[]", displayString);
        }

        public static IEnumerable<object[]> GetPropertyHandlerDisplayString_Single_MemberData {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.Strings),
                    ImmutableList.Create(Guid.NewGuid().ToString())
                };

                yield return new object[] {
                    nameof(FakeImmutable.Strings),
                    new string[] { null }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(new object[] { null })
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    new Guid[] { Guid.NewGuid() }
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyHandlerDisplayString_Single_MemberData))]
        public void GetPropertyHandlerDisplayString_Single(
            String propertyName,
            IEnumerable propertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = this.NextFake().SetPropertyValue(propertyName, propertyValue);
            var item = propertyValue.Cast<object>().Single();

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.Equal($"[ {item ?? "null"} ]", displayString);
        }

        public static IEnumerable<object[]> GetPropertyHandlerDisplayString_Many_MemberData {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.Strings),
                    ImmutableList.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
                };

                yield return new object[] {
                    nameof(FakeImmutable.Strings),
                    new string[] { null, "FOO", "bar", "biz", "gah", String.Empty }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<Guid?>(Guid.NewGuid(), null, Guid.NewGuid())
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    new object[] { "foo", Guid.NewGuid(), null, 35m, DateTime.UtcNow }
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetPropertyHandlerDisplayString_Many_MemberData))]
        public void GetPropertyHandlerDisplayString_Many(
            String propertyName,
            IEnumerable propertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var fake = this.NextFake().SetPropertyValue(propertyName, propertyValue);

            // Act

            var displayString = handler.GetPropertyValueDisplayString(fake);

            // Assert

            Assert.StartsWith("[ ", displayString);
            Assert.Contains(", ", displayString);
            Assert.EndsWith(" ]", displayString);

            foreach (var item in propertyValue) {
                Assert.Contains(item?.ToString() ?? "null", displayString);
            }
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
