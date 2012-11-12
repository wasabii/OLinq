using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    class MemberAccessOperation<T> : Operation<T>
    {

        IOperation targetOp;

        public MemberAccessOperation(OperationContext context, MemberExpression expression)
            : base(context, expression)
        {
            if (expression.Expression != null)
            {
                targetOp = OperationFactory.FromExpression(context, expression.Expression);
                targetOp.Init();
                targetOp.ValueChanged += target_ValueChanged;
                OnTargetValueChanged(null, targetOp.Value);
            }
        }

        /// <summary>
        /// Invoked when the result of the Object operation is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void target_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnTargetValueChanged(args.OldValue, args.NewValue);
        }

        /// <summary>
        /// Invoked when the result of the target operation is changed.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnTargetValueChanged(object oldValue, object newValue)
        {
            UnsubscribeTargetValue(oldValue);
            SubscribeTargetValue(newValue);
            ResetValue();
        }

        /// <summary>
        /// Subscribes to notifications on the target object.
        /// </summary>
        /// <param name="target"></param>
        void SubscribeTargetValue(object target)
        {
            var newValue = target as INotifyPropertyChanged;
            if (newValue != null)
                newValue.PropertyChanged += target_PropertyChanged;
        }

        /// <summary>
        /// Unsubscribes from notifications on the target object.
        /// </summary>
        /// <param name="target"></param>
        void UnsubscribeTargetValue(object target)
        {
            var oldValue = target as INotifyPropertyChanged;
            if (oldValue != null)
                oldValue.PropertyChanged -= target_PropertyChanged;
        }

        /// <summary>
        /// Invoked when a watched property is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void target_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ((MemberExpression)Expression).Member.Name)
                ResetValue();
        }

        /// <summary>
        /// Sets the value from the referenced member.
        /// </summary>
        void ResetValue()
        {
            var expression = (MemberExpression)Expression;
            if (expression.Member is PropertyInfo)
                SetValue((T)((PropertyInfo)expression.Member).GetValue(targetOp.Value, null));
            else if (expression.Member is FieldInfo)
                SetValue((T)((FieldInfo)expression.Member).GetValue(targetOp.Value));
            else
                throw new NotSupportedException(string.Format("MemberAccess does not support Member of type {0}.", expression.Member.MemberType));
        }

        public override void Dispose()
        {
            if (targetOp != null)
            {
                UnsubscribeTargetValue(targetOp.Value);
                targetOp.ValueChanged -= target_ValueChanged;
                targetOp.Dispose();
            }


            base.Dispose();
        }

    }

}
