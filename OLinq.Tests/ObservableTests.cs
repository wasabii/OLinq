using System;
using System.Linq;
using System.Reactive.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class ObservableTests
    {

        TestObservableCollection<int> source;
        ObservableValue<int> value;

        [TestInitialize]
        public void SetupFilters()
        {
            source = new TestObservableCollection<int>()
            {
                1, 2, 3, 4,
            };
            value = source.AsObservableQuery()
                .Observe(i => i.Max());
        }

        [TestMethod]
        public void ObservableSubscription()
        {
            // wait for max
            int max = value.Value;
            value.Subscribe(i => max = i);

            source.Add(1);
            Assert.AreEqual(4, max);
            source.Add(9);
            Assert.AreEqual(9, max);
            source.Add(11);
            Assert.AreEqual(11, max);
            source.Add(5);
            Assert.AreEqual(11, max);
        }

    }

}
