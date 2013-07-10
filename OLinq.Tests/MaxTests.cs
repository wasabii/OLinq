using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MaxTests
    {

        [TestMethod]
        public void MaxTest()
        {
            var c = new ObservableCollection<int>()
            {
                1,
                2,
            };

            var b1 = c.AsObservableQuery()
                .Observe(i => i.Max());
            Assert.AreEqual(2, b1.Value);

            c.RemoveAt(1);
            Assert.AreEqual(1, b1.Value);
        }

        [TestMethod]
        [TestCategory("Bugged")]
        public void MaxDuplicateValueTest()
        {

        }

    }

}
