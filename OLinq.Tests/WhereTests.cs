using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class WhereTests
    {

        int resetted;

        [TestMethod]
        public void WhereTest()
        {
            var c = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3",
                "Item4",
                "Item5",
            };

            var q = c.AsObservableQuery()
                .Where(i => i.EndsWith("3"))
                .AsObservableQuery();
            q.CollectionChanged += q_CollectionChanged;

            Assert.AreEqual(1, q.Count());

            c.Add("TestItem3");
            Assert.AreEqual(1, resetted);
            Assert.AreEqual(2, q.Count());

            c.Remove("TestItem3");
            Assert.AreEqual(2, resetted);
            Assert.AreEqual(1, q.Count());
        }

        void q_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
                resetted++;
        }

    }

}
