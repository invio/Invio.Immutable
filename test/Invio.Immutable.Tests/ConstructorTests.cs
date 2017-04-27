using System;
using Xunit;

namespace Invio.Immutable {

    public class ConstructorTests {

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

        [Fact]
        public void ThrowsOnMissingConstructor_TooFewParameters() {

            // Arrange & Act

            var exception = Record.Exception(
                () => new TooFewParameters("Foo")
            );

            // Assert

            Assert.IsType<TypeInitializationException>(exception);

            Assert.NotNull(exception.InnerException);
            var inner = Assert.IsType<NotSupportedException>(exception.InnerException);

            Assert.StartsWith(
                "The TooFewParameters class lacks a constructor which is " +
                "compatible with the following signature: TooFewParameters(",
                inner.Message
            );

            Assert.Contains("String Foo", inner.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", inner.Message, StringComparison.OrdinalIgnoreCase);
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

            // Arrange & Act

            var exception = Record.Exception(
                () => new TooManyParameters("Foo", Guid.NewGuid(), 1337)
            );

            // Assert

            Assert.IsType<TypeInitializationException>(exception);

            Assert.NotNull(exception.InnerException);
            var inner = Assert.IsType<NotSupportedException>(exception.InnerException);

            Assert.StartsWith(
                "The TooManyParameters class lacks a constructor which is " +
                "compatible with the following signature: TooManyParameters(",
                inner.Message
            );

            Assert.Contains("String Foo", inner.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", inner.Message, StringComparison.OrdinalIgnoreCase);
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
        public void FindsMatching_WithManyConstructors() {

            // Arrange

            var originalBar = Guid.NewGuid();
            var original = new WithManyConstructors(Tuple.Create("Foo", originalBar));

            // Act

            const string updatedFoo = "Update";
            var updated = original.SetFoo(updatedFoo);

            // Assert

            Assert.Equal(updated.Foo, updatedFoo);
            Assert.Equal(updated.Bar, originalBar);
        }

        private class WithManyConstructors : ImmutableBase<WithManyConstructors> {

            public String Foo { get; }
            public Guid Bar { get; }

            public WithManyConstructors(Tuple<String, Guid> tuple) :
                this(tuple.Item1, tuple.Item2) {}

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

            var original = WithPrivateConstructor.Empty;

            // Act

            const string updatedFoo = "Foo";
            var updated = original.SetFoo(updatedFoo);

            // Assert

            Assert.Equal(updated.Foo, updatedFoo);
            Assert.Equal(updated.Bar, original.Bar);
        }

        private class WithPrivateConstructor : ImmutableBase<WithPrivateConstructor> {

            public static WithPrivateConstructor Empty { get; } =
                new WithPrivateConstructor();

            public String Foo { get; }
            public Guid Bar { get; }

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

            var original = WithProtectedConstructor.Empty;

            // Act

            const string updatedFoo = "Foo";
            var updated = original.SetFoo(updatedFoo);

            // Assert

            Assert.Equal(updated.Foo, updatedFoo);
            Assert.Equal(updated.Bar, original.Bar);
        }

        private class WithProtectedConstructor : ImmutableBase<WithProtectedConstructor> {

            public static WithProtectedConstructor Empty { get; } =
                new WithProtectedConstructor();

            public String Foo { get; }
            public Guid Bar { get; }

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

            var original = new IgnoresStaticMembers("OriginalFoo");

            // Act

            const string updatedFoo = "UpdatedFoo";
            var updated = original.SetFoo(updatedFoo);

            // Assert

            Assert.Equal(updated.Foo, updatedFoo);
            Assert.Equal(IgnoresStaticMembers.StaticProperty, "SetViaStaticConstructor");
        }

        private class IgnoresStaticMembers : ImmutableBase<IgnoresStaticMembers> {

            public static String StaticProperty { get; }

            static IgnoresStaticMembers() {
                StaticProperty = "SetViaStaticConstructor";
            }

            public String Foo { get; }

            public IgnoresStaticMembers(String foo) {
                this.Foo = foo;
            }

            public IgnoresStaticMembers SetFoo(String foo) {
                return this.SetPropertyValueImpl(nameof(Foo), foo);
            }

        }

        [Fact]
        public void DecoratedConstructor_TooManyDecorated() {

            // Arrange & Act

            var exception = Record.Exception(
                () => new TooManyDecorated("Foo", Guid.NewGuid())
            );

            // Assert

            Assert.IsType<TypeInitializationException>(exception);

            Assert.NotNull(exception.InnerException);
            var inner = Assert.IsType<InvalidOperationException>(exception.InnerException);

            Assert.Equal(
                "The TooManyDecorated class has 2 constructors decorated " +
                "with the ImmutableSetterConstructorAttribute. Only one " +
                "constructor is allowed to have this attribute at a time.",
                inner.Message
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

            // Arrange & Act

            var exception = Record.Exception(
                () => new DecoratedIncompatible("Foo", Guid.NewGuid())
            );

            // Assert

            Assert.IsType<TypeInitializationException>(exception);

            Assert.NotNull(exception.InnerException);
            var inner = Assert.IsType<InvalidOperationException>(exception.InnerException);

            Assert.StartsWith(
                "The DecoratedIncompatible class has a constructor decorated " +
                "with ImmutableSetterConstructorAttribute that is incompatible " +
                "with the following signature: DecoratedIncompatible(",
                inner.Message
            );

            Assert.Contains("String Foo", inner.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Guid Bar", inner.Message, StringComparison.OrdinalIgnoreCase);
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

            var original = new ValidDecorated();
            ValidDecorated.IsDecoratedCalled = false;

            // Act

            original.SetFoo("Updated");

            // Assert

            Assert.True(ValidDecorated.IsDecoratedCalled);
        }

        public class ValidDecorated : ImmutableBase<ValidDecorated> {

            public static bool IsDecoratedCalled = false;

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

            [ImmutableSetterConstructor]
            public ValidDecorated(Guid bar, Int32 bizz, String foo) {
                this.Bizz = bizz;
                this.Bar = bar;
                this.Foo = foo;

                ValidDecorated.IsDecoratedCalled = true;
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

    }

}
