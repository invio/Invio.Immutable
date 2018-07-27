using System;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {
    [UnitTest]
    public class NullablePropertyHandlerTests : PropertyHandlerTestsBase {
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

            var handler = this.CreateHandler(NullableValuePropertyInfo);

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

        protected override PropertyInfo NextValidPropertyInfo() {
            return NullableValuePropertyInfo;
        }

        protected override Object NextParent() {
            return new FakeImmutable();
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new NullablePropertyHandler<Int32>(
                new FakePropertyHandler(),
                property
            );
        }

        private static PropertyInfo NullableValuePropertyInfo { get; } =
            ReflectionHelper<FakeImmutable>.GetProperty(obj => obj.NullableValueProperty);

        private class FakeImmutable : ImmutableBase<FakeImmutable> {
            public Int32? NullableValueProperty { get; }

            public FakeImmutable(Int32? nullableValueProperty = null) {
                this.NullableValueProperty = nullableValueProperty;
            }
        }

        private class FakePropertyHandler : IPropertyHandler {
            public string PropertyName => nameof(FakeImmutable.NullableValueProperty);
            public Type PropertyType { get; } = typeof(Int32);

            public bool ArePropertyValuesEqual(object leftParent, object rightParent) {
                return Math.Abs(((FakeImmutable)leftParent).NullableValueProperty.Value) ==
                    Math.Abs(((FakeImmutable)leftParent).NullableValueProperty.Value);
            }

            public int GetPropertyValueHashCode(object parent) {
                return Math.Abs(((FakeImmutable)parent).NullableValueProperty.Value).GetHashCode();
            }

            public object GetPropertyValue(object parent) {
                return ((FakeImmutable)parent).NullableValueProperty.Value;
            }
        }
    }
}