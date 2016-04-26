using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents an operation that has an <see cref="IEnumerable{TSource}"/> and associates a predicate with each source element.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    abstract class EnumerableSourceWithPredicateOperation<TSource, TResult> :
        EnumerableSourceWithFuncOperation<TSource, bool, TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <param name="sourceExpression"></param>
        /// <param name="predicateExpression"></param>
        public EnumerableSourceWithPredicateOperation(OperationContext context, MethodCallExpression expression, Expression sourceExpression, Expression<Func<TSource, bool>> predicateExpression)
            : base(context, expression, sourceExpression, predicateExpression)
        {

        }

        /// <summary>
        /// Gets the predicate collection.
        /// </summary>
        protected FuncContainer<TSource, bool> Predicates
        {
            get { return Funcs; }
        }

        /// <summary>
        /// Invoked when the lambda collection is changed.
        /// </summary>
        /// <param name="args"></param>
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

        /// <summary>
        /// Invoked when the lambda collection is reset.
        /// </summary>
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

        /// <summary>
        /// Invoked when items are added to the lambda collection.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected override sealed void OnLambdaCollectionItemsAdded(IEnumerable<FuncOperation<bool>> newItems, int startingIndex)
        {
            OnPredicateCollectionItemsAdded(newItems, startingIndex);
        }

        /// <summary>
        /// Invoked when items are added to the predicate collection.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnPredicateCollectionItemsAdded(IEnumerable<FuncOperation<bool>> newItems, int startingIndex)
        {

        }

        /// <summary>
        /// Invoked when items are removed from the lambda collection.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected override sealed void OnLambdaCollectionItemsRemoved(IEnumerable<FuncOperation<bool>> oldItems, int startingIndex)
        {
            OnPredicateCollectionItemsRemoved(oldItems, startingIndex);
        }

        /// <summary>
        /// Invoked when items are removed from the predicate collection.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnPredicateCollectionItemsRemoved(IEnumerable<FuncOperation<bool>> oldItems, int startingIndex)
        {

        }

        /// <summary>
        /// Invoked when the return value of one of the lambda items is changed.
        /// </summary>
        /// <param name="args"></param>
        protected override sealed void OnLambdaValueChanged(FuncValueChangedEventArgs<TSource, bool> args)
        {
            OnPredicateValueChanged(args);
        }

        /// <summary>
        /// Invoked when the value of a predicate is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPredicateValueChanged(FuncValueChangedEventArgs<TSource, bool> args)
        {

        }

    }

}
