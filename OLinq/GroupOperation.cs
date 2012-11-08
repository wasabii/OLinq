using System;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class GroupOperation<TSource, TLambdaResult, TResult> : SingleEnumerableLambdaSourceOperation<TSource, TLambdaResult, TResult>
    {

        public GroupOperation(OperationContext context, MethodCallExpression expression, Expression<Func<TSource, TLambdaResult>> lambdaExpression)
            : base(context, expression, lambdaExpression)
        {
            ResetValue();
        }

        public GroupOperation(OperationContext context, MethodCallExpression expression, TLambdaResult defaultLambdaResult)
            : base(context, expression, defaultLambdaResult)
        {
            ResetValue();
        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            ResetValue();
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, TLambdaResult> args)
        {
            ResetValue();
        }

        /// <summary>
        /// Gets the new value by recalculating the results.
        /// </summary>
        /// <returns></returns>
        protected abstract TResult RecalculateValue();

        /// <summary>
        /// Recalculates the value and sets it.
        /// </summary>
        protected void ResetValue()
        {
            SetValue(RecalculateValue());
        }

        public override void Init()
        {
            base.Init();

            // so event gets raised regardless of order of initialization
            OnValueChanged(null, Value);
        }

    }

}
