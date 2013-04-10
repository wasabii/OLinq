OLinq is a project to provide a Linq Queryable provider implementation for operating on ObservableCollections, or other INotifyCollectionChanged supporting lists. The output of OLinq is an ObservableView which notifies when the results of the query have changed.

```csharp
    var collection = new ObservableCollection<int>(){ 1, 2, 3 }; //an object implementing INotifyCollectionChanged
	var observableView = someQueryable.AsObservableQuery().Where(i => i > 2).ToObservableView(); //an ObservableView raises events when underlying source collections used in the linq statement are changed
	var observableBuffer = observableView.ToBuffer(); //an ObservableBuffer caches the results of the Linq query, so it is not re-evaluated, but instead the results are updated incrementally as the view raises events      
```


###Linq operator support###

The following Linq operators are both supported AND documented. There may be more that are supported, but aren't listed here :)

| Operator        | Supported| Preserves positioning  | Notes |
| -------------   ||:-------------:      ||
|                 | |                    ||
| Concat          | Yes|Yes                 ||
| Intersect       | Yes|No                  ||

####Notes on positioning####

The NewStartingIndex and OldStartingIndex values on the raised (NotifyCollectionChangedEventArgs)[http://msdn.microsoft.com/en-us/library/system.collections.specialized.notifycollectionchangedeventargs.aspx] are 
only supported by some of the Linq operators. If you use operators that don't support it, then you might find that the items in the buffer are not in the order you expect.



