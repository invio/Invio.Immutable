using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class DefaultPropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Theory]
        [InlineData(nameof(Fake.StringProperty))]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void IsSupported_AllSupported(String propertyName) {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(propertyName);

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.True(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.StringProperty))]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void Create_AllCreated(String propertyName) {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(propertyName);

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<DefaultPropertyHandler>(handler);
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return new DefaultPropertyHandlerProvider();
        }

        private sealed class Fake : ImmutableBase<Fake> {

            public String StringProperty { set { this.ObjectProperty = value; } }
            public Object ObjectProperty { get; set; }
            public Int32 Int32Property { get; } = 32;

        }

    }

}
