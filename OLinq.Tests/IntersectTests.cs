using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq.Tests
{

    [TestClass]
    public class IntersectTests
    {
        private ObservableCollection<string> one;
        private ObservableCollection<string> two;
        private ObservableBuffer<string> buffer;

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
 
    }

}
