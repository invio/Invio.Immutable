using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class StringPropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Fact]
        public void IsSupported_StringProperty() {

            // Arrange

            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.True(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void IsSupported_NonStringProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.False(isSupported);
        }

        [Fact]
        public void Create_StringProperty() {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(nameof(Fake.StringProperty));

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<StringPropertyHandler>(handler);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void Create_NonStringProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var exception = Record.Exception(
                () => provider.Create(property)
            );

            // Assert

            Assert.IsType<NotSupportedException>(exception);
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return new StringPropertyHandlerProvider();
        }

        private sealed class Fake : ImmutableBase<Fake> {

            public String StringProperty { get; }
            public Object ObjectProperty { get; }
            public Int32 Int32Property { get; }

            public Fake(
                String stringProperty = default(String),
                Object objectProperty = default(Object),
                Int32 int32Property = default(Int32)) {

                this.StringProperty = stringProperty;
                this.ObjectProperty = objectProperty;
                this.Int32Property = int32Property;
            }

        }

    }

}
