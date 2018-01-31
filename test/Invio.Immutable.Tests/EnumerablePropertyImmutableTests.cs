using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Invio.Xunit;
using Xunit;

namespace Invio.Immutable {

    [UnitTest]
    public sealed class EnumerablePropertyTests {

        private static Random random { get; }

        static EnumerablePropertyTests() {
            random = new Random();
        }

        [Fact]
        public void Equality_UntypedEnumerable_NonNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var leftUntyped = new [] { new Object() };
            var rightUntyped = new [] { new Object() };

            // Act

            var left = fake.SetUntyped(leftUntyped);
            var right = fake.SetUntyped(rightUntyped);

            // Assert

            AssertImmutablesNotEqual(left, right);

            AssertImmutablesEqual(left, right.SetUntyped(leftUntyped));
            AssertImmutablesEqual(left.SetUntyped(rightUntyped), right);
        }

        [Fact]
        public void Equality_UntypedEnumerable_WithNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var untyped = new [] { new Object() };

            // Act

            var nonNulled = fake.SetUntyped(untyped);
            var nulled = fake.SetUntyped(null);

            // Assert

            AssertImmutablesNotEqual(nonNulled, nulled);
            AssertImmutablesEqual(nulled, nonNulled.SetUntyped(null));
        }

        [Fact]
        public void Equality_TypedEnumerable_NonNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var leftTyped = new [] { "Foo" };
            var rightTyped = new [] { "Foo", "Foo" };

            // Act

            var left = fake.SetTyped(leftTyped);
            var right = fake.SetTyped(rightTyped);

            // Assert

            AssertImmutablesNotEqual(left, right);
            AssertImmutablesEqual(left, right.SetTyped(leftTyped));
            AssertImmutablesEqual(left.SetTyped(rightTyped), right);
        }

        [Fact]
        public void Equality_TypedEnumerable_WithNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var typed = new [] { "Foo" };

            // Act

            var nonNulled = fake.SetTyped(typed);
            var nulled = fake.SetTyped(null);

            // Assert

            AssertImmutablesNotEqual(nonNulled, nulled);
            AssertImmutablesEqual(nulled, nonNulled.SetTyped(null));
        }

        [Fact]
        public void Equality_List_NonNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var leftList = new [] { "Foo" };
            var rightList = new List<String> { "Bar" };

            // Act

            var left = fake.SetList(leftList);
            var right = fake.SetList(rightList);

            // Assert

            AssertImmutablesNotEqual(left, right);
            AssertImmutablesEqual(left, right.SetList(leftList));
            AssertImmutablesEqual(left.SetList(rightList), right);
        }

        [Fact]
        public void Equality_List_WithNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var list = new [] { "Foo" };

            // Act

            var nonNulled = fake.SetList(list);
            var nulled = fake.SetList(null);

            // Assert

            AssertImmutablesNotEqual(nonNulled, nulled);
            AssertImmutablesEqual(nulled, nonNulled.SetList(null));
        }

        [Fact]
        public void Equality_Set_NonNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();

            var originalLeftSet = new HashSet<String> { "Foo", "Bar", "Foo" };
            var matchingLeftSet = new HashSet<String> { "Bar", "Foo", "Bar" };

            var originalRightSet = new HashSet<String> { "Foo" };
            var matchingRightSet = new HashSet<String> { "Foo", "Foo", "Foo" };

            // Act

            var left = fake.SetSet(originalLeftSet);
            var right = fake.SetSet(originalRightSet);

            // Assert

            AssertImmutablesNotEqual(left, right);
            AssertImmutablesEqual(left, right.SetSet(matchingLeftSet));
            AssertImmutablesEqual(left.SetSet(matchingRightSet), right);
        }

        [Fact]
        public void Equality_Set_WithNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var @set = new HashSet<String> { "Foo", "Bar", "Foo" };

            // Act

            var nonNulled = fake.SetSet(@set);
            var nulled = fake.SetSet(null);

            // Assert

            AssertImmutablesNotEqual(nonNulled, nulled);
            AssertImmutablesEqual(nulled, nonNulled.SetSet(null));
        }

        [Fact]
        public void Equality_ImmutableSet_NonNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();

            var originalLeftSet = ImmutableHashSet.Create(-1, 2, -1);
            var matchingLeftSet = ImmutableHashSet.Create(2, -1, 2);;

            var originalRightSet = ImmutableHashSet.Create(-1);
            var matchingRightSet = ImmutableHashSet.Create(-1, -1, -1);

            // Act

            var left = fake.SetImmutableSet(originalLeftSet);
            var right = fake.SetImmutableSet(originalRightSet);

            // Assert

            AssertImmutablesNotEqual(left, right);
            AssertImmutablesEqual(left, right.SetImmutableSet(matchingLeftSet));
            AssertImmutablesEqual(left.SetImmutableSet(matchingRightSet), right);
        }

        [Fact]
        public void Equality_ImmutableSet_WithNull() {

            // Arrange

            var fake = this.NextFakeWithEnumerables();
            var immutableSet = ImmutableHashSet.Create(-1, 2, -1);

            // Act

            var nonNulled = fake.SetImmutableSet(immutableSet);
            var nulled = fake.SetImmutableSet(null);

            // Assert

            AssertImmutablesNotEqual(nonNulled, nulled);
            AssertImmutablesEqual(nulled, nonNulled.SetImmutableSet(null));
        }

        private FakeWithEnumerables NextFakeWithEnumerables() {
            return new FakeWithEnumerables(
                NextList(),
                NextList(),
                NextList(),
                NextList().ToImmutableHashSet(),
                NextList().Select(str => str.Length).ToImmutableHashSet()
            );
        }

        private static IList<String> NextList() {
            return
                Enumerable
                    .Range(0, random.Next(10))
                    .Select(_ => Guid.NewGuid().ToString())
                    .ToImmutableList();
        }

        private class FakeWithEnumerables : ImmutableBase<FakeWithEnumerables> {

            public IEnumerable Untyped { get; }
            public IEnumerable<String> Typed { get; }
            public IList<String> List { get; }
            public ISet<String> Set { get; }
            public IImmutableSet<int> ImmutableSet { get; }

            public FakeWithEnumerables(
                IEnumerable untyped,
                IEnumerable<String> typed,
                IList<String> list,
                ISet<String> @set,
                IImmutableSet<int> immutableSet) {

                this.Untyped = untyped;
                this.Typed = typed;
                this.List = list;
                this.Set = @set;
                this.ImmutableSet = immutableSet;
            }

            public FakeWithEnumerables SetUntyped(IEnumerable untyped) {
                return this.SetPropertyValueImpl(nameof(Untyped), untyped);
            }

            public FakeWithEnumerables SetTyped(IEnumerable<String> typed) {
                return this.SetPropertyValueImpl(nameof(Typed), typed);
            }

            public FakeWithEnumerables SetList(IList<String> list) {
                return this.SetPropertyValueImpl(nameof(List), list);
            }

            public FakeWithEnumerables SetSet(ISet<String> @set) {
                return this.SetPropertyValueImpl(nameof(Set), @set);
            }

            public FakeWithEnumerables SetImmutableSet(IImmutableSet<int> immutableSet) {
                return this.SetPropertyValueImpl(nameof(ImmutableSet), immutableSet);
            }

        }

        private static void AssertImmutablesEqual(
            FakeWithEnumerables left,
            FakeWithEnumerables right) {

            Assert.Equal(left, right);
            Assert.Equal((Object)left, (Object)right);

            if (left != null && right != null) {
                Assert.Equal(left.GetHashCode(), right.GetHashCode());
            }
        }

        private static void AssertImmutablesNotEqual(
            FakeWithEnumerables left,
            FakeWithEnumerables right) {

            Assert.NotEqual(left, right);
            Assert.NotEqual((Object)left, (Object)right);
        }

    }

}
