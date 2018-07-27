using System;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {
    [UnitTest]
    public class NullablePropertyHandlerProviderTests : PropertyHandlerProviderTestsBase {
        [Fact]
        public void Nullable_Supported() {
            var handler = CreateNullableProvider();

            var result = handler.IsSupported(NullableValuePropertyInfo);

            Assert.True(result);
        }

        [Fact]
        public void NonNullable_NotSupported() {
            var handler = CreateNullableProvider();

            var result = handler.IsSupported(ValuePropertyInfo);

            Assert.False(result);
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, 0, false)]
        [InlineData(0, null, false)]
        [InlineData(null, Int32.MaxValue, false)]
        [InlineData(Int32.MaxValue, null, false)]
        [InlineData(1, -1, true)]
        [InlineData(-1, 1, true)]
        public void TestNullHandling(Int32? value1, Int32? value2, Boolean equal) {
            var left = new FakeImmutable(value1);
            var right = new FakeImmutable(value2);
            var provider = CreateNullableProvider();

            var handler = provider.Create(NullableValuePropertyInfo);

            var areEqual = handler.ArePropertyValuesEqual(left, right);
            var leftHashCode = handler.GetPropertyValueHashCode(left);
            var rightHashCode = handler.GetPropertyValueHashCode(right);

            if (equal) {
                Assert.True(areEqual);
                Assert.Equal(leftHashCode, rightHashCode);
            } else {
                Assert.False(areEqual);
                Assert.NotEqual(leftHashCode, rightHashCode);
            }
        }

        protected override IPropertyHandlerProvider CreateProvider() {
            return CreateNullableProvider();
        }

        private static NullablePropertyHandlerProvider<Int32> CreateNullableProvider() {
            return new NullablePropertyHandlerProvider<Int32>(new FakePropertyHandlerProvider());
        }

        private static PropertyInfo NullableValuePropertyInfo { get; } =
            ReflectionHelper<FakeImmutable>.GetProperty(obj => obj.NullableValueProperty);

        private static PropertyInfo ValuePropertyInfo { get; } =
            ReflectionHelper<FakeImmutable>.GetProperty(obj => obj.ValueProperty);

        private class FakeImmutable : ImmutableBase<FakeImmutable> {
            public Int32? NullableValueProperty { get; }

            public Int32 ValueProperty { get; }

            public FakeImmutable(Int32? nullableValueProperty = null, Int32 valueProperty = 0) {
                this.NullableValueProperty = nullableValueProperty;
                this.ValueProperty = valueProperty;
            }
        }

        private class FakePropertyHandler : IPropertyHandler {
            public string PropertyName => this.property.Name;
            public Type PropertyType => this.property.PropertyType;

            private PropertyInfo property;

            public FakePropertyHandler(PropertyInfo property) {
                if (property.Name != nameof(FakeImmutable.NullableValueProperty) ||
                    property.PropertyType != typeof(Int32)) {

                    throw new NotSupportedException(
                        $"Only supports a non-nullable version of " +
                        $"{nameof(FakeImmutable.NullableValueProperty)}"
                    );
                }

                this.property = property;
            }

            public bool ArePropertyValuesEqual(object leftParent, object rightParent) {
                return Math.Abs((Int32)property.GetValue(leftParent)) ==
                    Math.Abs((Int32)property.GetMethod.Invoke(rightParent, new object[0]));
            }

            public int GetPropertyValueHashCode(object parent) {
                return Math.Abs((Int32)property.GetValue(parent)).GetHashCode();
            }

            public object GetPropertyValue(object parent) {
                return (Int32)property.GetValue(parent);
            }
        }

        private class FakePropertyHandlerProvider : PropertyHandlerProviderBase<Int32> {
            protected override bool IsSupportedImpl(PropertyInfo property) {
                return base.IsSupportedImpl(property) &&
                    property.Name == NullableValuePropertyInfo.Name;
            }

            protected override IPropertyHandler CreateImpl(PropertyInfo property) {
                return new FakePropertyHandler(property);
            }
        }
    }
}