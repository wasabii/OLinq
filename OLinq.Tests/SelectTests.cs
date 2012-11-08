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
                .ToView();
            q.CollectionChanged += q_CollectionChanged;

            Assert.AreEqual(q.Count(), 5);

            c.Add("TestItem1");
            Assert.AreEqual(1, added);
            Assert.AreEqual(q.Count(), 6);

            c.Remove("TestItem1");
            Assert.AreEqual(1, removed);
            Assert.AreEqual(q.Count(), 5);
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
                .ToView();
            q.CollectionChanged += q_CollectionChanged;

            Assert.AreEqual(q.Count(), 6);

            c[0].Add("Item2.5");
            Assert.AreEqual(q.Count(), 7);

            c[0].Remove("Item2.5");
            Assert.AreEqual(q.Count(), 6);
        }

    }

}
