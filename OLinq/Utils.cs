using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public static class Utils
    {

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

        public static Expression<Func<T, TResult>> UnpackLambda<T, TResult>(Expression e)
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

    }

}
