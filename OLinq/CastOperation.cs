using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class CastOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(Enumerable) &&
                expression.Method.DeclaringType != typeof(Queryable))
                throw new InvalidOperationException("Requires Enumerable or Queryable method.");

            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(CastOperation<>), context, expression, 0);

            throw new NotSupportedException("Cast operation not found.");
        }

    }

    class CastOperation<TResult> : EnumerableSourceOperation<object, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        public CastOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<object> newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.Cast<TResult>().ToList(), startingIndex));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<object> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.Cast<TResult>().ToList(), startingIndex));
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Source.Cast<TResult>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
