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
    public class SkipTests
    {
        private ObservableCollection<string> one;
        private ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetupFilters()
        {
            one = new ObservableCollection<string>("0123456789".Select(i => i.ToString()));
            buffer = one.AsObservableQuery().Skip(5).AsObservableQuery().ToObservableView().ToBuffer();
        }

        [TestMethod]
        public void NotYetImplemented_SkipWorks()
        {
            Assert.AreEqual("56789", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_InsertIntoSkipped()
        {
            one.Insert(0, "X");
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
            one.Insert(4, "X");
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));

        }
        [TestMethod]
        public void NotYetImplemented_InsertAfterSkipped()
        {
            one.Insert(5, "X");
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
            one.Insert(7, "X");
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_RemoveFromSkipped()
        {
            one.RemoveAt(0);
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
            one.RemoveAt(4);
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));

        }
        [TestMethod]
        public void NotYetImplemented_RemovedAfterSkipped()
        {
            one.RemoveAt(0); ;
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
            one.RemoveAt(7);
            Assert.AreEqual(string.Join("", one), string.Join("", buffer));
        }
    }
}
