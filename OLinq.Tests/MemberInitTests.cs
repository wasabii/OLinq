using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MemberInitTests
    {

        public class Foo : INotifyPropertyChanged
        {

            private string name;

            public string Name
            {
                get { return name; }
                set { name = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name")); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

        }

        public class FooIn : Foo
        {



        }

        public class FooOut : Foo
        {


        }

        ObservableCollection<FooIn> c = new ObservableCollection<FooIn>()
        {
            new FooIn() { Name = "A" },
            new FooIn() { Name = "AA" },
            new FooIn() { Name = "AAA" },
            new FooIn() { Name = "AAAA" },
        };

        [TestMethod]
        public void MemberInitTest1()
        {
            int count = 0;

            var v = c.AsObservableQuery()
                .Select(i => new FooOut()
                {
                    Name = i.Name,
                })
                .Where(i => i.Name.EndsWith("_End"))
                .AsObservableQuery()
                .ToView();
            v.CollectionChanged += (s, a) => count++;

            foreach (var i in c)
                i.Name = i.Name + "_End";

            Assert.AreEqual(4, count);
        }

    }

}
