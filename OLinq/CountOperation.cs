using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class CountOperation<TSource> : GroupOperation<TSource, bool, int>
    {

        int count = 0;

        public CountOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, true)
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
                    SetValue(count += args.NewItems.Cast<LambdaOperation<bool>>().Count(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(count -= args.OldItems.Cast<LambdaOperation<bool>>().Count(i => i.Value));
                    break;
            }
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            if (!args.OldValue && args.NewValue)
                SetValue(++count);
            else if (args.OldValue && !args.NewValue)
                SetValue(--count);
        }

        protected override int RecalculateValue()
        {
            return count = Lambdas.Count(i => i.Value);
        }

    }

}
