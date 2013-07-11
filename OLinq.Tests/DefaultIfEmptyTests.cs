using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class DefaultIfEmptyTests
    {

        ObservableCollection<string> items;

        [TestInitialize]
        public void SetupFilters()
        {
            items = new ObservableCollection<string>() { "a", "b", "c", "d" };
        }

        [TestMethod]
        public void DefaultIfEmptyTest()
        {
            var c = items.AsObservableQuery()
                .DefaultIfEmpty("Test")
                .AsObservableQuery()
                .ToObservableView();

            Assert.AreEqual(4, c.Count());
            items.Clear();
            Assert.AreEqual(1, c.Count());
            Assert.AreEqual("Test", c.FirstOrDefault());
        }

    }

}
