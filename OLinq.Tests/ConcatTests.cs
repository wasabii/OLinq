using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class ConcatTests
    {

        [TestMethod]
        public void ConcatTest()
        {

            int changes = 0;

            var c1 = new ObservableCollection<string>()
            {
                "Item_1_1",
                "Item_1_2",
                "Item_1_3",
                "Item_1_4",
                "Item_1_5",
            };

            var c2 = new ObservableCollection<string>()
            {
                "Item_2_1",
                "Item_2_2",
                "Item_2_3",
                "Item_2_4",
                "Item_2_5",
            };

            var q = c1.AsObservableQuery()
                .Concat(c2)
                .AsObservableQuery()
                .ToView();
            q.CollectionChanged += (s, a) => changes++;

            Assert.AreEqual(q.Count(), 10);

            c1.Add("Item_1_6");
            Assert.AreEqual(1, changes);
            Assert.AreEqual(q.Count(), 11);
            c1.Remove("Item_1_6");
            Assert.AreEqual(2, changes);
            Assert.AreEqual(q.Count(), 10);

            c1.Add("Item_2_6");
            Assert.AreEqual(3, changes);
            Assert.AreEqual(q.Count(), 11);
            c1.Remove("Item_2_6");
            Assert.AreEqual(4, changes);
            Assert.AreEqual(q.Count(), 10);
        }

    }

}
