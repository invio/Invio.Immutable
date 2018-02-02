using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class ListPropertyHandlerTests : EnumerablePropertyHandlerTestsBase {

        private static ISet<PropertyInfo> properties { get; }
        private static Random random { get; }

        static ListPropertyHandlerTests() {
            var parent = typeof(FakeImmutable);

            properties = ImmutableHashSet.Create(
                parent.GetProperty(nameof(FakeImmutable.Array)),
                parent.GetProperty(nameof(FakeImmutable.Enumerable)),
                parent.GetProperty(nameof(FakeImmutable.List)),
                parent.GetProperty(nameof(FakeImmutable.Collection))
            );

            random = new Random();
        }

        public static IEnumerable<object[]> MatchingMemberData {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { "foo", "bar" }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", null },
                    new string[] { "foo", null }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { String.Empty, 45m, DateTime.MinValue }
                };

                yield return new object[] {
                    nameof(FakeImmutable.List),
                    new List<string> { "12345", "Hello, World", "Bananas" },
                    ImmutableList.Create<string>("12345", "Hello, World", "Bananas" )
                };

                var guid = Guid.NewGuid();

                yield return new object[] {
                    nameof(FakeImmutable.Collection),
                    new List<Guid> { Guid.Empty, guid },
                    ImmutableList.Create(Guid.Empty, guid)
                };
            }
        }

        [Theory]
        [MemberData(nameof(MatchingMemberData))]
        public void GetPropertyValueHashCode_Matches(
            String propertyName,
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, leftPropertyValue);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, rightPropertyValue);

            // Act

            var leftHashCode = handler.GetPropertyValueHashCode(leftFake);
            var rightHashCode = handler.GetPropertyValueHashCode(rightFake);

            // Assert

            Assert.Equal(leftHashCode, rightHashCode);
        }

        [Theory]
        [MemberData(nameof(MatchingMemberData))]
        public void ArePropertyValuesEqual_Matches(
            String propertyName,
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, leftPropertyValue);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, rightPropertyValue);

            // Act

            var areEqual = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.True(areEqual);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_MismatchedValues_MemberData {
            get {

                // Case of 'foo' differs

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { "FOO", "bar" }
                };

                // null vs. "foo"

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { null, "bar" },
                    new string[] { "foo", "bar" }
                };

                // DateTime.MinValue vs. null

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { String.Empty, 45m, null }
                };

                // 12345 => 54321

                yield return new object[] {
                    nameof(FakeImmutable.List),
                    new List<string> { "12345", "Hello, World", "Bananas" },
                    ImmutableList.Create<string>("54321", "Hello, World", "Bananas" )
                };

                // Distinct Guids

                yield return new object[] {
                    nameof(FakeImmutable.Collection),
                    new List<Guid> { Guid.Empty, Guid.NewGuid() },
                    ImmutableList.Create(Guid.Empty, Guid.NewGuid())
                };
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_MismatchedValues_MemberData))]
        public void ArePropertyValuesEqual_MismatchedValues(
            String propertyName,
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, leftPropertyValue);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, rightPropertyValue);

            // Act

            var areEqual = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.False(areEqual);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_MismatchedOrder_MemberData {
            get {
                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { "bar", "foo" }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, DateTime.MinValue, 45m),
                    new object[] { String.Empty, 45m, DateTime.MinValue }
                };

                yield return new object[] {
                    nameof(FakeImmutable.List),
                    new List<string> { "12345", "Hello, World", "Bananas" },
                    ImmutableList.Create<string>("Bananas", "12345", "Hello, World")
                };

                var guid = Guid.NewGuid();

                yield return new object[] {
                    nameof(FakeImmutable.Collection),
                    new List<Guid> { Guid.Empty, guid },
                    ImmutableList.Create(guid, Guid.Empty)
                };
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_MismatchedOrder_MemberData))]
        public void ArePropertyValuesEqual_MismatchedOrder(
            String propertyName,
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, leftPropertyValue);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, rightPropertyValue);

            // Act

            var areEqual = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.False(areEqual);
        }

        public static IEnumerable<object[]> ArePropertyValuesEqual_MismatchedSize_MemberData {
            get {

                // Jagged on left

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar", "biz" },
                    new string[] { "foo", "bar" }
                };

                // Jagged on right

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { String.Empty, 45m, DateTime.MinValue, new object() }
                };

                // Trailing duplicate on left

                yield return new object[] {
                    nameof(FakeImmutable.List),
                    new List<string> { "12345", "Hello, World", "Bananas", "Bananas" },
                    ImmutableList.Create<string>("12345", "Hello, World", "Bananas" )
                };

                // Leading duplicate on right

                var guid = Guid.NewGuid();

                yield return new object[] {
                    nameof(FakeImmutable.Collection),
                    new List<Guid> { Guid.Empty, guid },
                    ImmutableList.Create(Guid.Empty, Guid.Empty, guid)
                };
            }
        }

        [Theory]
        [MemberData(nameof(ArePropertyValuesEqual_MismatchedSize_MemberData))]
        public void ArePropertyValuesEqual_MismatchedSize(
            String propertyName,
            IEnumerable leftPropertyValue,
            IEnumerable rightPropertyValue) {

            // Arrange

            var handler = this.CreateHandler(propertyName);
            var leftFake = this.NextFake().SetPropertyValue(propertyName, leftPropertyValue);
            var rightFake = this.NextFake().SetPropertyValue(propertyName, rightPropertyValue);

            // Act

            var areEqual = handler.ArePropertyValuesEqual(leftFake, rightFake);

            // Assert

            Assert.False(areEqual);
        }

        protected override PropertyInfo NextValidPropertyInfo() {
            return
                properties
                    .Skip(random.Next(0, properties.Count))
                    .First();
        }

        protected override object NextParent() {
            return this.NextFake();
        }

        private FakeImmutable NextFake() {
            return new FakeImmutable(
                new object[] { new object() },
                ImmutableHashSet.Create(DateTime.UtcNow),
                ImmutableList.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                new HashSet<Guid> { Guid.NewGuid() }
            );
        }

        private IPropertyHandler CreateHandler(string propertyName) {
            return this.CreateHandler(typeof(FakeImmutable).GetProperty(propertyName));
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new ListPropertyHandler(property);
        }

        private sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public object[] Array { get; }
            public IEnumerable Enumerable { get; }
            public IList<string> List { get; }
            public ICollection<Guid> Collection { get; }

            public FakeImmutable(
                object[] array = default(object[]),
                IEnumerable enumerable = default(IEnumerable),
                IList<string> list = default(IList<string>),
                ICollection<Guid> collection = default(ICollection<Guid>)) {

                this.Array = array;
                this.Enumerable = enumerable;
                this.List = list;
                this.Collection = collection;
            }

            public FakeImmutable SetPropertyValue(String propertyName, object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

    }

}
