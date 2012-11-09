using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OLinq;

namespace OLinq.Tests
{

    [TestClass]
    public class CoreTests
    {

        ObservableCollection<string> c = new ObservableCollection<string>()
        {
            "A",
            "AA",
            "AAA",
            "AAAA",
            "AAAAA",
        };

        [TestMethod]
        public void ChainTest()
        {
            var c1 = c.AsObservableQuery()
                .Where(i => i.Length >= 3)
                .AsObservableQuery()
                .ToObservableView();

            var c2 = c1.Query()
                .Where(i => i == "ChainTest")
                .AsObservableQuery()
                .ToObservableView();

            bool s1 = false;
            c2.CollectionChanged += (s, a) => s1 = true;
            c.Add("ChainTest");
            Assert.IsTrue(s1);
        }

        [TestMethod]
        public void AnyTest()
        {
            var c1 = c.AsObservableQuery()
                .Observe(i => i.Any(j => j == "AnyTest"));

            bool b = false;
            c1.ValueChanged += (s, a) => b = (bool)a.NewValue;
            c.Add("AnyTest");
            Assert.IsTrue(b);
            c.Clear();
            Assert.IsFalse(b);
            c.Add("NotAnyTest");
            Assert.IsFalse(b);
            c.Add("AnyTest");
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void AllTest()
        {
            var c1 = c.AsObservableQuery()
                .Observe(i => i.All(j => j == "AllTest"));

            bool b = false;
            c1.ValueChanged += (s, a) => b = (bool)a.NewValue;
            c.Clear();
            Assert.IsTrue(b);
            c.Add("NotAllTest");
            Assert.IsFalse(b);
            c.Remove("NotAllTest");
            Assert.IsTrue(b);
            c.Add("AllTest");
            Assert.IsTrue(b);
        }

    }

}
