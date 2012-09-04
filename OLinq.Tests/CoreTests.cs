using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class CoreTests
    {

        int resetted = 0;

        [TestMethod]
        public void ChainTest()
        {
            var c = new ObservableCollection<string>()
            {
                "A",
                "AA",
                "AAA",
                "AAAA",
                "AAAAA",
            };

            var c1 = c.AsObservableQuery()
                .Where(i => i.Length >= 3)
                .AsObservableQuery()
                .ToCollection();

            var c2 = c1
                .Where(i => i.Length >= 4)
                .AsObservableQuery();

            c2.CollectionChanged += c2_CollectionChanged;
            c.Add("AAAAAA");

            Assert.AreEqual(1, resetted);
        }

        void c2_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                resetted++;
        }

    }

}
