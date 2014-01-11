using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MemberAccessTests
    {

        static readonly object nullValue = null;
        static readonly object nonNullValue = new object();

        [TestMethod]
        public void Test_read()
        {
            var src = new NotificationObject<string>()
            {
                Value1 = "Test",
            };

            var op = new MemberAccessOperation<string>(new OperationContext(),
                Expression.MakeMemberAccess(
                    Expression.Constant(src),
                    typeof(NotificationObject<string>).GetProperty("Value1")));
            Assert.AreEqual("Test", op.Value);
        }

        [TestMethod]
        public void Test_write()
        {
            var src = new NotificationObject<string>()
            {
                Value1 = "Test",
            };

            var op = new MemberAccessOperation<string>(new OperationContext(),
                Expression.MakeMemberAccess(
                    Expression.Constant(src),
                    typeof(NotificationObject<string>).GetProperty("Value1")));
            Assert.AreEqual("Test", op.Value);

            src.Value1 = "Test2";
            Assert.AreEqual("Test2", op.Value);
        }

        [TestMethod]
        public void Test_static_null()
        {
            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => nullValue)
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count());
        }

        [TestMethod]
        public void Test_static_not_null()
        {
            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => nonNullValue)
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count());
        }

        [TestMethod]
        public void Test_null_instance()
        {
            var buffer = Enumerable.Repeat((string)null, 1).AsObservableQuery()
                .Select(i => i.EndsWith("1"))
                .AsObservableQuery()
                .WithNullSafe(true)
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(0, buffer.Count());
        }

    }

}
