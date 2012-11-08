using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class AllOperation<TSource> : GroupOperation<TSource, bool>
    {
        
        LambdaOperationContainer<TSource, bool> predicates;

        public AllOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            SetValue(false);
            predicates = new LambdaOperationContainer<TSource, bool>(
                ((MethodCallExpression)Expression).GetArgument(1).UnpackLambda<TSource, bool>(),
                CreatePredicateContext);
            predicates.CollectionChanged += predicates_CollectionChanged;
            predicates.LambdaValueChanged += predicates_LambdaValueChanged;
            predicates.Items = SourceCollection;
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

        /// <summary>
        /// Invoked when the predicate collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
                    ResetValue();
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
            if (!args.NewValue)
                SetValue(false);
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

            // update the predicate collection
            if (predicates != null)
                predicates.Items = newSource;
        }

        /// <summary>
        /// Recalculates the value.
        /// </summary>
        void ResetValue()
        {
            SetValue(predicates.All(i => i.Value));
        }

        public override void Init()
        {
            base.Init();

            // so event gets raised regardless of order of initialization
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
