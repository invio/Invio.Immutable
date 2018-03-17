using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Invio.Xunit;
using Moq;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class EnumerablePropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {

        [Theory]
        [InlineData(nameof(Fake.ImmutableSetProperty))]
        [InlineData(nameof(Fake.SetProperty))]
        [InlineData(nameof(Fake.EnumerableProperty))]
        [InlineData(nameof(Fake.ListProperty))]
        public void IsSupported_EnumerableProperties(String propertyName) {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(propertyName);

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.True(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void IsSupported_NonEnumerableProperties(String propertyName) {

            // Arrange

            var provider = this.CreateProvider();
            var property = typeof(Fake).GetProperty(propertyName);

            // Act

            var isSupported = provider.IsSupported(property);

            // Assert

            Assert.False(isSupported);
        }

        [Theory]
        [InlineData(nameof(Fake.ImmutableSetProperty))]
        [InlineData(nameof(Fake.SetProperty))]
        public void Create_SetProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<SetPropertyHandler>(handler);
        }

        [Theory]
        [InlineData(nameof(Fake.EnumerableProperty))]
        [InlineData(nameof(Fake.ListProperty))]
        public void Create_ListProperties(String propertyName) {

            // Arrange

            var property = typeof(Fake).GetProperty(propertyName);
            var provider = this.CreateProvider();

            // Act

            var handler = provider.Create(property);

            // Assert

            Assert.IsType<ListPropertyHandler>(handler);
        }

        [Theory]
        [InlineData(nameof(Fake.ObjectProperty))]
        [InlineData(nameof(Fake.Int32Property))]
        public void Create_NonEnumerableProperties(String propertyName) {

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
            return new EnumerablePropertyHandlerProvider();
        }

        private sealed class Fake : ImmutableBase<Fake> {

            public IEnumerable EnumerableProperty { get; }
            public IList<string> ListProperty { get; }
            public IImmutableSet<Guid> ImmutableSetProperty { get; }
            public ISet<object> SetProperty { get; }

            public Object ObjectProperty { get; }
            public Int32 Int32Property { get; }

            public Fake(
                IEnumerable enumerableProperty = default(IEnumerable),
                IList<string> listProperty = default(IList<string>),
                IImmutableSet<Guid> immutableSetProperty = default(IImmutableSet<Guid>),
                ISet<object> setProperty = default(ISet<object>),
                Object objectProperty = default(Object),
                Int32 int32Property = default(Int32)) {

                this.EnumerableProperty = enumerableProperty;
                this.ListProperty = listProperty;
                this.ImmutableSetProperty = immutableSetProperty;
                this.SetProperty = setProperty;
                this.ObjectProperty = objectProperty;
                this.Int32Property = int32Property;
            }

        }

    }

}
