using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class ConcatTests
    {

        [TestMethod]
        public void ConcatTest()
        {

            int changes = 0;

            var c1 = new ObservableCollection<string>()
            {
                "Item_1_1",
                "Item_1_2",
                "Item_1_3",
                "Item_1_4",
                "Item_1_5",
            };

            var c2 = new ObservableCollection<string>()
            {
                "Item_2_1",
                "Item_2_2",
                "Item_2_3",
                "Item_2_4",
                "Item_2_5",
            };

            var q = c1.AsObservableQuery()
                .Concat(c2)
                .AsObservableQuery()
                .ToObservableView();
            q.CollectionChanged += (s, a) => changes++;

            Assert.AreEqual(q.Count(), 10);

            c1.Add("Item_1_6");
            Assert.AreEqual(1, changes);
            Assert.AreEqual(q.Count(), 11);
            c1.Remove("Item_1_6");
            Assert.AreEqual(2, changes);
            Assert.AreEqual(q.Count(), 10);

            c1.Add("Item_2_6");
            Assert.AreEqual(3, changes);
            Assert.AreEqual(q.Count(), 11);
            c1.Remove("Item_2_6");
            Assert.AreEqual(4, changes);
            Assert.AreEqual(q.Count(), 10);
        }

        [TestMethod]
        public void OrderIsPreservedWhenAddingToFirstCollection()
        {
            var c1 = new ObservableCollection<string>() { "b" };
            var c2 = new ObservableCollection<string>() { "c", };
            var combined = c1.AsObservableQuery().Concat(c2).AsObservableQuery().ToObservableView().ToBuffer();
            c1.Insert(0, "a");
            Assert.AreEqual("abc", string.Join("", combined));
        }

        [TestMethod]
        public void OrderIsPreservedWhenAddingToSecondCollection()
        {
            var c1 = new ObservableCollection<string>() { "a", };
            var c2 = new ObservableCollection<string>() { "c", };
            var combined = c1.AsObservableQuery().Concat(c2).AsObservableQuery().ToObservableView().ToBuffer();
            c2.Insert(0, "b");
            Assert.AreEqual("abc", string.Join("", combined));
        }

        [TestMethod]
        public void OrderIsPreservedWhenRemovingFromFirstCollection()
        {
            var c1 = new ObservableCollection<string>() { "a", "b" };
            var c2 = new ObservableCollection<string>() { "a", "b", };
            var combined = c1.AsObservableQuery().Concat(c2).AsObservableQuery().ToObservableView().ToBuffer();
            c1.RemoveAt(0);
            Assert.AreEqual("bab", string.Join("", combined));
        }

        [TestMethod]
        public void OrderIsPreservedWhenRemovingFromSecondCollection()
        {
            var c1 = new ObservableCollection<string>() { "a", "b" };
            var c2 = new ObservableCollection<string>() { "a", "b", };
            var combined = c1.AsObservableQuery().Concat(c2).AsObservableQuery().ToObservableView().ToBuffer();
            c2.Remove("a");
            Assert.AreEqual("abb", string.Join("", combined));
        }

        [TestMethod]
        public void DoesNotEnumeratSourcesMoreThanOnce()
        {
            var c1 = new TestObservableCollection<string>() { "a", "b" };
            var c2 = new TestObservableCollection<string>() { "a", "b", };
            var buffer = c1.AsObservableQuery().Concat(c2).AsObservableQuery().ToObservableView().ToBuffer();
            buffer.ToArray();
            buffer.ToArray();
            Assert.AreEqual(1, c1.EnumerationCount);

            Assert.AreEqual(1, c2.EnumerationCount);
            
        }
    }
}
