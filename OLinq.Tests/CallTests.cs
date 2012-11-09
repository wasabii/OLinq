using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class CallTests
    {

        [TestMethod]
        public void CallOperationRead()
        {
            var op = new CallOperation<bool>(new OperationContext(),
                Expression.Call(
                    Expression.Constant("Test"),
                    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }),
                    Expression.Constant("Te")));
            Assert.IsTrue(op.Value);
        }

        [TestMethod]
        public void CallOperationUpdateInstance()
        {
            var o = new NotificationObject<string>()
            {
                Value1 = "Test",
            };

            var op = new CallOperation<bool>(new OperationContext(),
                Expression.Call(
                    Expression.MakeMemberAccess(
                        Expression.Constant(o),
                        typeof(NotificationObject<string>).GetProperty("Value1")),
                    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }),
                    Expression.Constant("Te")));
            Assert.IsTrue(op.Value);

            o.Value1 = "NotTest";
            Assert.IsFalse(op.Value);

            o.Value1 = "Test";
            Assert.IsTrue(op.Value);
        }

        [TestMethod]
        public void CallOperationUpdateArgument()
        {
            var o1 = new NotificationObject<string>()
            {
                Value1 = "Test",
                Value2 = "Te",
            };

            var op = new CallOperation<bool>(new OperationContext(),
                Expression.Call(
                    Expression.MakeMemberAccess(
                        Expression.Constant(o1),
                        typeof(NotificationObject<string>).GetProperty("Value1")),
                    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }),
                    Expression.MakeMemberAccess(
                        Expression.Constant(o1),
                        typeof(NotificationObject<string>).GetProperty("Value2"))));
            Assert.IsTrue(op.Value);

            o1.Value2 = "st";
            Assert.IsFalse(op.Value);

            o1.Value2 = "Te";
            Assert.IsTrue(op.Value);
        }

    }

}
