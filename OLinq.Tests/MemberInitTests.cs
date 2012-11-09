using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MemberInitTests
    {

        [TestMethod]
        public void MemberInitOperationRead()
        {
            var src = new NotificationObject<string>()
            {
                Value1 = "Test",
            };

            var op = new MemberInitOperation<NotificationObject<string>>(new OperationContext(),
                Expression.MemberInit(
                    Expression.New(typeof(NotificationObject<string>)),
                    Expression.Bind(typeof(NotificationObject<string>).GetProperty("Value2"),
                        Expression.MakeMemberAccess(
                            Expression.Constant(src),
                            typeof(NotificationObject<string>).GetProperty("Value1")))));
            Assert.AreEqual("Test", op.Value.Value2);
        }

        [TestMethod]
        public void MemberInitOperationWrite()
        {
            var src = new NotificationObject<string>()
            {
                Value1 = "Test",
            };

            var op = new MemberInitOperation<NotificationObject<string>>(new OperationContext(),
                Expression.MemberInit(
                    Expression.New(typeof(NotificationObject<string>)),
                    Expression.Bind(typeof(NotificationObject<string>).GetProperty("Value2"),
                        Expression.MakeMemberAccess(
                            Expression.Constant(src),
                            typeof(NotificationObject<string>).GetProperty("Value1")))));

            src.Value1 = "Test2";
            Assert.AreEqual("Test2", op.Value.Value2);
        }

    }

}
