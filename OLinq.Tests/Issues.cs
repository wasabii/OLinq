using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class Issues
    {

        [TestMethod]
        public void Issue_12()
        {
            var oc = new ObservableCollection<int>();

            oc.Add(10);
            oc.Add(20);
            oc.Add(30);

            var ov = oc.AsObservableQuery().Where(i => i < 30).AsObservableQuery().ToObservableView();

            oc.Remove(20);
        }

    }

}
