using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class CountTests
    {

        [TestMethod]
        public void CountTest()
        {
            var c = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3",
                "Item4",
                "Item5",
            };

            var b1 = c.AsObservableQuery()
                .Observe(i => i.Count());
            Assert.AreEqual(5, b1.Value);

            c.RemoveAt(0);
            Assert.AreEqual(4, b1.Value);
        }

    }

}
