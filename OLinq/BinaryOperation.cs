using System;
using System.Linq.Expressions;

namespace OLinq
{

    static class BinaryOperation
    {



    }

    class BinaryOperation<T> : Operation<T>
    {

        BinaryExpression self;
        IOperation left;
        IOperation right;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public BinaryOperation(OperationContext context, BinaryExpression expression)
            : base(context, expression)
        {
            self = expression;

            if (self.Left == null)
                throw new ArgumentNullException("Left side of expression must not be null.");
            if (self.Right == null)
                throw new ArgumentNullException("Right side of expression must not be null.");

            left = OperationFactory.FromExpression(context, expression.Left);
            left.ValueChanged += left_ValueChanged;

            right = OperationFactory.FromExpression(context, expression.Right);
            right.ValueChanged += right_ValueChanged;

            ResetValue();
        }

        void left_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            ResetValue();
        }

        void right_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            ResetValue();
        }

        /// <summary>
        /// Gets the operation on the left.
        /// </summary>
        protected IOperation Left
        {
            get { return left; }
        }

        /// <summary>
        /// Gets the operation on the right.
        /// </summary>
        protected IOperation Right
        {
            get { return right; }
        }

        /// <summary>
        /// Resets the output value.
        /// </summary>
        /// <returns></returns>
        protected T ResetValue()
        {
            return SetValue(GetValue());
        }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns></returns>
        protected virtual T GetValue()
        {
            return
                Expression.Lambda<Func<T>>(Expression.MakeBinary(self.NodeType,
                   Expression.Constant(left.Value, self.Left.Type),
                   Expression.Constant(right.Value, self.Right.Type)))
                       .Compile()();
        }

        public override void Dispose()
        {
            if (left != null)
            {
                left.ValueChanged -= left_ValueChanged;
                left.Dispose();
                left = null;
            }

            if (right != null)
            {
                right.ValueChanged -= right_ValueChanged;
                right.Dispose();
                right = null;
            }

            base.Dispose();
        }

    }

}
