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

    public class ObservableQuery<T> : ObservableQuery, IQueryable<T>, IQueryProvider, INotifyCollectionChanged
    {

        private IEnumerable<T> enumerable;
        private Expression expression;
        private IOperation<IEnumerable<T>> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="enumerable"></param>
        public ObservableQuery(IEnumerable<T> enumerable)
        {
            SetEnumerable(enumerable);
            this.expression = Expression.Constant(this);
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
        private void EnsureQuery()
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
        /// Invoked when the handler changes its output value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (operation.Value is IEnumerable<T>)
                SetEnumerable(operation.Value as IEnumerable<T>);
        }

        /// <summary>
        /// Invoked when the handler's value changes.
        /// </summary>
        private void SetEnumerable(IEnumerable<T> newEnumerable)
        {
            // unsubscribe from collection changed
            var observable = enumerable as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged -= observable_CollectionChanged;

            enumerable = newEnumerable;

            // subscribe to collection changed
            observable = enumerable as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += observable_CollectionChanged;

            // collection is reset
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Invoked when the handler's collection value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void observable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(args);
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureQuery();

            return enumerable.GetEnumerator();
        }

        private event NotifyCollectionChangedEventHandler collectionChanged;

        /// <summary>
        /// Raised when the items in the query change.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { EnsureQuery(); collectionChanged += value; }
            remove { EnsureQuery(); collectionChanged -= value; }
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

        object Execute(Expression expression)
        {
            var op = OperationFactory.FromExpression(new OperationContext(), expression);
            op.Load();
            return op.Value;
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

        public ObservableQuery<T> ToCollection()
        {
            return new ObservableQuery<T>(this);
        }

    }

}
