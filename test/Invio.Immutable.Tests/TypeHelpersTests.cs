using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Invio.Immutable {

    public class TypeHelpersTests {

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

    }

}
