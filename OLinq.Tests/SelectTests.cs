using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class SelectTests
    {

        int added = 0;
        int removed = 0;

        [TestMethod]
        public void SelectTest()
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
                .Select(i => new { Value = i })
                .AsObservableQuery()
                .ToObservableView();
            q.CollectionChanged += q_CollectionChanged;

            Assert.AreEqual(q.Count(), 5);

            c.Add("TestItem1");
            Assert.AreEqual(1, added);
            Assert.AreEqual(6, q.Count());

            c.Remove("TestItem1");
            Assert.AreEqual(1, removed);
            Assert.AreEqual(5, q.Count());
        }

        void q_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
                added++;
            else if (args.Action == NotifyCollectionChangedAction.Remove)
                removed++;
        }

        [TestMethod]
        public void SelectManyTest()
        {
            var c = new ObservableCollection<ObservableCollection<string>>()
            {
                new ObservableCollection<string>() { "Item1", "Item2" },
                new ObservableCollection<string>() { "Item3", "Item4" },
                new ObservableCollection<string>() { "Item5", "Item6" },
            };

            var q = c.AsObservableQuery()
                .SelectMany(i => i)
                .AsObservableQuery()
                .ToObservableView();
            q.CollectionChanged += q_CollectionChanged;

            Assert.AreEqual(6, q.Count());

            c[0].Add("Item2.5");
            Assert.AreEqual(7, q.Count());

            c[0].Remove("Item2.5");
            Assert.AreEqual(6, q.Count());
        }

        [TestMethod]
        public void SelectNullTest()
        {
            var buffer = Enumerable.Range(1, 10).AsObservableQuery()
                .Select(i => (object)null)
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(10, buffer.Count());
        }

    }

}
