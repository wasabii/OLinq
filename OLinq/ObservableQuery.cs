using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public abstract class ObservableQuery
    {

        public static IQueryable Create(Type elementType, Expression expression)
        {
            return (IQueryable)Activator.CreateInstance(typeof(ObservableQuery<>).MakeGenericType(elementType), expression);
        }

        public static IQueryable Create(Type elementType, IEnumerable enumerable)
        {
            return (IQueryable)Activator.CreateInstance(typeof(ObservableQuery<>).MakeGenericType(elementType), enumerable);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected ObservableQuery()
        {

        }

        internal abstract IEnumerable Enumerable { get; }

        internal abstract Expression Expression { get; }

    }

    public class ObservableQuery<T> : ObservableQuery, IQueryable<T>, IQueryProvider, INotifyCollectionChanged, IDisposable
    {

        private int refCount = 0;
        private IEnumerable<T> enumerable;
        private Expression expression;
        private IOperation<IEnumerable<T>> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="enumerable"></param>
        public ObservableQuery(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
            this.expression = Expression.Constant(this);

            var collection = enumerable as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += enumerable_CollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="expression"></param>
        public ObservableQuery(Expression expression)
        {
            this.expression = expression;
        }

        internal override IEnumerable Enumerable
        {
            get { return enumerable; }
        }

        internal override Expression Expression
        {
            get { return expression; }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// Ensures the structures associated with the query have been initialized.
        /// </summary>
        private void Ensure()
        {
            // skip for base query instances
            if (enumerable != null)
                return;

            // generate new handler
            if (operation == null)
            {
                operation = OperationFactory.FromExpression<IEnumerable<T>>(new OperationContext(), expression);
                operation.ValueChanged += operation_ValueChanged;
                operation.Load();
            }
        }

        /// <summary>
        /// Checks whether the structures associated with the query need to be initialized, and if not, disposes of them.
        /// </summary>
        private void Check()
        {
            if (enumerable != null)
                return;

            if (operation != null &&
                refCount == 0 && collectionChanged != null)
            {
                operation.ValueChanged -= operation_ValueChanged;
                operation.Dispose();
                operation = null;
            }
        }

        /// <summary>
        /// Invoked when the handler changes its output value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= operation_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += operation_CollectionChanged;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Invoked when the base enumerable's collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void enumerable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(args);
        }

        /// <summary>
        /// Invoked when the operation's collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(args);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the results of the query.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (enumerable != null)
            {
                foreach (var i in enumerable)
                    yield return i;
                yield break;
            }

            if (operation != null)
                try
                {
                    refCount++;
                    foreach (var i in operation.Value)
                        yield return i;
                }
                finally
                {
                    refCount--;
                    Check();
                }
        }

        private event NotifyCollectionChangedEventHandler collectionChanged;

        /// <summary>
        /// Raised when the items in the query change.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { Ensure(); collectionChanged += value; }
            remove { collectionChanged -= value; Check(); }
        }

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (collectionChanged != null)
                collectionChanged(this, args);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        Expression IQueryable.Expression
        {
            get { return Expression; }
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return ObservableQuery.Create(expression.Type, expression);
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type))
                throw new ArgumentException("expression");

            return new ObservableQuery<S>(expression);
        }

        /// <summary>
        /// Executes the expression by creating an operation graph, obtaining it's value, and disposing of it.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        object Execute(Expression expression)
        {
            using (var op = OperationFactory.FromExpression(new OperationContext(), expression))
            {
                op.Load();
                return op.Value;
            }
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)Execute(expression);
        }

        public ObservableValue<ObservableQuery<T>, TResult> Observe<TResult>(Expression<Func<ObservableQuery<T>, TResult>> scalarFunc)
        {
            return new ObservableValue<ObservableQuery<T>, TResult>(expression, scalarFunc);
        }

        public ObservableQueryView<T> ToView()
        {
            return new ObservableQueryView<T>(this);
        }

        public void Dispose()
        {
            refCount = 0;
            collectionChanged = null;
            Check();
        }

    }

}
