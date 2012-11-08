using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SumInt32Operation<TSource> : GroupOperation<TSource, int, int>
    {

        int sum = 0;

        public SumInt32Operation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.GetLambdaArgument<TSource, int>(1) ?? (Expression<Func<TSource,int>>)Utils.CreateSelfLambdaExpression(typeof(int)))
        {

        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    SetValue(sum += args.NewItems.Cast<LambdaOperation<int>>().Sum(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(sum -= args.OldItems.Cast<LambdaOperation<int>>().Sum(i => i.Value));
                    break;
            }
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, int> args)
        {
            SetValue(sum = sum - args.OldValue + args.NewValue);
        }

        protected override int RecalculateValue()
        {
            return sum = Lambdas.Sum(i => i.Value);
        }

    }

}
