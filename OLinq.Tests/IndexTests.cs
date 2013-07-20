using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class IndexTests
    {

        static readonly char[] array = new[]
        {
            'a', 'b', 'c',
        };

        static readonly char[,] mdArray = new[,]
        {
            { 'a', 'b', 'c' },
            { 'd', 'e', 'f' },
            { 'g', 'h', 'i' },
        };

        ObservableCollection<string> source;


        [TestInitialize]
        public void Setup()
        {
            source = new ObservableCollection<string>() { "a", "b", "c", "d" };
        }

        [TestMethod]
        public void IndexTest()
        {
            var buffer = source.AsObservableQuery()
                .Select(i => i[0])
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(4, buffer.Count(), "Index does not have expected size.");
        }

        [TestMethod]
        public void IndexArrayStaticTest()
        {
            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => array[1])
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count(), "Index does not have expected size.");
        }

        [TestMethod]
        public void IndexArrayTest()
        {
            var a = array;

            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => a[1])
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count(), "Index does not have expected size.");
        }

        public void MultidimensionalArrayIndexTest()
        {
            var a = mdArray;

            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => a[1, 1])
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count(), "Index does not have expected size.");
        }

    }

}
