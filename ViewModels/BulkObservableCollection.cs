using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CollectionViewIssues.ViewModels;

/// <summary>
/// This one is a helper to be able to replace / add a lot of items to a ObservableCollection
/// without having the CollectionView "flashing" or moving all the time
/// For example if i like to replace / relaod the Items completly i need to make 
/// a .Clear and then add the items. On iOS this results in the CollectionView
/// "flash" (after the .Clear it removes all items imediatly) then it refreshes with every .add
/// 
/// You sure could set a NEW Item-Collection in Code like:
///    newItemsList = new ObservablyCollection(loadedData);
///    Items = newItemsList;
/// but then weird things are happening with the scrollposition being reset 
/// </summary>
/// <typeparam name="T"></typeparam>
public class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification = false;

    public BulkObservableCollection() : base()
    {

    }
    public BulkObservableCollection(IEnumerable<T> collection) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
    {

    }

    public void AddRange(IEnumerable<T> items, bool clearBeforeAdding = false)
    {
        _suppressNotification = true;
        try
        {
            if (clearBeforeAdding)
                Clear();

            if (items == null)
                return;

            foreach (var item in items)
                Add(item);
        }
        finally
        {
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }


    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
            base.OnCollectionChanged(e);
    }
}
