using System;
using System.Linq;
using System.Reflection;
using Invio.Xunit;
using Xunit;

using static Invio.Immutable.ConstructorHelpers;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class ConstructorTests {

        [Fact]
        public void GetImmutableSetterConstructor_NullPropertyMap() {

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<ValidTrivialExample>(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        private class ValidTrivialExample : ImmutableBase<ValidTrivialExample> {

            public String Foo { get; }

            public ValidTrivialExample(String foo) {
                this.Foo = foo;
            }

        }

        [Fact]
        public void ThrowsOnMissingConstructor_TooFewParameters() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<TooFewParameters>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<TooFewParameters>(propertyMap)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);

            Assert.StartsWith(
                "The TooFewParameters class lacks a constructor which is " +
                "compatible with the following signature: TooFewParameters(",
                exception.Message
            );

            Assert.Contains("String Foo", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class TooFewParameters : ImmutableBase<TooFewParameters> {

            public String Foo { get; }
            public Guid Bar { get; }

            public TooFewParameters(String foo) {
                this.Foo = foo;
            }

        }

        [Fact]
        public void ThrowsOnMissingConstructor_TooManyParameters() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<TooFewParameters>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<TooManyParameters>(propertyMap)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);

            Assert.StartsWith(
                "The TooManyParameters class lacks a constructor which is " +
                "compatible with the following signature: TooManyParameters(",
                exception.Message
            );

            Assert.Contains("String Foo", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class TooManyParameters : ImmutableBase<TooManyParameters> {

            public String Foo { get; }
            public Guid Bar { get; }

            public TooManyParameters(String foo, Guid bar, int bizz) {
                this.Foo = foo;
                this.Bar = bar;
            }

        }

        [Fact]
        public void ThrowsOnMissingConstructor_MismatchedParameterName() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<MismatchedParameterName>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<MismatchedParameterName>(propertyMap)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);

            Assert.StartsWith(
                "The MismatchedParameterName class lacks a constructor which is " +
                "compatible with the following signature: MismatchedParameterName(",
                exception.Message
            );

            Assert.Contains("String Foo", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class MismatchedParameterName : ImmutableBase<MismatchedParameterName> {

            public String Foo { get; }
            public Guid Bar { get; }

            public MismatchedParameterName(String foo, Guid bizz) {
                this.Foo = foo;
                this.Bar = bizz;
            }

        }

        [Fact]
        public void ThrowsOnMissingConstructor_MismatchedParameterType() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<MismatchedParameterType>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<MismatchedParameterType>(propertyMap)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);

            Assert.StartsWith(
                "The MismatchedParameterType class lacks a constructor which is " +
                "compatible with the following signature: MismatchedParameterType(",
                exception.Message
            );

            Assert.Contains("String Foo", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("String Bar", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class MismatchedParameterType : ImmutableBase<MismatchedParameterType> {

            public String Foo { get; }
            public String Bar { get; }

            public MismatchedParameterType(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar.ToString();
            }

        }

        [Fact]
        public void FindsMatching_WithManyConstructors() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<WithManyConstructors>();
            var expected = GetExpectedConstructor<WithManyConstructors>();

            // Act

            var actual = GetImmutableSetterConstructor<WithManyConstructors>(propertyMap);

            // Assert

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        private class WithManyConstructors : ImmutableBase<WithManyConstructors> {

            public String Foo { get; }
            public Guid Bar { get; }

            public WithManyConstructors(Tuple<String, Guid> tuple) :
                this(tuple.Item1, tuple.Item2) {}

            [Expected]
            public WithManyConstructors(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar;
            }

            public WithManyConstructors SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        [Fact]
        public void FindsMatching_WithPrivateConstructor() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<WithPrivateConstructor>();
            var expected = GetExpectedConstructor<WithPrivateConstructor>();

            // Act

            var actual = GetImmutableSetterConstructor<WithPrivateConstructor>(propertyMap);

            // Assert

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        private class WithPrivateConstructor : ImmutableBase<WithPrivateConstructor> {

            public static WithPrivateConstructor Empty { get; } =
                new WithPrivateConstructor();

            public String Foo { get; }
            public Guid Bar { get; }

            [Expected]
            private WithPrivateConstructor(
                String foo = default(String),
                Guid bar = default(Guid)) {

                this.Foo = foo;
                this.Bar = bar;
            }

            public WithPrivateConstructor SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        [Fact]
        public void FindsMatching_WithProtectedConstructor() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<WithProtectedConstructor>();
            var expected = GetExpectedConstructor<WithProtectedConstructor>();

            // Act

            var actual = GetImmutableSetterConstructor<WithProtectedConstructor>(propertyMap);

            // Assert

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        private class WithProtectedConstructor : ImmutableBase<WithProtectedConstructor> {

            public static WithProtectedConstructor Empty { get; } =
                new WithProtectedConstructor();

            public String Foo { get; }
            public Guid Bar { get; }

            [Expected]
            private WithProtectedConstructor(
                String foo = default(String),
                Guid bar = default(Guid)) {

                this.Foo = foo;
                this.Bar = bar;
            }

            public WithProtectedConstructor SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        [Fact]
        public void FindsMatching_IgnoresStaticMembers() {

            // Arrange

            var expected = GetExpectedConstructor<IgnoresStaticMembers>();
            var propertyMap = PropertyHelpers.GetPropertyMap<IgnoresStaticMembers>();

            // Act

            var actual = GetImmutableSetterConstructor<IgnoresStaticMembers>(propertyMap);

            // Assert

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        private class IgnoresStaticMembers : ImmutableBase<IgnoresStaticMembers> {

            public static String StaticProperty { get; }

            static IgnoresStaticMembers() {
                StaticProperty = "SetViaStaticConstructor";
            }

            public String Foo { get; }

            [Expected]
            public IgnoresStaticMembers(String foo) {
                this.Foo = foo;
            }

            public IgnoresStaticMembers SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        [Fact]
        public void DecoratedConstructor_TooManyDecorated() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<IgnoresStaticMembers>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<TooManyDecorated>(propertyMap)
            );

            // Assert

            Assert.IsType<InvalidOperationException>(exception);

            Assert.Equal(
                "The TooManyDecorated class has 2 constructors decorated " +
                "with the ImmutableSetterConstructorAttribute. Only one " +
                "constructor is allowed to have this attribute at a time.",
                exception.Message
            );
        }

        private class TooManyDecorated : ImmutableBase<TooManyDecorated> {

            public String Foo { get; }
            public Guid Bar { get; }

            [ImmutableSetterConstructor]
            public TooManyDecorated(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar;
            }

            [ImmutableSetterConstructor]
            public TooManyDecorated(Guid bar, String foo) {
                this.Foo = foo;
                this.Bar = bar;
            }

        }

        [Fact]
        public void DecoratedConstructor_DecoratedIncompatible() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<DecoratedIncompatible>();

            // Act

            var exception = Record.Exception(
                () => GetImmutableSetterConstructor<DecoratedIncompatible>(propertyMap)
            );

            // Assert

            Assert.IsType<InvalidOperationException>(exception);

            Assert.StartsWith(
                "The DecoratedIncompatible class has a constructor decorated " +
                "with ImmutableSetterConstructorAttribute that is incompatible " +
                "with the following signature: DecoratedIncompatible(",
                exception.Message
            );

            Assert.Contains("String Foo", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class DecoratedIncompatible : ImmutableBase<DecoratedIncompatible> {

            public String Foo { get; }
            public Guid Bar { get; }

            [ImmutableSetterConstructor]
            public DecoratedIncompatible(String foo) {
                this.Foo = foo;
            }

            public DecoratedIncompatible(String foo, Guid bar) {
                this.Foo = foo;
                this.Bar = bar;
            }

        }

        [Fact]
        public void DecoratedConstructor_ValidDecorated() {

            // Arrange

            var propertyMap = PropertyHelpers.GetPropertyMap<ValidDecorated>();
            var expected = GetExpectedConstructor<ValidDecorated>();

            // Act

            var actual = GetImmutableSetterConstructor<ValidDecorated>(propertyMap);

            // Assert

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        public class ValidDecorated : ImmutableBase<ValidDecorated> {

            public String Foo { get; }
            public Guid Bar { get; }
            public Int32 Bizz { get; }

            public ValidDecorated() {
                this.Foo = null;
                this.Bar = Guid.Empty;
                this.Bizz = 0;
            }

            public ValidDecorated(String foo, Guid bar, Int32 bizz) {
                this.Foo = foo;
                this.Bar = bar;
                this.Bizz = bizz;
            }

            public ValidDecorated(String foo, Int32 bizz, Guid bar) {
                this.Foo = foo;
                this.Bizz = bizz;
                this.Bar = bar;
            }

            public ValidDecorated(Guid bar, String foo, Int32 bizz) {
                this.Bar = bar;
                this.Foo = foo;
                this.Bizz = bizz;
            }

            [ImmutableSetterConstructor, Expected]
            public ValidDecorated(Guid bar, Int32 bizz, String foo) {
                this.Bizz = bizz;
                this.Bar = bar;
                this.Foo = foo;
            }

            public ValidDecorated(Int32 bizz, String foo, Guid bar) {
                this.Bizz = bizz;
                this.Foo = foo;
                this.Bar = bar;
            }

            public ValidDecorated(Int32 bizz, Guid bar, String foo) {
                this.Bizz = bizz;
                this.Bar = bar;
                this.Foo = foo;
            }

            public ValidDecorated SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        private static ConstructorInfo GetExpectedConstructor<TImmutable>() {
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            return typeof(TImmutable).GetConstructors(flags).Single(IsExpectedConstructor);
        }

        private static bool IsExpectedConstructor(ConstructorInfo constructor) {
            return constructor.GetCustomAttributes(typeof(ExpectedAttribute)).Any();
        }

        private class ExpectedAttribute : Attribute {}

    }

}
