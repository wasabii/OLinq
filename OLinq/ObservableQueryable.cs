using System.Collections.Generic;

namespace OLinq
{

    public static class ObservableQueryable
    {

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

    }

}
