using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arzenal.StoreManager.WPF.Views.AppInfo.Controls
{
    /// <summary>
    /// Logique d'interaction pour GenericCollectionField.xaml
    /// </summary>
    public partial class GenericCollectionField : UserControl
    {
        public GenericCollectionField()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(GenericCollectionField), new PropertyMetadata(null));

        public IEnumerable Items
        {
            get => (IEnumerable)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IEnumerable), typeof(GenericCollectionField), new PropertyMetadata(null));

        public IEnumerable SelectedItems
        {
            get => (IEnumerable)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty ChosenItemProperty =
            DependencyProperty.Register(nameof(ChosenItem), typeof(object), typeof(GenericCollectionField), new PropertyMetadata(null));

        public object ChosenItem
        {
            get => GetValue(ChosenItemProperty);
            set => SetValue(ChosenItemProperty, value);
        }

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(GenericCollectionField), new PropertyMetadata(null));

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(GenericCollectionField), new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public static readonly DependencyProperty SectionTitleProperty =
            DependencyProperty.Register(nameof(SectionTitle), typeof(string), typeof(GenericCollectionField), null);

        public string SectionTitle
        {
            get => (string)GetValue(SectionTitleProperty);
            set => SetValue(SectionTitleProperty, value);
        }


    }

}
