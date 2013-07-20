using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    class MemberAccessOperation<T> : Operation<T>
    {

        MemberExpression self;
        IOperation targetOp;

        public MemberAccessOperation(OperationContext context, MemberExpression expression)
            : base(context, expression)
        {
            self = expression;

            if (self.Expression != null)
            {
                targetOp = OperationFactory.FromExpression(context, self.Expression);
                targetOp.ValueChanged += target_ValueChanged;
                OnTargetValueChanged(null, targetOp.Value);
            }
            else if (!IsStatic(self.Member))
                throw new ArgumentException("Cannot access a non-static method without a target expression.");
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
            // obtain target if possible
            var target = targetOp != null ? targetOp.Value : null;

            // if target is null, but it should be a value type, generate default instance of value type
            if (target == null &&
                targetOp != null &&
                self.Expression.Type.IsValueType)
                target = Activator.CreateInstance(self.Expression.Type);

            var member = self.Member;
            if (member is PropertyInfo)
                SetValue(GetValue((PropertyInfo)member, target));
            else if (self.Member is FieldInfo)
                SetValue(GetValue((FieldInfo)member, target));
            else
                throw new NotSupportedException(string.Format("MemberAccess does not support Member of type {0}.", member.MemberType));
        }

        T GetValue(FieldInfo member, object target)
        {
            return (T)member.GetValue(target);
        }

        T GetValue(PropertyInfo member, object target)
        {
            return (T)member.GetValue(target, null);
        }

        bool IsStatic(MemberInfo member)
        {
            if (member is FieldInfo)
                return ((FieldInfo)member).IsStatic;
            else if (member is PropertyInfo)
                return (((PropertyInfo)member)).GetGetMethod().IsStatic;
            else
                throw new ArgumentException("Member is not of a known type.");
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
