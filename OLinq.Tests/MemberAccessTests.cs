using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{

    [TestClass]
    public class MemberAccessTests
    {

        [TestMethod]
        public void MemberAccessOperationRead()
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
        public void MemberAccessOperationWrite()
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

    }

}
