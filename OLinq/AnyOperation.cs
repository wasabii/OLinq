using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class AnyOperation<TSource> : GroupOperation<TSource, bool>
    {

        LambdaOperationContainer<TSource, bool> predicates;

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            var expr = (MethodCallExpression)Expression;
            if (expr.Arguments.Count >= 2)
            {
                predicates = new LambdaOperationContainer<TSource, bool>(Utils.UnpackLambda(expr.Arguments[1]), CreatePredicateContext);
                predicates.CollectionChanged += predicates_CollectionChanged;
                predicates.LambdaValueChanged += predicates_LambdaValueChanged;
                predicates.Items = SourceCollection;
            }

            OnValueChanged(null, Value);
        }

        /// <summary>
        /// Creates a context for a new predicate operation.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        OperationContext CreatePredicateContext(TSource item)
        {
            // generate new parameter
            var ctx = new OperationContext(Context);
            var var = OperationFactory.FromValue(item);
            ctx.Variables[predicates.Expression.Parameters[0].Name] = var;
            return ctx;
        }

        void predicates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (Value || args.NewItems.Cast<LambdaOperation<bool>>().Any(i => i.Value))
                        SetValue(true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ResetValue();
                    break;
            }
        }

        /// <summary>
        /// Invoked when one of the predicates has a value change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void predicates_LambdaValueChanged(object sender, LambdaValueChangedEventArgs<TSource, bool> args)
        {
            if (args.NewValue)
                SetValue(true);
            else
                ResetValue();
        }

        /// <summary>
        /// Invoked when the underlying source changes.
        /// </summary>
        /// <param name="oldSource"></param>
        /// <param name="newSource"></param>
        protected override void SourceChanged(IEnumerable<TSource> oldSource, IEnumerable<TSource> newSource)
        {
            base.SourceChanged(oldSource, newSource);

            if (predicates != null)
                predicates.Items = newSource;
        }

        /// <summary>
        /// Recalculates the value.
        /// </summary>
        void ResetValue()
        {
            SetValue(predicates.Any(i => i.Value));
        }

        public override void Init()
        {
            base.Init();
            OnValueChanged(null, Value);
        }

        public override void Dispose()
        {
            if (predicates != null)
            {
                predicates.Dispose();
                predicates = null;
            }

            base.Dispose();
        }

    }

}
