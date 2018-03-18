using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class AggregatePropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Fact]
        public void Constructor_NullProviders() {

            // Act

            var exception = Record.Exception(
                () => new AggregatePropertyHandlerProvider(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_NullInProviders() {

            // Arrange

            var providers = ImmutableList.Create<IPropertyHandlerProvider>(
                new DefaultPropertyHandlerProvider(),
                null,
                new DefaultPropertyHandlerProvider()
            );

            // Act

            var exception = Record.Exception(
                () => new AggregatePropertyHandlerProvider(providers)
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "One or more of the providers was null." +
                Environment.NewLine + "Parameter name: providers",
                exception.Message
            );
        }

        [Fact]
        public void Create_NullInProviders() {

            // Act

            var exception = Record.Exception(
                () => AggregatePropertyHandlerProvider.Create(
                    new DefaultPropertyHandlerProvider(),
                    null,
                    new DefaultPropertyHandlerProvider()
                )
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "One or more of the providers was null." +
                Environment.NewLine + "Parameter name: providers",
                exception.Message
            );
        }

        public static IEnumerable<object[]> IsSupported_UsesInnerProviders_MemberData {
            get {
                yield return new object[] {
                    nameof(Fake.ObjectProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new EnumerablePropertyHandlerProvider(),
                        new StringPropertyHandlerProvider()
                    ),
                    false
                };

                yield return new object[] {
                    nameof(Fake.EnumerableProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new StringPropertyHandlerProvider(),
                        new EnumerablePropertyHandlerProvider()
                    ),
                    true
                };

                yield return new object[] {
                    nameof(Fake.ObjectProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new StringPropertyHandlerProvider(),
                        new EnumerablePropertyHandlerProvider(),
                        new DefaultPropertyHandlerProvider()
                    ),
                    true
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsSupported_UsesInnerProviders_MemberData))]
        public void IsSupported_UsesInnerProviders(
            String propertyName,
            IList<IPropertyHandlerProvider> providers,
            bool expected) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider(providers);

            // Act

            var actual = provider.IsSupported(property);

            // Assert

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> Create_UsesInnerProviders_MemberData {
            get {
                yield return new object[] {
                    nameof(Fake.StringProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new StringPropertyHandlerProvider(),
                        new EnumerablePropertyHandlerProvider(),
                        new DefaultPropertyHandlerProvider()
                    ),
                    typeof(StringPropertyHandler)
                };

                yield return new object[] {
                    nameof(Fake.EnumerableProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new StringPropertyHandlerProvider(),
                        new EnumerablePropertyHandlerProvider()
                    ),
                    typeof(ListPropertyHandler)
                };

                yield return new object[] {
                    nameof(Fake.ObjectProperty),
                    ImmutableList.Create<IPropertyHandlerProvider>(
                        new StringPropertyHandlerProvider(),
                        new EnumerablePropertyHandlerProvider(),
                        new DefaultPropertyHandlerProvider()
                    ),
                    typeof(DefaultPropertyHandler)
                };
            }
        }

        [Theory]
        [MemberData(nameof(Create_UsesInnerProviders_MemberData))]
        public void Create_UsesInnerProviders(
            String propertyName,
            IList<IPropertyHandlerProvider> providers,
            Type expectedType) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider(providers);

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType(expectedType, handler);
        }

        [Fact]
        public void Create_ThrowsIfUnsupported() {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));

            // Act

            var exception = Record.Exception(
                () => provider.Create(property)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return this.CreateProvider(ImmutableList<IPropertyHandlerProvider>.Empty);
        }

        private IPropertyHandlerProvider CreateProvider(
            IEnumerable<IPropertyHandlerProvider> providers) {

            return new AggregatePropertyHandlerProvider(providers?.ToImmutableList());
        }

        private sealed class Fake : ImmutableBase<Fake> {

            public IEnumerable EnumerableProperty { get; }
            public String StringProperty { get; }
            public Object ObjectProperty { get; }

            public Fake(
                IEnumerable enumerableProperty,
                String stringProperty,
                Object objectProperty) {

                this.EnumerableProperty = enumerableProperty;
                this.StringProperty = stringProperty;
                this.ObjectProperty = objectProperty;
            }

        }

    }

}
