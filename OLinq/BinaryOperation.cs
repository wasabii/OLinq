using System;
using System.Linq.Expressions;

namespace OLinq
{

    class BinaryOperation : Operation<bool>
    {

        private BinaryExpression self;
        private IOperation left;
        private IOperation right;

        public BinaryOperation(OperationContext context, BinaryExpression expression)
            : base(context, expression)
        {
            self = expression;

            left = OperationFactory.FromExpression(context, expression.Left);
            left.ValueChanged += left_ValueChanged;

            right = OperationFactory.FromExpression(context, expression.Right);
            right.ValueChanged += right_ValueChanged;
        }

        void left_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (!IsLoaded)
                return;

            Reset();
        }

        void right_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (!IsLoaded)
                return;

            Reset();
        }

        bool Reset()
        {
            return SetValue(
                Expression.Lambda<Func<bool>>(Expression.MakeBinary(self.NodeType,
                    Expression.Constant(left.Value, self.Left.Type),
                    Expression.Constant(right.Value, self.Right.Type)))
                        .Compile()());
        }

        public override void Load()
        {
            if (left != null)
                left.Load();
            if (right != null)
                right.Load();

            base.Load();

            // set our initial value after both left and right initial values have loaded
            Reset();
        }

        public override void Dispose()
        {
            if (left != null)
            {
                left.ValueChanged -= left_ValueChanged;
                left.Dispose();
            }
            if (right != null)
            {
                right.ValueChanged -= right_ValueChanged;
                right.Dispose();
            }

            base.Dispose();
        }

    }

}
