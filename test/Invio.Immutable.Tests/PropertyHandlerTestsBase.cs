using System;
using System.Reflection;
using Xunit;

namespace Invio.Immutable {

    public abstract class PropertyHandlerTestsBase {

        [Fact]
        public void PropertyName() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);

            // Act

            var handlerPropertyName = handler.PropertyName;

            // Assert

            Assert.Equal(property.Name, handlerPropertyName);
        }

        [Fact]
        public void PropertyType() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);

            // Act

            var handlerPropertyType = handler.PropertyType;

            // Assert

            Assert.Equal(property.PropertyType, handlerPropertyType);
        }

        [Fact]
        public void GetPropertyValueHashCode_NullParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueHashCode(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValueHashCode_InvalidParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);
            var invalidParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValueHashCode(invalidParent)
            );

            // Assert

            Assert.Equal(
                $"The value object provided is of type {nameof(Object)}, " +
                $"which does not contains the {property.Name} property." +
                Environment.NewLine + "Parameter name: parent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void ArePropertyValuesEqual_NullLeftParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);
            var leftParent = this.NextParent();

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(leftParent, null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ArePropertyValuesEqual_NullRightParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);
            var rightParent = this.NextParent();

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(null, rightParent)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ArePropertyValuesEqual_InvalidLeftParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = CreateHandler(property);

            var leftParent = new object();
            var rightParent = this.NextParent();

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(leftParent, rightParent)
            );

            // Assert

            Assert.Equal(
                $"The value object provided is of type {nameof(Object)}, " +
                $"which does not contains the {property.Name} property." +
                Environment.NewLine + "Parameter name: leftParent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void ArePropertyValuesEqual_InvalidRightParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);

            var leftParent = this.NextParent();
            var rightParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.ArePropertyValuesEqual(leftParent, rightParent)
            );

            // Assert

            Assert.Equal(
                $"The value object provided is of type {nameof(Object)}, " +
                $"which does not contains the {property.Name} property." +
                Environment.NewLine + "Parameter name: rightParent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void GetPropertyValue_NullParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValue(null)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetPropertyValue_InvalidParent() {

            // Arrange

            var property = this.NextValidPropertyInfo();
            var handler = this.CreateHandler(property);
            var invalidParent = new object();

            // Act

            var exception = Record.Exception(
                () => handler.GetPropertyValue(invalidParent)
            );

            // Assert

            Assert.Equal(
                $"The value object provided is of type {nameof(Object)}, " +
                $"which does not contains the {property.Name} property." +
                Environment.NewLine + "Parameter name: parent",
                exception.Message
            );

            Assert.IsType<ArgumentException>(exception);

            Assert.NotNull(exception.InnerException);
        }

        protected abstract PropertyInfo NextValidPropertyInfo();
        protected abstract object NextParent();
        protected abstract IPropertyHandler CreateHandler(PropertyInfo property);

    }

}
