using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class AnyOperation<TSource> : GroupOperation<TSource, bool>
    {

        LambdaExpression predicateExpr;
        Dictionary<object, LambdaOperation<bool>> predicates = new Dictionary<object, LambdaOperation<bool>>();

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        LambdaExpression GetPredicateExpr()
        {
            if (predicateExpr != null)
                return predicateExpr;

            var expr = (MethodCallExpression)Expression;
            if (expr.Arguments.Count >= 2)
            {
                predicateExpr = Utils.UnpackLambda(expr.Arguments[1]);
                if (predicateExpr == null)
                    throw new InvalidOperationException("Could not load predicate.");
            }

            return predicateExpr;
        }

        protected override void SourceCollectionReset()
        {
            base.SourceCollectionReset();
            ResetValue();
        }

        protected override void SourceCollectionAddItem(TSource item, int index)
        {
            base.SourceCollectionAddItem(item, index);

            // we've added a true item, we must be true
            if (GetItemValue(item))
                SetValue(true);
        }

        protected override void SourceCollectionRemoveItem(TSource item, int index)
        {
            base.SourceCollectionRemoveItem(item, index);

            ResetValue();
        }

        void ResetValue()
        {
            SetValue(SourceCollection.Any(i => GetItemValue(i)));
        }

        /// <summary>
        /// Invoked when any of the current tests change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void predicate_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = (bool)args.OldValue;
            var newValue = (bool)args.NewValue;

            if (oldValue != newValue)
                if (newValue == true)
                    // new value is true means 'any' is true
                    SetValue(true);
                else if (Value == true)
                    // new value is false, existing value is true, and so are we
                    SetValue(true);
                else
                    // undetermined, rebuild
                    ResetValue();
        }

        private LambdaOperation<bool> GetPredicate(object item)
        {
            if (GetPredicateExpr() == null)
                return null;

            LambdaOperation<bool> predicate;
            if (!predicates.TryGetValue(item, out predicate))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = OperationFactory.FromValue(item);
                ctx.Variables[GetPredicateExpr().Parameters[0].Name] = var;

                // create new test and subscribe to test modifications
                predicate = new LambdaOperation<bool>(ctx, GetPredicateExpr());
                predicate.Init(); // load before value changed to prevent double notification
                predicate.ValueChanged += predicate_ValueChanged;
                predicates[item] = predicate;
            }

            return predicate;
        }

        /// <summary>
        /// Gets whether or not the item is true.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool GetItemValue(object item)
        {
            var p = GetPredicate(item);
            return p != null ? p.Value : true;
        }

        public override void Dispose()
        {
            foreach (var predicate in predicates.Values)
            {
                predicate.ValueChanged -= predicate_ValueChanged;
                predicate.Dispose();
                foreach (var var in predicate.Context.Variables)
                    var.Value.Dispose();
            }
            predicates = null;

            base.Dispose();
        }

    }

}
