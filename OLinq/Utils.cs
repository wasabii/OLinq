using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public static class Utils
    {

        public static LambdaExpression CreateConstantLambdaExpression(object value, Type type)
        {
            return Expression.Lambda(
                Expression.Constant(value, type),
                Expression.Parameter(type, "p")); ;
        }

        public static LambdaExpression CreateSelfLambdaExpression(Type type)
        {
            return Expression.Lambda(
                Expression.Parameter(type, "p"),
                Expression.Parameter(type, "p"));
        }

        public static IEnumerable<T> AsEnumerable<T>(object o)
        {
            var e1 = o as IEnumerable<T>;
            if (e1 != null)
                return e1;

            var e2 = o as IEnumerable;
            if (e2 != null)
                return e2.Cast<T>();

            if (o == null)
                return Enumerable.Empty<T>();

            throw new ArgumentOutOfRangeException();
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : default(TValue);
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> func)
        {
            var v = self.ValueOrDefault(key);
            if (v == null)
                self[key] = v = func(key);
            return v;
        }

        /// <summary>
        /// Unpacks an expression into a <see cref="LambdaExpression" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Expression<Func<T, TResult>> UnpackLambda<T, TResult>(this Expression e)
        {
            var expr = e as Expression<Func<T, TResult>>;
            if (expr == null)
            {
                var unaryExpr = e as UnaryExpression;
                if (unaryExpr != null)
                    expr = unaryExpr.Operand as Expression<Func<T, TResult>>;
            }

            return expr;
        }

        /// <summary>
        /// Unpacks an <see cref="Expression"/> into a <see cref="LambdaExpression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static LambdaExpression UnpackLambda(this Expression expression)
        {
            var expr = expression as LambdaExpression;
            if (expr == null)
            {
                var unaryExpr = expression as UnaryExpression;
                if (unaryExpr != null)
                    expr = unaryExpr.Operand as LambdaExpression;
            }

            return expr;
        }

        /// <summary>
        /// Gets the method expression argument at the given index, or the default value if not available.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="index"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Expression GetArgument(this MethodCallExpression expression, int index)
        {
            return expression.Arguments.Count > index ? expression.Arguments[index] : null;
        }

        /// <summary>
        /// Gets the lambda expression given by the first argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Expression<Func<T, TResult>> GetLambdaArgument<T, TResult>(this MethodCallExpression expression, int index)
        {
            return expression.GetArgument(index).UnpackLambda<T, TResult>();
        }

        /// <summary>
        /// Gets the lambda expression given by the first argument.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static LambdaExpression GetLambdaArgument(this MethodCallExpression expression, int index)
        {
            return expression.GetArgument(index).UnpackLambda();
        }

    }

}
