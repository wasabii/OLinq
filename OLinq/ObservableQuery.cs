using System;
using System.Collections;
using System.Collections.Generic;
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

        public abstract Expression Expression { get; }

        public abstract Type ElementType { get; }

        public abstract IQueryProvider Provider { get; }

    }

    public class ObservableQuery<T> : ObservableQuery, IQueryable<T>, IQueryProvider
    {

        /// <summary>
        /// Used to transform an expression tree referencing an <see cref="ObservableQuery"/> to one wrapping an
        /// <see cref="EnumerableQuery"/>.
        /// </summary>
        private class EnumerableTransformVisitor : ExpressionVisitor
        {

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var value = node.Value as ObservableQuery;
                if (value != null)
                {
                    // extract item type, generate new query type
                    var itemType = value.GetType().GetGenericArguments()[0];
                    var type = typeof(EnumerableQuery<>).MakeGenericType(itemType);

                    // generate new enumerable query wrapper for existing value
                    var query = Activator.CreateInstance(type, value.Enumerable);

                    // transform constant
                    return Expression.Constant(query, type);
                }

                return base.VisitConstant(node);
            }

        }

        private IEnumerable<T> enumerable;
        private Expression expression;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="enumerable"></param>
        public ObservableQuery(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
            this.expression = Expression.Constant(this, typeof(IQueryable<T>));
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

        public override Expression Expression
        {
            get { return expression; }
        }

        public override Type ElementType
        {
            get { return typeof(T); }
        }

        public override IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// IQueryable Enumerator implementation. This should preferably not be used. The query should first be
        /// transformed into a View.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            // transform the query into an enumerable query
            var expr = new EnumerableTransformVisitor().Visit(expression);
            var query = new EnumerableQuery<T>(expr);
            return ((IEnumerable<T>)query).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            // transform the query into an enumerable query
            var expr = new EnumerableTransformVisitor().Visit(expression);
            var query = new EnumerableQuery<T>(expr);
            return ((IQueryProvider)query).Execute(expr);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)Execute(expression);
        }

        /// <summary>
        /// Obtains an <see cref="ObservableValue`1"/> for a scalar query result, from which you can watch for change
        /// notifications of the scalar value.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="scalarFunc"></param>
        /// <returns></returns>
        public ObservableValue<IEnumerable<T>, TResult> Observe<TResult>(Expression<Func<IQueryable<T>, TResult>> scalarFunc)
        {
            return new ObservableValue<IEnumerable<T>, TResult>(expression, scalarFunc);
        }

        /// <summary>
        /// Obtains an <see cref="ObservableView`1"/> for this query, from which you can watch for change notifications
        /// in the query results.
        /// </summary>
        /// <returns></returns>
        public ObservableView<T> ToObservableView()
        {
            return new ObservableView<T>(this);
        }

    }

}
