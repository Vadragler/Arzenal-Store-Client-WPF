using MagasinPC;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Magasin_PC
{
    /// <summary>
    /// Logique d'interaction pour AppInfoWindow.xaml
    /// </summary>
    public partial class AppInfoWindow : Window
    {
        public Guid id { get; private set; }
        public string name { get; private set; }
        public string description { get; private set; }
        public string version { get; private set; }
        public bool isvisible { get; private set; }
        public string icone { get; private set; }
        

        public AppInfoWindow(Guid appId, string initname="",string? initdesc="",string initvers="",bool initisvisible=false,string initfilepath="",string initicone="")
        {
            InitializeComponent();
            this.id = appId;
            Update(appId,initname, initfilepath);
            AppNameTextBox.Text = initname;
            AppDescriptionTextBox.Text = initdesc;
            AppVersionTextBox.Text = initvers;
            AppIsVisibleCheckBox.IsChecked = initisvisible;
            AppFilePathBox.Text = initfilepath;
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
            isvisible = (bool)this.AppIsVisibleCheckBox.IsChecked;
            icone = this.AppIconPathTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
