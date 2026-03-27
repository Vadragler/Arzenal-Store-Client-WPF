using Arzenal.StoreManager.ViewModels.Helpers;
using System.Collections.ObjectModel;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class CollectionHelperTests
    {
        [Fact]
        public void AddItem_AddsItem_WhenNotPresent()
        {
            var collection = new ObservableCollection<string>();

            CollectionHelper.AddItem("A", collection);

            Assert.Single(collection);
            Assert.Contains("A", collection);
        }

        [Fact]
        public void AddItem_DoesNotAddDuplicate()
        {
            var collection = new ObservableCollection<string> { "A" };

            CollectionHelper.AddItem("A", collection);

            Assert.Single(collection);
        }

        [Fact]
        public void AddItem_DoesNothing_WhenItemIsNull()
        {
            var collection = new ObservableCollection<string>();

            CollectionHelper.AddItem(null, collection);

            Assert.Empty(collection);
        }

        [Fact]
        public void CanAddItem_ReturnsTrue_WhenItemNotInCollection()
        {
            var collection = new ObservableCollection<string>();

            var result = CollectionHelper.CanAddItem("A", collection);

            Assert.True(result);
        }

        [Fact]
        public void CanAddItem_ReturnsFalse_WhenItemAlreadyInCollection()
        {
            var collection = new ObservableCollection<string> { "A" };

            var result = CollectionHelper.CanAddItem("A", collection);

            Assert.False(result);
        }

        [Fact]
        public void CanAddItem_ReturnsFalse_WhenItemIsNull()
        {
            var collection = new ObservableCollection<string>();

            var result = CollectionHelper.CanAddItem(null, collection);

            Assert.False(result);
        }


        [Fact]
        public void RemoveItem_RemovesItem_WhenPresent()
        {
            var collection = new ObservableCollection<string> { "A" };

            CollectionHelper.RemoveItem("A", collection);

            Assert.Empty(collection);
        }

        [Fact]
        public void RemoveItem_DoesNothing_WhenItemNotPresent()
        {
            var collection = new ObservableCollection<string> { "A" };

            CollectionHelper.RemoveItem("B", collection);

            Assert.Single(collection);
        }

        [Fact]
        public void RemoveItem_DoesNothing_WhenItemIsNull()
        {
            var collection = new ObservableCollection<string> { "A" };

            CollectionHelper.RemoveItem(null, collection);

            Assert.Single(collection);
        }

        private class Dummy { }

        [Fact]
        public void Works_With_CustomObjects()
        {
            var obj = new Dummy();
            var collection = new ObservableCollection<Dummy>();

            CollectionHelper.AddItem(obj, collection);

            Assert.Contains(obj, collection);
        }

    }
}
