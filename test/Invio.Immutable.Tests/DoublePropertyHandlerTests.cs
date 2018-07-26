using System;
using System.Reflection;
using Invio.Extensions.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {
    [UnitTest]
    public sealed class DoublePropertyHandlerTests : PropertyHandlerTestsBase {
        private Random rand { get; } = new Random();

        private static PropertyInfo singleProperty { get; } =
            ReflectionHelper.GetProperty<FakeImmutable, Double>(fake => fake.DoubleProperty);


        [Fact]
        public void Constructor_NullProperty() {
            var exception = Record.Exception(
                () => new DoublePropertyHandler(null, 0)
            );

            var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("property", argumentNullException.ParamName);
        }

        [Fact]
        public void Constructor_NonDoubleProperty() {
            var property =
                typeof(FakeImmutable)
                    .GetProperty(nameof(FakeImmutable.NullableDateTimeProperty));

            var exception = Record.Exception(
                () => new DoublePropertyHandler(property, 1)
            );

            var argumentException = Assert.IsType<ArgumentException>(exception);
            Assert.Equal(
                $"The '{property.Name}' property is not of type 'Double'." +
                Environment.NewLine + "Parameter name: property",
                exception.Message
            );
            Assert.Equal("property", argumentException.ParamName);
        }

        [Theory]
        [InlineData(Int32.MinValue, PrecisionStyle.SignificantFigures)]
        [InlineData(Int32.MinValue, PrecisionStyle.DecimalPlaces)]
        [InlineData(-1, PrecisionStyle.SignificantFigures)]
        [InlineData(-1, PrecisionStyle.DecimalPlaces)]
        [InlineData(0, PrecisionStyle.SignificantFigures)]
        [InlineData(16, PrecisionStyle.DecimalPlaces)]
        [InlineData(16, PrecisionStyle.SignificantFigures)]
        public void Constructor_PrecisionOutOfRange(Int32 precision, PrecisionStyle precisionStyle) {
            var exception = Record.Exception(
                () => new DoublePropertyHandler(singleProperty, precision, precisionStyle)
            );

            var outOfRangeException = Assert.IsType<ArgumentOutOfRangeException>(exception);
            Assert.Equal("precision", outOfRangeException.ParamName);
            Assert.Equal(precision, outOfRangeException.ActualValue);
        }

        [Theory]
        [InlineData(1.0d, 1.0d, 4)]
        [InlineData(12345.67, 12345.68, 0)]
        [InlineData(1.00001d, 1.00002d, 4)]
        [InlineData(1.234e-10d, 4.321e-10d, 4)] // These both round to zero
        public void Test_AbsolutePrecision_Equal(Double value1, Double value2, Int32 precision) {
            var handler = CreateHandler(singleProperty, precision, PrecisionStyle.DecimalPlaces);

            var instance1 = this.NextFake().SetDoubleProperty(value1);
            var instance2 = this.NextFake().SetDoubleProperty(value2);

            Assert.True(handler.ArePropertyValuesEqual(instance1, instance2));
            Assert.Equal(
                handler.GetPropertyValueHashCode(instance1),
                handler.GetPropertyValueHashCode(instance2)
            );
        }

        [Theory]
        [InlineData(12345.12, 12345.67, 0)]
        // No rounding happens because all digits are to the left of the decimal place
        [InlineData(1.222211e10d, 1.222244e10d, 4)]
        public void Test_AbsolutePrecision_NotEqual(Double value1, Double value2, Int32 precision) {
            var handler = CreateHandler(singleProperty, precision, PrecisionStyle.DecimalPlaces);

            var instance1 = this.NextFake().SetDoubleProperty(value1);
            var instance2 = this.NextFake().SetDoubleProperty(value2);

            Assert.False(handler.ArePropertyValuesEqual(instance1, instance2));
            Assert.NotEqual(
                handler.GetPropertyValueHashCode(instance1),
                handler.GetPropertyValueHashCode(instance2)
            );
        }

        [Theory]
        [InlineData(1.0d, 1.0d, 4)]
        [InlineData(1.00001d, 1.00002d, 4)]
        [InlineData(1.22211e-10d, 1.22244e-10d, 4)]
        [InlineData(1.222211e-10d, 1.222266e-10d, 4)]
        // Unlike the normal precision test above where no round occurs, here the values will be
        // rounded to 4 sig figs, so both become 1.2222e10
        [InlineData(1.222211e10d, 1.222244e10d, 4)]
        public void Test_SigFigPrecision_Equal(Double value1, Double value2, Int32 precision) {
            var handler = CreateHandler(singleProperty, precision, PrecisionStyle.SignificantFigures);

            var instance1 = this.NextFake().SetDoubleProperty(value1);
            var instance2 = this.NextFake().SetDoubleProperty(value2);

            Assert.True(handler.ArePropertyValuesEqual(instance1, instance2));
            Assert.Equal(
                handler.GetPropertyValueHashCode(instance1),
                handler.GetPropertyValueHashCode(instance2)
            );
        }

        [Theory]
        [InlineData(1.22211e-10d, 1.22266e-10d, 4)]
        [InlineData(1.22211e10d, 1.22266e10d, 4)]
        // Unlike the normal precision test above where these both round to zero, when using
        // significant figures the four digits here are preserved
        [InlineData(1.234e-10d, 4.321e-10d, 4)]
        public void Test_SigFigPrecision_NotEqual(Double value1, Double value2, Int32 precision) {
            var handler = CreateHandler(singleProperty, precision, PrecisionStyle.SignificantFigures);

            var instance1 = this.NextFake().SetDoubleProperty(value1);
            var instance2 = this.NextFake().SetDoubleProperty(value2);

            Assert.False(handler.ArePropertyValuesEqual(instance1, instance2));
            Assert.NotEqual(
                handler.GetPropertyValueHashCode(instance1),
                handler.GetPropertyValueHashCode(instance2)
            );
        }

        protected override PropertyInfo NextValidPropertyInfo() {
            return singleProperty;
        }

        protected override Object NextParent() {
            return this.NextFake();
        }

        private FakeImmutable NextFake() {
            return new FakeImmutable(rand.NextDouble(), Guid.NewGuid().ToString(), DateTime.UtcNow);
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new DoublePropertyHandler(property, 10);
        }

        private IPropertyHandler CreateHandler(
            PropertyInfo property,
            Int32 precision,
            PrecisionStyle precisionStyle) {

            return new DoublePropertyHandler(property, precision, precisionStyle);
        }

        public sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public Double DoubleProperty { get; }
            public String StringProperty { get; }
            public DateTime? NullableDateTimeProperty { get; }
            public Guid UnrelatedGuid { get; }

            public FakeImmutable(
                Double doubleProperty = default(Double),
                String stringProperty = default(String),
                DateTime? nullableDateTimeProperty = default(DateTime?),
                Guid unrelatedGuid = default(Guid)) {

                this.DoubleProperty = doubleProperty;
                this.StringProperty = stringProperty;
                this.NullableDateTimeProperty = nullableDateTimeProperty;
                this.UnrelatedGuid = unrelatedGuid;
            }

            public FakeImmutable SetDoubleProperty(Double singleProperty) {
                return this.SetPropertyValueImpl(nameof(DoubleProperty), singleProperty);
            }

        }
    }
}