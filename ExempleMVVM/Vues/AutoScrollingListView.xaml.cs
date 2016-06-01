using System.Collections.Specialized;
using System.Windows.Controls;

namespace TP2.Vues
{
    public partial class AutoScrollingListView : ListView
    {
        #region Protected Methods

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (oldValue as INotifyCollectionChanged != null)
                (oldValue as INotifyCollectionChanged).CollectionChanged -= ItemsCollectionChanged;

            if (newValue as INotifyCollectionChanged == null) return;

            (newValue as INotifyCollectionChanged).CollectionChanged += ItemsCollectionChanged;
        }

        #endregion Protected Methods

        #region Private Methods

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Items.Count > 0)
                ScrollIntoView(Items[Items.Count - 1]);
        }

        #endregion Private Methods
    }
}