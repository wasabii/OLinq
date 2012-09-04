using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class AnyOperation : GroupOperation<bool>
    {

        LambdaExpression predicateExpr;
        Dictionary<object, LambdaOperation<bool>> predicates = new Dictionary<object, LambdaOperation<bool>>();

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            if (expression.Arguments.Count >= 2)
            {
                predicateExpr = expression.Arguments[1] as LambdaExpression;
                if (predicateExpr == null)
                {
                    var unaryExpr = expression.Arguments[1] as UnaryExpression;
                    if (unaryExpr != null)
                        predicateExpr = unaryExpr.Operand as LambdaExpression;
                }

                if (predicateExpr == null)
                    throw new InvalidOperationException("Could not load predicate.");
            }
        }

        protected override void OnSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Reset(Source);
        }

        protected override void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            Reset(Source);
        }

        bool Reset(IEnumerable source)
        {
            return SetValue(source.Cast<object>().Any(i => GetPredicateValue(i)));
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
                    // any value becoming true means 'any' is true
                    SetValue(true);
                else
                    Reset(Source);
        }

        private LambdaOperation<bool> GetPredicate(object item)
        {
            if (predicateExpr == null)
                return null;

            LambdaOperation<bool> predicate;
            if (!predicates.TryGetValue(item, out predicate))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<object>(item);
                var.Load();
                ctx.Variables[predicateExpr.Parameters[0].Name] = var;

                // create new test and subscribe to test modifications
                predicate = new LambdaOperation<bool>(ctx, predicateExpr);
                predicate.Load(); // load before value changed to prevent double notification
                predicate.ValueChanged += predicate_ValueChanged;
                predicates[item] = predicate;
            }

            return predicate;
        }

        private bool GetPredicateValue(object item)
        {
            if (predicateExpr != null)
                return GetPredicate(item).Value;
            else
                return true;
        }

        public override void Load()
        {
            base.Load();
            Reset(Source);
        }

    }

}
