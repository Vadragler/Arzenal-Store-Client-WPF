using Magasin_PC;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.IO;

namespace Magasin_PC
{
    /// <summary>
    /// Logique d'interaction pour AppInfoWindow.xaml
    /// </summary>
    public partial class AppInfoWindow : Window
    {
        public ObservableCollection<string> AvailableTags { get; set; }
        public ObservableCollection<string> AvailableCategories { get; set; }
        public Guid id { get; private set; }
        public string name { get; private set; }
        public string description { get; private set; }
        public string version { get; private set; }
        public bool isvisible { get; private set; }
        public string icone { get; private set; }
        public string filepath { get; private set; }
        public ObservableCollection<string> tags { get; set; }
        public ObservableCollection<string> platforms { get; set; }
        public ObservableCollection<string> languages { get; set; }
        public string requirements { get; private set; }
        public string category { get; private set; }
        public DateTime releasedate { get; private set; }
        public DateTime? lastupdated { get; private set; }
        public long appsize { get; private set; }



        public AppInfoWindow(Guid appId,
                             string initname="",
                             string? initdesc="",
                             string initvers="",
                             bool initisvisible=false,
                             string initfilepath="",
                             string? initicone="",
                             ObservableCollection<string>? inittags = null,
                             ObservableCollection<string>? initplatforms = null,
                             ObservableCollection<string>? initlanguages = null,
                             string? initrequirements = null,
                             string? initcategory = null,
                             DateTime initreleaseDate = default,
                             DateTime? initlastUpdated = null,
                             long initappSize = 0
                             )
        {
            InitializeComponent();
            AvailableTags = LoadDataFromJson("C:\\Users\\Vadra\\source\\repos\\Magasin-PC\\Magasin PC\\Data\\tags.json");
            AvailableCategories = LoadDataFromJson("C:\\Users\\Vadra\\source\\repos\\Magasin-PC\\Magasin PC\\Data\\categories.json");
            tags = new ObservableCollection<string>();
            this.DataContext = this;
            SelectedTagsListBox.ItemsSource = tags;
            this.id = appId;
            Update(appId,initname, initfilepath);
            AppNameTextBox.Text = initname;
            AppDescriptionTextBox.Text = initdesc;
            AppVersionTextBox.Text = initvers;
            AppIsVisibleCheckBox.IsChecked = initisvisible;
            AppFilePathBox.Text = initfilepath;
            AppIconPathTextBox.Text = initicone;
            if (!string.IsNullOrWhiteSpace(initicone))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(initicone, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AppIconImage.Source = bitmap;
            }
            else
            {
                // Vous pouvez définir une image par défaut ici si l'icône n'existe pas
                AppIconImage.Source = null;
            }
            AppNameTextBox.TextChanged += AppNameTextBox_TextChanged;
        }

        // Méthode pour charger les données à partir d'un fichier JSON
        private ObservableCollection<string> LoadDataFromJson(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    var data = JsonConvert.DeserializeObject<ObservableCollection<string>>(jsonData);
                    return data ?? new ObservableCollection<string>();
                }
                else
                {
                    MessageBox.Show($"Le fichier {filePath} n'a pas été trouvé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new ObservableCollection<string>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier {filePath}: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<string>();
            }
        }

        private void BrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.ico)|*.png;*.jpg;*.jpeg;*.ico";

            if (openFileDialog.ShowDialog() == true)
            {
                string iconPath = openFileDialog.FileName;
                AppIconPathTextBox.Text = iconPath;

                // Charger l'image sélectionnée
                BitmapImage bitmap = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                AppIconImage.Source = bitmap;
            }
        }

        private void Update(Guid appId, string initname,string initfilepath)
        {
            var apps = (Application.Current.MainWindow.DataContext as MainViewModel)?.Apps;
            var existingFilepathApp = apps?.FirstOrDefault(app => app.FilePath == initfilepath && app.Id != appId);
            var existingNameApp = apps?.FirstOrDefault(app => app.Name == initname && app.Id != appId);
            WarningTextName.Visibility = Visibility.Collapsed;
            if (existingFilepathApp != null)
            {
                WarningTextFilePath.Visibility = Visibility.Visible;
            }
            else
            {
                WarningTextFilePath.Visibility = Visibility.Collapsed;
            }
            if (existingNameApp != null)
            {
                WarningTextName.Visibility = Visibility.Visible;
            }
            else
            {
                WarningTextName.Visibility = Visibility.Collapsed;
            }
        }

        private void AppNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Update(this.id, AppNameTextBox.Text, AppFilePathBox.Text);
        }

        private void ModifyFilePath_Click(object sender, RoutedEventArgs e)
        {
            FileService fileService = new FileService();
            string? filePath = fileService.SelectFile();  // Ouvre une boîte de dialogue pour sélectionner un fichier ZIP
            if (filePath != null)
            {
                this.AppFilePathBox.Text = filePath;
                var test = (this.DataContext as MainViewModel)?.Apps.FirstOrDefault(app => app.FilePath == filePath);

                if (test != null)
                {
                    WarningTextFilePath.Visibility = Visibility.Visible;
                }
                else
                {
                    WarningTextFilePath.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            id = this.id;
            name=this.AppNameTextBox.Text;
            description=this.AppDescriptionTextBox.Text;
            version=this.AppVersionTextBox.Text;
            isvisible = (bool)AppIsVisibleCheckBox.IsChecked;
            icone = this.AppIconPathTextBox.Text;
            filepath = this.AppFilePathBox.Text;

            this.DialogResult = true;
            this.Close();
        }

        // Bouton Ajouter Tag
        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagComboBox.SelectedItem != null)
            {
                string selectedTag = TagComboBox.SelectedItem.ToString();
                if (!tags.Contains(selectedTag))
                {
                    tags.Add(selectedTag);
                }
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            string tagToRemove = (sender as Button)?.Tag.ToString();
            if (tagToRemove != null && tags.Contains(tagToRemove))
            {
                tags.Remove(tagToRemove);
            }
        }


    }
}
