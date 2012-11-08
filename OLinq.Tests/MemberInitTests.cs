using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
                set { name = value; OnPropertyChanged("Name"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        public class FooIn : Foo
        {



        }

        public class FooOut : Foo
        {

            private bool isItTrue;

            public bool IsItTrue
            {
                get { return isItTrue; }
                set { isItTrue = value; OnPropertyChanged("IsItTrue"); }
            }

        }

        ObservableCollection<FooIn> c = new ObservableCollection<FooIn>()
        {
            new FooIn() { Name = "A" },
        };

        [TestMethod]
        public void MemberInitTest1()
        {
            int count = 0;

            var v = c.AsObservableQuery()
                .Select(i => new FooOut()
                {
                    IsItTrue = i.Name.Any(j => j == 'Z'),
                })
                .AsObservableQuery()
                .ToView();

            c[0].Name += "Z";
            Assert.AreEqual(true, v.Count(i => i.IsItTrue) == 1);
        }

    }

}
