using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    public static class ObservableQueryable
    {

        static MethodInfo WithNullSafeMethodInfo = typeof(ObservableQueryable)
            .GetMethods()
            .Single(i => i.Name == "WithNullSafe");

        /// <summary>
        /// Creates an observable query based on the given enumerable, or if the enumerable is already an observable
        /// query, simply casts it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ObservableQuery<T> AsObservableQuery<T>(this IEnumerable<T> self)
        {
            if (self is ObservableQuery<T>)
                return (ObservableQuery<T>)self;
            else
                return new ObservableQuery<T>(self);
        }

        /// <summary>
        /// Enables or disables null protection for the given <see cref="ObservableQuery"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ObservableQuery<T> WithNullSafe<T>(this ObservableQuery<T> self, bool enable)
        {
            Contract.Requires<ArgumentNullException>(self != null);

            return (ObservableQuery<T>)self.Provider.CreateQuery<T>(
                Expression.Call(
                    WithNullSafeMethodInfo.MakeGenericMethod(typeof(T)),
                    Expression.Convert(self.Expression, typeof(ObservableQuery<T>)),
                    Expression.Constant(enable, typeof(bool))));
        }

    }

}
