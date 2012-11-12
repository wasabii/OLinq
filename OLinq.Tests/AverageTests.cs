using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class Average
    {

        [TestMethod]
        public void AverageTest()
        {
            var c = new ObservableCollection<int>()
            {
                1,2,3,
            };

            var q = c.AsObservableQuery()
                .Observe(i => i.Average());
        }

    }

}
