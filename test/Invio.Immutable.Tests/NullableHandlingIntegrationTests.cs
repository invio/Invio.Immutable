using System;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {
    [IntegrationTest]
    public class NullableHandlingIntegrationTests {

        [Theory]
        [InlineData(123000f, 123001f)]
        [InlineData(1.23f, 1.23001f)]
        [InlineData(1.23e-10f, 1.23001e-10f)]
        public void TestFuzzyNullableSingle_IsFuzzy(Single? value1, Single? value2) {

            var obj1 = new NullableHandlingTest(fuzzyNullableSingle: value1);
            var obj2 = new NullableHandlingTest(fuzzyNullableSingle: value2);

            Assert.Equal(obj1, obj2);
            Assert.Equal(obj1.GetHashCode(), obj2.GetHashCode());
        }

        [Theory]
        [InlineData(0.0f, null, false)]
        [InlineData(Single.MaxValue, null, false)]
        [InlineData(Single.PositiveInfinity, null, false)]
        [InlineData(Single.NegativeInfinity, null, false)]
        [InlineData(Single.NaN, null, false)]
        [InlineData(Single.Epsilon, null, false)]
        [InlineData(null, null, true)]
        public void TestFuzzyNullableSingle_NullHandling(
            Single? value1,
            Single? value2,
            Boolean areEqual) {

            var obj1 = new NullableHandlingTest(fuzzyNullableSingle: value1);
            var obj2 = new NullableHandlingTest(fuzzyNullableSingle: value2);

            if (areEqual) {
                Assert.Equal(obj1, obj2);
                Assert.Equal(obj1.GetHashCode(), obj2.GetHashCode());
            } else {
                Assert.NotEqual(obj1, obj2);
                Assert.NotEqual(obj1.GetHashCode(), obj2.GetHashCode());
            }
        }

        [Theory]
        [InlineData(0.0f, null, false)]
        [InlineData(Single.MaxValue, null, false)]
        [InlineData(Single.PositiveInfinity, null, false)]
        [InlineData(Single.NegativeInfinity, null, false)]
        [InlineData(Single.NaN, null, false)]
        [InlineData(Single.Epsilon, null, false)]
        [InlineData(null, null, true)]
        public void TestDefaultNullableSingle_NullHandling(
            Single? value1,
            Single? value2,
            Boolean areEqual) {

            var obj1 = new NullableHandlingTest(defaultNullableSingle: value1);
            var obj2 = new NullableHandlingTest(defaultNullableSingle: value2);

            if (areEqual) {
                Assert.Equal(obj1, obj2);
                Assert.Equal(obj1.GetHashCode(), obj2.GetHashCode());
            } else {
                Assert.NotEqual(obj1, obj2);
                Assert.NotEqual(obj1.GetHashCode(), obj2.GetHashCode());
            }
        }

        private class NullableHandlingTest : ImmutableBase<NullableHandlingTest> {
            [SingleComparison(3, PrecisionStyle.SignificantFigures)]
            public Single? FuzzyNullableSingle { get; }

            public Single? DefaultNullableSingle { get; }

            public NullableHandlingTest(
                Single? fuzzyNullableSingle = null,
                Single? defaultNullableSingle = null) {

                this.FuzzyNullableSingle = fuzzyNullableSingle;
                this.DefaultNullableSingle = defaultNullableSingle;
            }
        }
    }
}