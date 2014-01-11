using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public static class Utils
    {

        /// <summary>
        /// Casts the specified object as an enumerable of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the value for the specified key, or the default if not found.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : default(TValue);
        }

        /// <summary>
        /// Returns the existing value for the specififed key, or creates one using the specified function.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> func)
        {
            bool created;
            return GetOrCreate<TKey, TValue>(self, key, func, out created);
        }

        /// <summary>
        /// Returns the existing value for the specififed key, or creates one using the specified function.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> func, out bool created)
        {
            TValue value;
            if (created = !self.TryGetValue(key, out value))
                value = self[key] = func(key);
            return value;
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
