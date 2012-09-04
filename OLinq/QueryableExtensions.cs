using System.Collections.Generic;

namespace OLinq
{

    public static class QueryableExtensions
    {

        public static ObservableQuery<T> AsObservableQuery<T>(this IEnumerable<T> self)
        {
            if (self is ObservableQuery<T>)
                return (ObservableQuery<T>)self;
            else
                return new ObservableQuery<T>(self);
        }

    }

}
