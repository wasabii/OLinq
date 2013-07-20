using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class SelectManyTests
    {
         TestObservableCollection<List<string>> source;
         ObservableBuffer<string> buffer;

        [TestInitialize]
        public void SetUp()
        {
            source = new TestObservableCollection<List<string>>()
            {
                new[] {"1"}.ToList(),
                new[] {"2"}.ToList(),
                new[] {"3"}.ToList()
            };
            buffer = source.AsObservableQuery().SelectMany(a => a).AsObservableQuery().ToObservableView().ToBuffer();
        }

        [TestMethod]
        public void SelectManyTest()
        {
            var c1 = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3",
                "Item4",
                "Item5",
            };

            var c2 = new ObservableCollection<ObservableCollection<string>>()
            {
                c1,
                c1,
                c1,
            };

            var o = new SelectManyOperation<IEnumerable<string>, string>(new OperationContext(),
                Expression.Call(typeof(Enumerable), "SelectMany", new Type[] { typeof(IEnumerable<string>), typeof(string) },
                    Expression.Constant(c2),
                    Expression.Lambda<Func<IEnumerable<string>, IEnumerable<string>>>(
                        Expression.Parameter(typeof(IEnumerable<string>), "p"),
                        Expression.Parameter(typeof(IEnumerable<string>), "p"))));

            int added = 0;
            int removed = 0;
            o.CollectionChanged += (s, a) =>
            {
                switch (a.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        added++;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        removed++;
                        break;
                }
            };

            c1.Add("Item6");
            Assert.AreEqual(3, added);
            Assert.AreEqual(0, removed);

            c1.Remove("Item6");
            Assert.AreEqual(3, added);
            Assert.AreEqual(3, removed);
        }

        
        
        [TestMethod]
        public void NotYetImplemented_SelectManyWorks()
        {
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_CanAddToSourceCollection()
        {
            source.Insert(2, new List<string> { "B" });
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_CanRemoveFromSourceCollection()
        {
            source.RemoveAt(1);
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_CanInsertIntoChildCollections()
        {
            source[1].Add("A");
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_DoesntRemoveDuplicates()
        {
            source[0].Add("1");
            DoAssert();
            source[0].RemoveAt(0);
            DoAssert();
        }

        [TestMethod]
        public void NotYetImplemented_DoesntRemoveDuplicatesOnRemove()
        {
            source[0].Add("1");
            source[0].RemoveAt(0);
            DoAssert();
        }

        private void DoAssert()
        {
            Assert.AreEqual(string.Join("", source.SelectMany(c => c)), string.Join("", buffer));
        }


        [TestMethod]
        public void NotYetImplemented_EnumeratesSourceOnlyOnce()
        {
            buffer.ToArray();
            buffer.ToArray();
            Assert.AreEqual(1, source.EnumerationCount);
        }

    }
}
