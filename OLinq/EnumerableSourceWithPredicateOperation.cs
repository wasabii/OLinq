using System;
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
