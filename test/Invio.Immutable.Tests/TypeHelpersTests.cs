using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class TypeHelpersTests {

        [Fact]
        public void IsImplementingOpenGenericInterface_NullType() {

            // Arrange

            Type type = null;

            // Act

            var exception = Record.Exception(
                () => type.IsImplementingOpenGenericInterface(typeof(IEnumerable<>))
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void IsImplementingOpenGenericInterface_NullInterfaceType() {

            // Arrange

            var type = typeof(List<object>);

            // Act

            var exception = Record.Exception(
                () => type.IsImplementingOpenGenericInterface(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void IsImplementingOpenGenericInterface_NotAGenericType() {

            // Arrange

            var type = typeof(List<object>);
            var argument = typeof(IEnumerable);

            // Act

            var exception = Record.Exception(
                () => type.IsImplementingOpenGenericInterface(argument)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The 'openGenericInterface' argument 'IEnumerable' is not a generic Type." +
                Environment.NewLine + "Parameter name: openGenericInterface",
                exception.Message
            );
        }

        [Fact]
        public void IsImplementingOpenGenericInterface_NotAnOpenGeneric() {

            // Arrange

            var type = typeof(List<object>);
            var argument = typeof(IEnumerable<object>);

            // Act

            var exception = Record.Exception(
                () => type.IsImplementingOpenGenericInterface(argument)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The 'openGenericInterface' argument " +
                "'IEnumerable`1' is not an open generic Type." +
                Environment.NewLine + "Parameter name: openGenericInterface",
                exception.Message
            );
        }

        [Fact]
        public void IsImplementingOpenGenericInterface_NotAnInterface() {

            // Arrange

            var type = typeof(List<object>);
            var argument = typeof(List<>);

            // Act

            var exception = Record.Exception(
                () => type.IsImplementingOpenGenericInterface(argument)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The 'openGenericInterface' argument 'List`1' is not an interface." +
                Environment.NewLine + "Parameter name: openGenericInterface",
                exception.Message
            );
        }

        public static IEnumerable<object> ValidCheckArguments {
            get {
                return new List<object> {
                    new object[] { typeof(IEnumerable<>), typeof(IEnumerable<>) },
                    new object[] { typeof(IEnumerable<object>), typeof(IEnumerable<>) },
                    new object[] { typeof(IList<object>), typeof(IEnumerable<>) },
                    new object[] { typeof(IList<object>), typeof(IList<>) },
                    new object[] { typeof(HashSet<string>), typeof(IEnumerable<>) },
                    new object[] { typeof(HashSet<string>), typeof(ISet<>) },
                    new object[] { typeof(List<string>), typeof(IList<>) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCheckArguments))]
        public void IsImplementingOpenGenericInterface_ValidChecks(
            Type type,
            Type openGenericInterface) {

            // Arrange

            var result = type.IsImplementingOpenGenericInterface(openGenericInterface);

            // Assert

            Assert.True(result);
        }

        public static IEnumerable<object> InvalidCheckArguments {
            get {
                return new List<object> {
                    new object[] { typeof(IEnumerable<object>), typeof(IList<>) },
                    new object[] { typeof(HashSet<object>), typeof(IList<>) },
                    new object[] { typeof(List<string>), typeof(ISet<>) },
                    new object[] { typeof(LinkedList<object>), typeof(ISet<>) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidCheckArguments))]
        public void IsImplementingOpenGenericInterface_InvalidChecks(
            Type type,
            Type openGenericInterface) {

            // Arrange

            var result = type.IsImplementingOpenGenericInterface(openGenericInterface);

            // Assert

            Assert.False(result);
        }

        [Fact]
        public void CreateSetEqualsFunc_NullSet() {

            // Arrange

            Type type = null;

            // Act

            var exception = Record.Exception(
                () => type.CreateSetEqualsFunc()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CreateSetEqualsFunc_NotASet() {

            // Arrange

            var type = typeof(Object);

            // Act

            var exception = Record.Exception(
                () => type.CreateSetEqualsFunc()
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The 'type' provided does not implement ISet<>." +
                Environment.NewLine + "Parameter name: type",
                exception.Message
            );
        }

        public static IEnumerable<object> SetEqualArguments {
            get {
                return new List<object> {
                    new object[] {
                        new HashSet<String> { "foo", "foo", "foo" },
                        new HashSet<String> { "foo", "foo" }
                    },
                    new object[] {
                        new HashSet<String> { "foo", null },
                        new HashSet<String> { null, "foo", "foo" }
                    },
                    new object[] {
                        new HashSet<String> { "foo", "bar", "foo" },
                        new HashSet<String> { "bar", "foo", "bar" }
                    },
                    new object[] {
                        new HashSet<Object> { "foo" },
                        new HashSet<Object> { "foo" }
                    },
                    new object[] {
                        new HashSet<Object> { null, null },
                        new HashSet<Object> { null, null, null }
                    },
                    new object[] {
                        new HashSet<DateTime> { DateTime.MinValue },
                        new HashSet<DateTime> { DateTime.MinValue }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(SetEqualArguments))]
        public void CreateSetEqualsFunc_Equal<T>(ISet<T> left, ISet<T> right) {

            // Arrange

            var type = typeof(ISet<T>);

            // Act

            var func = type.CreateSetEqualsFunc();
            var areEqual = func(left, right);

            // Assert

            Assert.True(areEqual);
        }

        [Fact]
        public void CreateSetEqualsFunc_Equal_Null() {

            // Arrange

            var type = typeof(ISet<String>);

            // Act

            var func = type.CreateSetEqualsFunc();
            var areEqual = func(null, null);

            // Assert

            Assert.True(areEqual);
        }

        public static IEnumerable<object> SetNotEqualArguments {
            get {
                return new List<object> {
                    new object[] {
                        new HashSet<String> { "foo", "bar", "foo" },
                        new HashSet<String> { "bar", "foo", "biz" }
                    },
                    new object[] {
                        new HashSet<Object> { "foo" },
                        new HashSet<Object> { "FOO" }
                    },
                    new object[] {
                        new HashSet<Object> { null },
                        new HashSet<Object> { "FOO" }
                    },
                    new object[] {
                        new HashSet<Object> { "foo" },
                        new HashSet<Object> { null }
                    },
                    new object[] {
                        new HashSet<DateTime> { DateTime.MinValue },
                        new HashSet<DateTime> { DateTime.MaxValue }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(SetNotEqualArguments))]
        public void CreateSetEqualsFunc_NotEqual<T>(ISet<T> left, ISet<T> right) {

            // Arrange

            var type = typeof(ISet<T>);

            // Act

            var func = type.CreateSetEqualsFunc();
            var areEqual = func(left, right);

            // Assert

            Assert.False(areEqual);
        }

        [Fact]
        public void CreateSetEqualsFunc_NotEqual_Null() {

            // Arrange

            var type = typeof(ISet<String>);
            var @set = new HashSet<String> { "foo" };

            // Act

            var func = type.CreateSetEqualsFunc();
            var areEqualLeftNull = func(null, @set);
            var areEqualRightNull = func(@set, null);

            // Assert

            Assert.False(areEqualLeftNull);
            Assert.False(areEqualRightNull);
        }

        [Fact]
        public void CreateEnumerableEqualsFunc_Null() {

            // Arrange

            Type type = null;

            // Act

            var exception = Record.Exception(
                () => type.CreateEnumerableEqualsFunc()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CreateEnumerableEqualsFunc_NotAnIEnumerable() {


            // Arrange

            var type = typeof(Object);

            // Act

            var exception = Record.Exception(
                () => type.CreateEnumerableEqualsFunc()
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The 'type' provided does not implement IEnumerable." +
                Environment.NewLine + "Parameter name: type",
                exception.Message
            );
        }

        public static IEnumerable<object> EnumerableEqualArguments {
            get {
                return new List<object> {
                    new object[] {
                        new List<String> { "foo", "foo" },
                        new List<String> { "foo", "foo" }
                    },
                    new object[] {
                        new List<String> { "foo", "bar" },
                        new List<String> { "foo", "bar" }
                    },
                    new object[] {
                        new Object[] { "foo" },
                        new Object[] { "foo" }
                    },
                    new object[] {
                        new Object[] { null },
                        new Object[] { null }
                    },
                    new object[] {
                        ImmutableList.Create<DateTime>(DateTime.MinValue),
                        ImmutableList.Create<DateTime>(DateTime.MinValue)
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(EnumerableEqualArguments))]
        public void CreateEnumerableEqualsFunc_Equal<T>(
            IEnumerable<T> left,
            IEnumerable<T> right) {

            // Arrange

            var type = typeof(IEnumerable<T>);

            // Act

            var func = type.CreateEnumerableEqualsFunc();
            var areEqual = func(left, right);

            // Assert

            Assert.True(areEqual);
        }

        [Fact]
        public void CreateEnumerableEqualsFunc_Equal_Null() {

            // Arrange

            var type = typeof(ISet<String>);

            // Act

            var func = type.CreateEnumerableEqualsFunc();
            var areEqual = func(null, null);

            // Assert

            Assert.True(areEqual);
        }

        public static IEnumerable<object> EnumerableNotEqualArguments {
            get {
                return new List<object> {
                    new object[] {
                        new List<String> { "foo", "bar" },
                        new List<String> { "bar", "foo" }
                    },
                    new object[] {
                        new Object[] { new Object() },
                        new Object[] { new Object() }
                    },
                    new object[] {
                        new Object[] { null },
                        new Object[] { new Object() }
                    },
                    new object[] {
                        new Object[] { new Object() },
                        new Object[] { null }
                    },
                    new object[] {
                        ImmutableList.Create<DateTime>(DateTime.MinValue, DateTime.MinValue),
                        ImmutableList.Create<DateTime>(DateTime.MinValue)
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(EnumerableNotEqualArguments))]
        public void CreateEnumerableEqualsFunc_NotEqual(
            IEnumerable left,
            IEnumerable right) {

            // Arrange

            var type = typeof(IEnumerable);

            // Act

            var func = type.CreateEnumerableEqualsFunc();
            var areEqual = func(left, right);

            // Assert

            Assert.False(areEqual);
        }

        [Fact]
        public void CreateEnumerableEqualsFunc_NotEqual_Null() {

            // Arrange

            var type = typeof(ISet<String>);
            var enumerable = new List<String> { "foo" };

            // Act

            var func = type.CreateEnumerableEqualsFunc();
            var areEqualLeftNull = func(null, enumerable);
            var areEqualRightNull = func(enumerable, null);

            // Assert

            Assert.False(areEqualLeftNull);
            Assert.False(areEqualRightNull);
        }

    }

}
