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

        [TestMethod]
        public void MemberAccessStaticNullTest()
        {
            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => nullValue)
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count());
        }

        [TestMethod]
        public void MemberAccessStaticNonNullTest()
        {
            var buffer = Enumerable.Range(0, 1).AsObservableQuery()
                .Select(i => nonNullValue)
                .AsObservableQuery()
                .ToObservableView()
                .ToBuffer();

            Assert.AreEqual(1, buffer.Count());
        }

        [TestMethod]
        public void Test_member_access_non_static()
        {

        }

    }

}
