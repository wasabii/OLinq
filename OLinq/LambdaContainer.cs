using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Provides a lambda container. This maps a set of objects to lambda expressions.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    class LambdaContainer<TSource, TResult> :
        IEnumerable<LambdaOperation<TResult>>,
        INotifyPropertyChanging,
        INotifyPropertyChanged,
        INotifyCollectionChanged,
        IDisposable
    {

        IEnumerable<TSource> source;
        Dictionary<TSource, LambdaOperation<TResult>> lambdas =
            new Dictionary<TSource, LambdaOperation<TResult>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="createContextAction"></param>
        public LambdaContainer(Expression<Func<TSource, TResult>> expression, Func<TSource, ParameterExpression[], OperationContext> createContextAction)
        {
            Expression = expression;
            CreateContextAction = createContextAction;

            PropertyChanging += this_PropertyChanging;
            PropertyChanged += this_PropertyChanged;
        }

        /// <summary>
        /// Gets the expression used to generate new lambda operations.
        /// </summary>
        public LambdaExpression Expression { get; private set; }

        /// <summary>
        /// Gets the action that will generate a new context for lambda operations.
        /// </summary>
        public Func<TSource, ParameterExpression[], OperationContext> CreateContextAction { get; private set; }

        /// <summary>
        /// Gets the Lambda operation for the given source item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public LambdaOperation<TResult> this[TSource item]
        {
            get { return GetOrCreateLambda(item); }
        }

        /// <summary>
        /// Gets the original source for the given lambda.
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public TSource this[LambdaOperation<TResult> lambda]
        {
            get { return (TSource)lambda.Tag; }
        }

        /// <summary>
        /// Gets or sets the set of items for which to generate lambda operations.
        /// </summary>
        public IEnumerable<TSource> Source
        {
            get { return source; }
            set { RaisePropertyChanging("Source"); source = value; RaisePropertyChanged("Source"); }
        }

        void this_PropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "Source":
                    var o = source as INotifyCollectionChanged;
                    if (o != null)
                        o.CollectionChanged -= items_CollectionChanged;
                    break;
            }
        }

        void this_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "Source":
                    var o = source as INotifyCollectionChanged;
                    if (o != null)
                        o.CollectionChanged += items_CollectionChanged;

                    // reinitialize from new collection
                    Reset();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    var newLambdas = args.NewItems.Cast<TSource>().Select(i => GetOrCreateLambda(i)).ToList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newLambdas, args.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldLambdas = args.OldItems.Cast<TSource>().Select(i => GetOrCreateLambda(i)).ToList();
                    ReleaseLambdaOperations(oldLambdas);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldLambdas, args.OldStartingIndex));
                    break;
            }
        }

        /// <summary>
        /// Invoked to reset the collection.
        /// </summary>
        void Reset()
        {
            // release all missing lambdas
            ReleaseLambdaOperations(lambdas.Values.Except((Source ?? Enumerable.Empty<TSource>()).Select(i => GetLambda(i))).ToList());

            // ensure new lambdas
            if (Source != null)
                foreach (var lambda in Source)
                    GetOrCreateLambda(lambda);
        }

        /// <summary>
        /// Gets the lambda operation for the given item or creates a new one.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        LambdaOperation<TResult> GetOrCreateLambda(TSource item)
        {
            return lambdas.GetOrCreate(item, i => CreateLambdaOperation(i));
        }

        /// <summary>
        /// Gets the lambda operation for the given operation.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        LambdaOperation<TResult> GetLambda(TSource item)
        {
            return lambdas.GetOrDefault(item);
        }

        /// <summary>
        /// Creates a new lambda operation.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        LambdaOperation<TResult> CreateLambdaOperation(TSource item)
        {
            // generate new context
            var ctx = CreateContextAction(item, Expression.Parameters.ToArray());
            if (ctx == null)
                throw new InvalidOperationException("Could not generate context for new lambda operation.");

            // create new test and subscribe to test modifications
            var lambda = new LambdaOperation<TResult>(ctx, Expression);
            lambda.Tag = item;
            lambda.ValueChanged += lambda_ValueChanged;
            lambdas[item] = lambda;

            return lambda;
        }

        /// <summary>
        /// Disposes of the given lambda operations.
        /// </summary>
        /// <param name="lambdas"></param>
        void ReleaseLambdaOperations(IEnumerable<LambdaOperation<TResult>> lambdas)
        {
            foreach (var lambda in lambdas)
                ReleaseLambdaOperation(lambda);
        }

        /// <summary>
        /// Disposes of the given lambda operation.
        /// </summary>
        /// <param name="lambda"></param>
        void ReleaseLambdaOperation(LambdaOperation<TResult> lambda)
        {
            // remove from selectors
            lambdas.Remove((TSource)lambda.Tag);

            // dispose of selector and variables
            lambda.ValueChanged -= lambda_ValueChanged;
            lambda.Dispose();
            foreach (var var in lambda.Context.Variables)
                var.Value.Dispose();
        }

        /// <summary>
        /// Invoked when the value of a lambda operation is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void lambda_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            // retrieve information relating to changed lambda
            var lambda = (LambdaOperation<TResult>)sender;
            var item = (TSource)lambda.Tag;

            // raise the lambda notification
            RaiseValueChanged(item, lambda, (TResult)args.OldValue, (TResult)args.NewValue);
        }

        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Raises the PropertyChanging event.
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raised when the value of one of the maintained lambda operations is changed.
        /// </summary>
        public event LambdaValueChangedEventHandler<TSource, TResult> ValueChanged;

        /// <summary>
        /// Raises the LambdaValuesChanged event.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="operation"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void RaiseValueChanged(TSource item, LambdaOperation<TResult> operation, TResult oldValue, TResult newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new LambdaValueChangedEventArgs<TSource, TResult>(item, operation, oldValue, newValue));
        }

        public IEnumerator<LambdaOperation<TResult>> GetEnumerator()
        {
            return Source != null ? Source.Select(i => GetOrCreateLambda(i)).GetEnumerator() : Enumerable.Empty<LambdaOperation<TResult>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when the contents of the collection is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        public void Dispose()
        {
            // removes items, which results in removal and disposal of predicates
            Source = null;
        }

    }

}
