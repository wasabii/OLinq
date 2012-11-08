using System;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class AnyOperation<TSource> : GroupOperation<TSource, bool, bool>
    {

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, true)
        {

        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            if (!args.NewValue)
                SetValue(false);
            else
                ResetValue();
        }

        protected override bool RecalculateValue()
        {
            return Lambdas.Any(i => i.Value);
        }

    }

}
