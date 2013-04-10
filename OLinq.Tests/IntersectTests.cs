using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class IntersectTests
    {

        ObservableCollection<string> one;
        ObservableCollection<string> two;
        ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetupFilters()
        {
            one = new ObservableCollection<string>() { "a", "b", "c", "d" };
            two = new ObservableCollection<string>() { "b", "c", "d", "e" };

            buffer = one.AsObservableQuery().Intersect(two).AsObservableQuery().ToObservableView().ToBuffer();
        }

        [TestMethod]
        public void IntersectWorks()
        {
            Assert.AreEqual("bcd", string.Join("", buffer));
        }

        [TestMethod]
        public void ItemAddedToSourceWorks()
        {
            one.Add("e");
            Assert.AreEqual("bcde", string.Join("", buffer));
        }

        [TestMethod]
        public void ItemAddedToSource2Works()
        {
            two.Add("a");
            Assert.AreEqual("bcda", string.Join("", buffer)); //order isn't perfect...
        }

        [TestMethod]
        public void ItemRemovedFromSourceWorks()
        {
            one.Remove("b");
            Assert.AreEqual("cd", string.Join("", buffer));
        }
        [TestMethod]
        public void ItemRemovedFromSource2Works()
        {
            two.Remove("b");
            Assert.AreEqual("cd", string.Join("", buffer));
        }

        [TestMethod]
        public void Semantics1()
        {
            var q1 = one.Intersect(two);
            var q2 = one.AsObservableQuery().Intersect(two).AsObservableQuery().ToObservableView();

            Assert.IsTrue(q1.SequenceEqual(q2));
        }

    }

}
