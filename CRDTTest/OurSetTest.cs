using System;
using System.Linq;
using CRDT;
using NUnit.Framework;

namespace CRDTTest
{
    [TestFixture]
    public class OurSetTest
    {
        private class Helper
        {
            public static ElementState<int, string> MakeEditedElement(ElementState<int, string> elementState) =>
                MakeElement(elementState, false);

            public static ElementState<int, string> MakeRemovedElement(ElementState<int, string> elementState) =>
                MakeElement(elementState, true);


            private static ElementState<int, string> MakeElement(ElementState<int, string> elementState, bool removed)
            {
                return new ElementState<int, string>(
                    elementState.Element,
                    elementState.Id,
                    elementState.Timestamp + 1,
                    removed);
            }
        }

        private ElementState<int, string> _cat;
        private ElementState<int, string> _dog;
        private ElementState<int, string> _duck;
        private ElementState<int, string> _fish;
        private ElementState<int, string> _removeCat;

        private OurSet<int, string> _primaryOurSet;
        private OurSet<int, string> _secondOurSet;

        [SetUp]
        public void SetUp()
        {
            _cat = new ElementState<int, string>("cat", 1, DateTime.Now.Ticks);
            _dog = new ElementState<int, string>("dog", 2, DateTime.Now.Ticks);
            _duck = new ElementState<int, string>("duck", 3, DateTime.Now.Ticks);
            _fish = new ElementState<int, string>("fish", 4, DateTime.Now.Ticks);


            _removeCat = Helper.MakeRemovedElement(_cat);

            _primaryOurSet = new OurSet<int, string>();
            _secondOurSet = new OurSet<int, string>();

            _primaryOurSet.AddRange(_cat, _dog, _duck);
            _secondOurSet.AddRange(_fish, _removeCat);
        }


        [Test]
        public void AddTest()
        {
            _primaryOurSet.Add(_fish);

            var elements = _primaryOurSet.GetElements().ToArray();

            Assert.AreEqual(elements.Count(), 4);

            Assert.Contains(_cat, elements);
            Assert.Contains(_dog, elements);
            Assert.Contains(_duck, elements);
            Assert.Contains(_fish, elements);
        }

        [Test]
        public void LookUpTest()
        {
            var removedDog = Helper.MakeRemovedElement(_dog);
            var editedCat = Helper.MakeEditedElement(_cat);

            _primaryOurSet.Remove(removedDog);
            _primaryOurSet.Add(editedCat);

            var lookupResult = _primaryOurSet.LookUp().ToArray();

            Assert.AreEqual(lookupResult.Length, 2);

            Assert.Contains(_cat.Element, lookupResult);
            Assert.Contains(_duck.Element, lookupResult);
        }

        [Test]
        public void MergeTest()
        {
            var mergeResultElements = _primaryOurSet.Merge(_secondOurSet).GetElements().ToArray();

            Assert.AreEqual(mergeResultElements.Length, 4);

            Assert.Contains(_duck, mergeResultElements);
            Assert.Contains(_dog, mergeResultElements);
            Assert.Contains(_removeCat, mergeResultElements);
            Assert.Contains(_fish, mergeResultElements);

            var reverseMergeResultElements = _secondOurSet.Merge(_primaryOurSet).GetElements().ToArray();

            Assert.AreEqual(mergeResultElements, reverseMergeResultElements);
        }


        [Test]
        public void DiffTest()
        {
            var diffResult = _primaryOurSet.Diff(_secondOurSet).GetElements().ToArray();

            Assert.AreEqual(diffResult.Length, 2);
            Assert.Contains(_duck, diffResult);
            Assert.Contains(_dog, diffResult);

            var reverseDiffResult = _secondOurSet.Diff(_primaryOurSet).GetElements().ToArray();

            Assert.AreEqual(reverseDiffResult.Length, 2);
            Assert.Contains(_fish, reverseDiffResult);
            Assert.Contains(_removeCat, reverseDiffResult);
        }
    }
}