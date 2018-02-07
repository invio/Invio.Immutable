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
    public sealed class SetPropertyHandlerTests : EnumerablePropertyHandlerTestsBase {

        private static ISet<PropertyInfo> properties { get; }
        private static Random random { get; }

        static SetPropertyHandlerTests() {
            var parent = typeof(FakeImmutable);

            properties = ImmutableHashSet.Create(
                parent.GetProperty(nameof(FakeImmutable.Array)),
                parent.GetProperty(nameof(FakeImmutable.Enumerable)),
                parent.GetProperty(nameof(FakeImmutable.Set)),
                parent.GetProperty(nameof(FakeImmutable.ImmutableSet))
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
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { 45m, String.Empty, DateTime.MinValue }
                };

                yield return new object[] {
                    nameof(FakeImmutable.Set),
                    new HashSet<string> { "12345", "Hello, World", "Bananas", "Bananas" },
                    ImmutableHashSet.Create("12345", "Hello, World", "Bananas", "12345")
                };

                yield return new object[] {
                    nameof(FakeImmutable.Set),
                    ImmutableHashSet.Create(new string[] { "foo", null }),
                    ImmutableHashSet.Create(new string[] { null, null, "foo" })
                };

                var guid = Guid.NewGuid();

                yield return new object[] {
                    nameof(FakeImmutable.ImmutableSet),
                    ImmutableSortedSet.Create(guid, Guid.Empty),
                    ImmutableSortedSet.Create(Guid.Empty, guid)
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

                // One of the values is null

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { null, "bar" }
                };

                // DateTime.MinValue vs. DateTime.MaxValue

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { String.Empty, 45m, DateTime.MaxValue }
                };

                // 12345 => 54321

                yield return new object[] {
                    nameof(FakeImmutable.Set),
                    new HashSet<string> { "12345", "Hello, World", "Bananas" },
                    ImmutableHashSet.Create("54321", "Hello, World", "Bananas")
                };

                // Distinct Guids

                yield return new object[] {
                    nameof(FakeImmutable.ImmutableSet),
                    ImmutableSortedSet.Create(Guid.NewGuid(), Guid.Empty),
                    ImmutableSortedSet.Create(Guid.Empty, Guid.NewGuid())
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

        public static IEnumerable<object[]> ArePropertyValuesEqual_MismatchedSize_MemberData {
            get {

                // Trailing duplicate on left

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar", "biz" },
                    new string[] { "foo", "bar" }
                };

                // Duplicate of 'foo' on right

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { "foo", "bar", "foo" }
                };

                // Trailing duplicate on right

                yield return new object[] {
                    nameof(FakeImmutable.Array),
                    new object[] { "foo", "bar" },
                    new string[] { "foo", "bar", "biz" }
                };

                // Leading duplicate on right

                yield return new object[] {
                    nameof(FakeImmutable.Enumerable),
                    ImmutableList.Create<object>(String.Empty, 45m, DateTime.MinValue),
                    new object[] { String.Empty, String.Empty, 45m, DateTime.MinValue }
                };

                // Jagged on left

                yield return new object[] {
                    nameof(FakeImmutable.Set),
                    new HashSet<string> { "12345", "Hello, World", "Bananas", "Apples" },
                    ImmutableSortedSet.Create<string>("12345", "Hello, World", "Bananas" )
                };

                // Jagged on right

                var guid = Guid.NewGuid();

                yield return new object[] {
                    nameof(FakeImmutable.ImmutableSet),
                    ImmutableHashSet.Create(Guid.Empty, guid),
                    ImmutableSortedSet.Create(Guid.NewGuid(), Guid.Empty, guid)
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
                ImmutableList.Create(DateTime.UtcNow),
                new HashSet<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                ImmutableHashSet.Create(Guid.NewGuid())
            );
        }

        private IPropertyHandler CreateHandler(string propertyName) {
            return this.CreateHandler(typeof(FakeImmutable).GetProperty(propertyName));
        }

        protected override IPropertyHandler CreateHandler(PropertyInfo property) {
            return new SetPropertyHandler(property);
        }

        private sealed class FakeImmutable : ImmutableBase<FakeImmutable> {

            public object[] Array { get; }
            public IEnumerable Enumerable { get; }
            public ISet<string> Set { get; }
            public IImmutableSet<Guid> ImmutableSet { get; }

            public FakeImmutable(
                object[] array = default(object[]),
                IEnumerable enumerable = default(IEnumerable),
                ISet<string> set = default(ISet<string>),
                IImmutableSet<Guid> immutableSet = default(IImmutableSet<Guid>)) {

                this.Array = array;
                this.Enumerable = enumerable;
                this.Set = set;
                this.ImmutableSet = immutableSet;
            }

            public FakeImmutable SetPropertyValue(String propertyName, object propertyValue) {
                return this.SetPropertyValueImpl(propertyName, propertyValue);
            }

        }

    }

}
