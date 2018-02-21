using System;
using Xunit;

namespace Invio.Immutable {

    public abstract class PropertyHandlerProviderTestsBase : PropertyHandlerFactoryTestsBase {

        [Fact]
        public void IsSupported_Null_PropertyInfo() {

            // Arrange

            var provider = this.CreateProvider();

            // Act

            var exception = Record.Exception(
                () => provider.Create(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        protected override IPropertyHandlerFactory CreateFactory() {
            return this.CreateProvider();
        }

        protected abstract IPropertyHandlerProvider CreateProvider();

    }

}
