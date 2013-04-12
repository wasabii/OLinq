using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class WhereTests
    {

        [TestMethod]
        public void WhereOperationRead()
        {
            var c = new ObservableCollection<NotificationObject<string>>()
            {
                new NotificationObject<string>() { Value1 = "Test1" },
                new NotificationObject<string>() { Value1 = "Test2" },
                new NotificationObject<string>() { Value1 = "Test3" },
            };

            var op = new WhereOperation<NotificationObject<string>>(new OperationContext(),
                Expression.Call(
                    typeof(Queryable).GetMethods()
                        .Where(i => i.Name == "Where")
                        .Where(i => i.IsGenericMethodDefinition)
                        .Where(i => i.GetGenericArguments().Length == 1)
                        .Select(i => i.MakeGenericMethod(typeof(NotificationObject<string>)))
                        .Where(i => i.GetParameters().Length == 2)
                        .Where(i => i.GetParameters()[1].ParameterType == typeof(Expression<Func<NotificationObject<string>, bool>>))
                        .Single(),
                    new ObservableQuery<NotificationObject<string>>(c).Expression,
                    Expression.Lambda<Func<NotificationObject<string>, bool>>(
                        Expression.Constant(true),
                        Expression.Parameter(typeof(NotificationObject<string>), "i"))));
            Assert.AreEqual(3, op.Value.Count());
        }

        [TestMethod]
        public void WhereOperationAdd()
        {
            var c = new ObservableCollection<NotificationObject<string>>()
            {
                new NotificationObject<string>() { Value1 = "Test1" },
                new NotificationObject<string>() { Value1 = "Test2" },
                new NotificationObject<string>() { Value1 = "Test3" },
            };

            var op = new WhereOperation<NotificationObject<string>>(new OperationContext(),
                Expression.Call(
                    typeof(Queryable).GetMethods()
                        .Where(i => i.Name == "Where")
                        .Where(i => i.IsGenericMethodDefinition)
                        .Where(i => i.GetGenericArguments().Length == 1)
                        .Select(i => i.MakeGenericMethod(typeof(NotificationObject<string>)))
                        .Where(i => i.GetParameters().Length == 2)
                        .Where(i => i.GetParameters()[1].ParameterType == typeof(Expression<Func<NotificationObject<string>, bool>>))
                        .Single(),
                    new ObservableQuery<NotificationObject<string>>(c).Expression,
                    Expression.Lambda<Func<NotificationObject<string>, bool>>(
                        Expression.Constant(true),
                        Expression.Parameter(typeof(NotificationObject<string>), "i"))));
            Assert.AreEqual(3, op.Value.Count());

            c.Add(new NotificationObject<string>() { Value1 = "Test4" });
            Assert.AreEqual(4, op.Value.Count());
        }

        [TestMethod]
        public void WhereOperationPredicate()
        {
            var c = new ObservableCollection<NotificationObject<string>>()
            {
                new NotificationObject<string>() { Value1 = "False" },
                new NotificationObject<string>() { Value1 = "True" },
                new NotificationObject<string>() { Value1 = "False" },
            };

            var op = new WhereOperation<NotificationObject<string>>(new OperationContext(),
                Expression.Call(
                    typeof(Queryable).GetMethods()
                        .Where(i => i.Name == "Where")
                        .Where(i => i.IsGenericMethodDefinition)
                        .Where(i => i.GetGenericArguments().Length == 1)
                        .Select(i => i.MakeGenericMethod(typeof(NotificationObject<string>)))
                        .Where(i => i.GetParameters().Length == 2)
                        .Where(i => i.GetParameters()[1].ParameterType == typeof(Expression<Func<NotificationObject<string>, bool>>))
                        .Single(),
                    new ObservableQuery<NotificationObject<string>>(c).Expression,
                    Expression.Lambda<Func<NotificationObject<string>, bool>>(
                        Expression.Equal(
                            Expression.MakeMemberAccess(
                                Expression.Parameter(typeof(NotificationObject<string>), "i"),
                                typeof(NotificationObject<string>).GetProperty("Value1")),
                            Expression.Constant("True", typeof(string))),
                        Expression.Parameter(typeof(NotificationObject<string>), "i"))));
            Assert.AreEqual(1, op.Value.Count());

            c.Add(new NotificationObject<string>() { Value1 = "False" });
            Assert.AreEqual(1, op.Value.Count());

            c.Add(new NotificationObject<string>() { Value1 = "True" });
            Assert.AreEqual(2, op.Value.Count());

            c.Add(new NotificationObject<string>() { Value1 = "True" });
            Assert.AreEqual(3, op.Value.Count());

            c.RemoveAt(1);
            Assert.AreEqual(2, op.Value.Count());

            c[0].Value1 = "True";
            Assert.AreEqual(3, op.Value.Count());

            c[1].Value1 = "True";
            Assert.AreEqual(4, op.Value.Count());
        }

        [TestMethod]
        public void WhereTest()
        {
            var c = new ObservableCollection<string>()
            {
                "False",
                "False",
                "True",
                "False",
                "False",
            };

            int changed = 0;

            var q = c.AsObservableQuery()
                .Where(i => i == "True")
                .AsObservableQuery()
                .ToObservableView();
            q.CollectionChanged += (s, a) => changed++;

            Assert.AreEqual(1, q.Count());

            c.Add("True");
            Assert.AreEqual(1, changed);
            Assert.AreEqual(2, q.Count());

            c.Remove("True");
            Assert.AreEqual(2, changed);
            Assert.AreEqual(1, q.Count());
        }

        [TestMethod]
        public void WhereTestUsing_OrElse_and_NotEquals()
        {
            var c = new ObservableCollection<string>() {"1", "2", "3"}.AsObservableQuery()
                .Where(i => (i != "1" && i != "2") || i == "3").AsObservableQuery().ToObservableView().ToBuffer();
            Assert.AreEqual("3", string.Join("",c));
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException), "This was expected to fail as the feature is not implemented")]
        public void WhereBufferTest()
        {
            var c = new ObservableCollection<string>() { "a", "b", "c", "b", "c", "a"};

            var buffer = c.AsObservableQuery().Where(i => i != "a")
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();
            Assert.AreEqual("bcbc", string.Join("", buffer));
            
            c.Insert(0, "a");
            Assert.AreEqual("bcbc", string.Join("", buffer));
            
            c.Insert(0, "c");
            Assert.AreEqual("cbcbc", string.Join("", buffer));

            
            c.Insert(4, "b");
            Assert.AreEqual("cbcbcb", string.Join("", buffer));
        }

    }

}
