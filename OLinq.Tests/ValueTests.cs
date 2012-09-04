using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class ValueTests
    {

        int called = 0;
        
        [TestMethod]
        public void ValueTest()
        {
            var c = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3",
                "Item4",
                "Item5",
            };

            var v = c.AsObservableQuery()
                .Observe(i => i.Count());

            v.ValueChanged += v_ValueChanged;
            Assert.AreEqual(5, v.Value);

            c.Add("Item6");
            Assert.AreEqual(6, v.Value);

            c.Remove("Item1");
            Assert.AreEqual(5, v.Value);

            Assert.AreEqual(2, called);
        }

        void v_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            called++;
        }

    }

}
