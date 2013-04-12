using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class GroupByTests
    {

        [TestMethod]
        public void GroupByOperation()
        {
            var c = new ObservableCollection<NotificationObject<string>>()
            {
                new NotificationObject<string>() { Value1 = "Group1", Value2 = "Item1" },
                new NotificationObject<string>() { Value1 = "Group1", Value2 = "Item2" },
                new NotificationObject<string>() { Value1 = "Group1", Value2 = "Item3" },
                new NotificationObject<string>() { Value1 = "Group2", Value2 = "Item4" },
                new NotificationObject<string>() { Value1 = "Group2", Value2 = "Item5" },
                new NotificationObject<string>() { Value1 = "Group2", Value2 = "Item6" },
            };

            var op = new GroupByOperation<NotificationObject<string>, string>(new OperationContext(),
                Expression.Call(
                    typeof(Queryable).GetMethods()
                        .Where(i => i.Name == "GroupBy")
                        .Where(i => i.IsGenericMethodDefinition)
                        .Where(i => i.GetGenericArguments().Length == 2)
                        .Select(i => i.MakeGenericMethod(typeof(NotificationObject<string>), typeof(string)))
                        .Where(i => i.GetParameters().Length == 2)
                        .Where(i => i.GetParameters()[1].ParameterType == typeof(Expression<Func<NotificationObject<string>, string>>))
                        .Single(),
                    new ObservableQuery<NotificationObject<string>>(c).Expression,
                    Expression.Lambda<Func<NotificationObject<string>, string>>(
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(NotificationObject<string>), "p"),
                            typeof(NotificationObject<string>).GetProperty("Value1")),
                        Expression.Parameter(typeof(NotificationObject<string>), "p"))));
            Assert.AreEqual(2, op.Value.Count());
            Assert.AreEqual(3, op.Value.ToList()[0].Count());
            Assert.AreEqual(3, op.Value.ToList()[1].Count());

            c.Add(new NotificationObject<string>() { Value1 = "Group1", Value2 = "Item7" });
            Assert.AreEqual(2, op.Value.Count());
            Assert.AreEqual(4, op.Value.ToList()[0].Count());
            Assert.AreEqual(3, op.Value.ToList()[1].Count());

            c.Add(new NotificationObject<string>() { Value1 = "Group2", Value2 = "Item8" });
            Assert.AreEqual(2, op.Value.Count());
            Assert.AreEqual(4, op.Value.ToList()[0].Count());
            Assert.AreEqual(4, op.Value.ToList()[1].Count());

            c[0].Value1 = "Group3";
            Assert.AreEqual(3, op.Value.Count());
            Assert.AreEqual(3, op.Value.ToList()[0].Count());
            Assert.AreEqual(4, op.Value.ToList()[1].Count());
        }

    }

}
