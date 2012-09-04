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
            Reset();
        }

        void right_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Reset();
        }

        bool Reset()
        {
            if (left.Value != null &&
                right.Value != null)
                return SetValue(
                    Expression.Lambda<Func<bool>>(Expression.MakeBinary(self.NodeType,
                        Expression.Constant(left.Value, self.Left.Type),
                        Expression.Constant(right.Value, self.Right.Type)))
                            .Compile()());
            else
                return SetValue(false);
        }

        public override void Load()
        {
            if (left != null)
                left.Load();
            if (right != null)
                right.Load();

            base.Load();
        }

    }

}
