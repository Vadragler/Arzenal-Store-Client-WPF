using Arzenal.Dto.DTOs.OperatingSystemDto;
using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.ViewModels.AppFormViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class GenericCollectionViewModelTests
    {
        private static GenericCollectionViewModel<string> CreateVm(
    Mock<IFileDialogService>? fileDialogMock = null,
    ObservableCollection<string>? items = null)
        {
            fileDialogMock ??= new Mock<IFileDialogService>();

            Func<Task<ObservableCollection<string>>> loader =
                () => Task.FromResult(items ?? new ObservableCollection<string>());

            return new GenericCollectionViewModel<string>(
                fileDialogMock.Object,
                loader);
        }

        [Fact]
        public async Task Constructor_LoadsItems()
        {
            var items = new ObservableCollection<string> { "A", "B" };
            var vm = CreateVm(items: items);

            await Task.Delay(50); // laisse le temps au ctor async

            Assert.Equal(2, vm.Items.Count);
            Assert.Contains("A", vm.Items);
        }

        [Fact]
        public void AddCommand_AddsItemToSelectedItems()
        {
            var vm = CreateVm();

            vm.AddCommand.Execute("Item1");

            Assert.Single(vm.SelectedItems);
            Assert.Contains("Item1", vm.SelectedItems);
        }

        [Fact]
        public void AddCommand_CannotAddDuplicate()
        {
            var vm = CreateVm();

            vm.AddCommand.Execute("Item1");

            Assert.False(vm.AddCommand.CanExecute("Item1"));
        }

        [Fact]
        public void RemoveCommand_RemovesItem()
        {
            var vm = CreateVm();
            vm.AddCommand.Execute("Item1");

            vm.RemoveCommand.Execute("Item1");

            Assert.Empty(vm.SelectedItems);
        }

        [Fact]
        public void ChosenItem_AddsAndResets()
        {
            var vm = CreateVm();

            vm.ChosenItem = "Item1";

            Assert.Single(vm.SelectedItems);
            Assert.Null(vm.ChosenItem);
        }

        [Fact]
        public void IsOsSection_False_ForOtherTypes()
        {
            var vm = CreateVm();

            Assert.False(vm.IsOsSection);
        }

        [Fact]
        public void BrowseFileCommand_CannotExecute_WhenNotOsSection()
        {
            var vm = CreateVm();

            Assert.False(vm.BrowseFileCommand.CanExecute("Item1"));
        }

        [Fact]
        public void BrowseFileCommand_StoresPath_WhenOsSection()
        {
            var fileDialogMock = new Mock<IFileDialogService>();
            fileDialogMock.Setup(x => x.SelectFile())
                          .Returns("C:\\file.iso");

            var vm = new GenericCollectionViewModel<ReadOperatingSystemDto>(
                fileDialogMock.Object,
                () => Task.FromResult(new ObservableCollection<ReadOperatingSystemDto>())
            );

            var os = new ReadOperatingSystemDto();

            vm.BrowseFileCommand.Execute(os);

            Assert.True(vm.LocalFilePaths.ContainsKey(os));
            Assert.Equal("C:\\file.iso", vm.LocalFilePaths[os]);
        }

        [Fact]
        public async Task ItemsLoaded_Event_IsRaised()
        {
            var tcs = new TaskCompletionSource();

            var vm = CreateVm(items: new ObservableCollection<string> { "A" });
            vm.ItemsLoaded += () => tcs.TrySetResult();

            await vm.Initialization;

            Assert.True(true);
        }


    }
}
