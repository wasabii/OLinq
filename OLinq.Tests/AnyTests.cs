using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class AnyTests
    {

        [TestMethod]
        public void AnyTest()
        {
            var c = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3",
                "Item4",
                "Item5",
            };

            var b1 = c.AsObservableQuery()
                .Any();

            Assert.IsTrue(b1);

            var b2 = c.AsObservableQuery()
                .Where(i => i.StartsWith("B"))
                .Any();

            Assert.IsFalse(b2);
        }

    }

}
