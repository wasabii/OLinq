using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class EnumerableSourceWithPredicateOperation<TSource, TResult> : EnumerableSourceWithLambdaOperation<TSource, bool, TResult>
    {

        public EnumerableSourceWithPredicateOperation(OperationContext context, MethodCallExpression expression, Expression sourceExpression, Expression<Func<TSource, bool>> predicateExpression)
            : base(context, expression, sourceExpression, predicateExpression)
        {

        }

        /// <summary>
        /// Gets the predicate collection.
        /// </summary>
        protected LambdaContainer<TSource, bool> Predicates
        {
            get { return Lambdas; }
        }

        protected override sealed void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            OnPredicateCollectionChanged(args);
        }

        /// <summary>
        /// Invoked when the predicate collection is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPredicateCollectionChanged(NotifyCollectionChangedEventArgs args)
        {

        }

        protected override void OnLambdaCollectionReset()
        {
            OnPredicateCollectionReset();
        }

        /// <summary>
        /// Invoked when the predicate collection is reset.
        /// </summary>
        protected virtual void OnPredicateCollectionReset()
        {

        }

        protected override sealed void OnLambdaCollectionItemsAdded(IEnumerable<LambdaOperation<bool>> newItems, int startingIndex)
        {
            OnPredicateCollectionItemsAdded(newItems, startingIndex);
        }

        /// <summary>
        /// Invoked when items are added to the predicate collection.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnPredicateCollectionItemsAdded(IEnumerable<LambdaOperation<bool>> newItems, int startingIndex)
        {

        }

        protected override sealed void OnLambdaCollectionItemsRemoved(IEnumerable<LambdaOperation<bool>> oldItems, int startingIndex)
        {
            OnPredicateCollectionItemsRemoved(oldItems, startingIndex);
        }

        /// <summary>
        /// Invoked when items are removed from the predicate collection.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnPredicateCollectionItemsRemoved(IEnumerable<LambdaOperation<bool>> oldItems, int startingIndex)
        {

        }

        protected override sealed void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            OnPredicateValueChanged(args);
        }

        /// <summary>
        /// Invoked when the value of a predicate is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPredicateValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {

        }

    }

}
