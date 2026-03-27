using System.Collections.ObjectModel;

namespace Arzenal.StoreManager.ViewModels.Helpers
{
    public static class CollectionHelper
    {
        public static void AddItem<T>(T item, ObservableCollection<T> collection)
        {
            if (item != null && !collection.Contains(item))
                collection.Add(item);
        }

        public static bool CanAddItem<T>(T item, ObservableCollection<T> collection)
        {
            return item != null && !collection.Contains(item);
        }

        public static void RemoveItem<T>(T item, ObservableCollection<T> collection)
        {
            if (item != null && collection.Contains(item))
                collection.Remove(item);
        }
    }
}
