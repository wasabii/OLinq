using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class OrderByDescendingTests
    {
        private ObservableCollection<string> source;
        private ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetupFilters()
        {
            source = new ObservableCollection<string>("864".Select(i => i.ToString()));
            buffer =
                source.AsObservableQuery()
                      .OrderByDescending(letter => letter)
                      .AsObservableQuery()
                      .ToObservableView()
                      .ToBuffer();
        }

        [TestMethod]
        public void NotYetImplemented_OrderWorks()
        {
            Assert.AreEqual("864", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_InsertAtStartWorks()
        {
            source.Insert(0, "7");
            Assert.AreEqual("8764", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_InsertInMiddleWorks()
        {
            source.Insert(1, "7");
            Assert.AreEqual("8764", string.Join("", buffer));
            source.Insert(3, "9");
            Assert.AreEqual("98764", string.Join("", buffer));

        }

        [TestMethod]
        public void NotYetImplemented_InsertAtEndWorks()
        {
            source.Insert(3, "3");
            Assert.AreEqual("8643", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_RemoveAtStartWorks()
        {
            source.RemoveAt(0);
            Assert.AreEqual("64", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_RemoveInMiddleWorks()
        {
            source.RemoveAt(1);
            Assert.AreEqual("84", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_RemoveAtEndWorks()
        {
            source.RemoveAt(2);
            Assert.AreEqual("86", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_DoesntRemoveDuplicates()
        {
            source = new ObservableCollection<string>("44".Select(i => i.ToString()));
            buffer =
                source.AsObservableQuery()
                      .OrderByDescending(letter => letter)
                      .AsObservableQuery()
                      .ToObservableView()
                      .ToBuffer();
            Assert.AreEqual("44", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_DoesntRemoveDuplicatesAfterRemove()
        {
            source = new ObservableCollection<string>("44".Select(i => i.ToString()));
            buffer =
                source.AsObservableQuery()
                      .OrderByDescending(letter => letter)
                      .AsObservableQuery()
                      .ToObservableView()
                      .ToBuffer();
            source.RemoveAt(0);
            Assert.AreEqual("4", string.Join("", buffer));
        }

    }

}