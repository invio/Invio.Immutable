using System;
using Xunit;

namespace Invio.Immutable {

    public abstract class PropertyHandlerFactoryTestsBase {

        [Fact]
        public void Create_Null_PropertyInfo() {

            // Arrange

            var factory = this.CreateFactory();

            // Act

            var exception = Record.Exception(
                () => factory.Create(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        protected abstract IPropertyHandlerFactory CreateFactory();

    }

}
