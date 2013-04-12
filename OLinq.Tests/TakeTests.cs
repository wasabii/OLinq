using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{
    [TestClass]
    public class TakeTests
    {

        ObservableCollection<string> source;
        ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetupFilters()
        {
            source = new ObservableCollection<string>("0123456789".Select(i => i.ToString()));
            buffer = source.AsObservableQuery().Take(5).AsObservableQuery().ToObservableView().ToBuffer();
        }

        [TestMethod] 
        public void NotYetImplemented_TakeWorks()
        {
            Assert.AreEqual("01234", string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_InsertIntoTaken()
        {
            source.Insert(0, "X");
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
            source.Insert(4, "X");
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));

        }
        [TestMethod]
        public void NotYetImplemented_InsertAfterTaken()
        {
            source.Insert(5, "X");
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
            source.Insert(7, "X");
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_RemoveFromTaken()
        {
            source.RemoveAt(0);
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
            source.RemoveAt(4);
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));

        }

        [TestMethod]
        public void NotYetImplemented_RemovedAfterTaken()
        {
            source.RemoveAt(0); ;
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
            source.RemoveAt(7);
            Assert.AreEqual(string.Join("", source.Take(5)), string.Join("", buffer));
        }

    }

}