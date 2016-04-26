using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Base class for an operation that operates against a source collection, maintains a func for each source
    /// element, and returns a enumerable of result elements.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TFuncResult"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    abstract class EnumerableSourceWithFuncOperation<TSource, TFuncResult, TResult> :
        EnumerableSourceOperation<TSource, TResult>
    {

        readonly FuncContainer<TSource, TFuncResult> funcs;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <param name="sourceExpression"></param>
        /// <param name="funcExpression"></param>
        public EnumerableSourceWithFuncOperation(OperationContext context, MethodCallExpression expression, Expression sourceExpression, Expression<Func<TSource, TFuncResult>> funcExpression)
            : base(context, expression, sourceExpression)
        {
            // generate lambda collection
            funcs = new FuncContainer<TSource, TFuncResult>(funcExpression, CreateFuncContext);
            funcs.CollectionChanged += funcs_CollectionChanged;
            funcs.ValueChanged += funcs_ValueChanged;
            funcs.Source = Source;
        }

        /// <summary>
        /// Gets the func collection which wraps the source.
        /// </summary>
        protected FuncContainer<TSource, TFuncResult> Funcs
        {
            get { return funcs; }
        }

        /// <summary>
        /// Creates a context for a new func expression.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        OperationContext CreateFuncContext(TSource item, params ParameterExpression[] parameters)
        {
            // generate new parameter
            var ctx = new OperationContext(Context);
            var var = OperationFactory.FromValue(item);
            ctx.Variables[parameters[0].Name] = var;
            return ctx;
        }

        /// <summary>
        /// Invoked when the source collection has been reset.
        /// </summary>
        protected override void OnSourceCollectionReset()
        {
            if (funcs != null)
                funcs.Source = Source;
        }

        /// <summary>
        /// Invoked when then lambda collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void funcs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnLambdaCollectionChanged(args);

            switch (args.Action)
            {
#if !SILVERLIGHT && !PCL
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                    OnLambdaCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnLambdaCollectionItemsAdded(Utils.AsEnumerable<FuncOperation<TFuncResult>>(args.NewItems), args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnLambdaCollectionItemsRemoved(Utils.AsEnumerable<FuncOperation<TFuncResult>>(args.OldItems), args.OldStartingIndex);
                    break;
            }
        }

        /// <summary>
        /// Invoked when then lambda collection is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {

        }

        /// <summary>
        /// Invoked when one of the lambda's value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void funcs_ValueChanged(object sender, FuncValueChangedEventArgs<TSource, TFuncResult> args)
        {
            OnLambdaValueChanged(args);
        }

        /// <summary>
        /// Invoked when the lambda collection is reset.
        /// </summary>
        protected virtual void OnLambdaCollectionReset()
        {

        }

        /// <summary>
        /// Invoked when items are added to the lambda collection.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnLambdaCollectionItemsAdded(IEnumerable<FuncOperation<TFuncResult>> newItems, int startingIndex)
        {

        }

        /// <summary>
        /// Invoked when items are removed from the lambda collection.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnLambdaCollectionItemsRemoved(IEnumerable<FuncOperation<TFuncResult>> oldItems, int startingIndex)
        {

        }

        /// <summary>
        /// Invoked when one of the lambda's value is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLambdaValueChanged(FuncValueChangedEventArgs<TSource, TFuncResult> args)
        {

        }

    }

}
