using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MinTests
    {

        [TestMethod]
        public void MinTest()
        {
            var c = new ObservableCollection<int>()
            {
                1,
                2,
            };

            var b1 = c.AsObservableQuery()
                .Observe(i => i.Min());
            Assert.AreEqual(1, b1.Value);

            c.RemoveAt(0);
            Assert.AreEqual(2, b1.Value);
        }

    }

}
