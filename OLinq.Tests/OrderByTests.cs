using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class OrderByTests
    {

        TestObservableCollection<string> source;
        ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetupFilters()
        {
            source = new TestObservableCollection<string>("864".Select(i => i.ToString()));
            buffer = source.AsObservableQuery().OrderBy(letter => letter).AsObservableQuery().ToObservableView().ToBuffer();
        }

        [TestMethod]
        public void NotYetImplemented_OrderWorks()
        {
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_EnumeratesSourceOnlyOnce()
        {
            buffer.ToArray();
            buffer.ToArray();
            Assert.AreEqual(1, source.EnumerationCount);
        }

        void DoAssert()
        {
            Assert.AreEqual(string.Join("", source.OrderBy(s => s)), string.Join("", buffer));
        }

        [TestMethod]
        public void NotYetImplemented_InsertAtStartWorks()
        {
            source.Insert(0, "7");
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_InsertInMiddleWorks()
        {
            source.Insert(1, "7");
            DoAssert();
            source.Insert(3, "9");
            DoAssert();

        }

        [TestMethod]
        public void NotYetImplemented_InsertAtEndWorks()
        {
            source.Insert(3, "3");
            DoAssert();
        }
        [TestMethod]
        public void NotYetImplemented_RemoveAtStartWorks()
        {
            source.RemoveAt(0);
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_RemoveInMiddleWorks()
        {
            source.RemoveAt(1);
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_RemoveAtEndWorks()
        {
            source.RemoveAt(2);
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_DoesntRemoveDuplicatesAfterRemove()
        {
            source = new TestObservableCollection<string>("44".Select(i => i.ToString()));
            buffer = source.AsObservableQuery().OrderBy(letter => letter).AsObservableQuery().ToObservableView().ToBuffer();
            source.RemoveAt(0);
            DoAssert();
        }

    }

}