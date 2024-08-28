using Magasin_PC;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MagasinPC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.UpdateAppButton.Visibility = Visibility.Collapsed;
            this.DataContext = new MainViewModel();
        }

        private void AppList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Si l'utilisateur clique à l'extérieur d'un élément sélectionné
            if (AppList.SelectedItem != null)
            {
                // Vérifie si le clic s'est produit à l'extérieur de l'élément sélectionné
                var point = e.GetPosition(AppList);
                var hitTestResult = VisualTreeHelper.HitTest(AppList, point);

                if (hitTestResult == null || !(hitTestResult.VisualHit is FrameworkElement element) || element.DataContext != AppList.SelectedItem)
                {
                    // Désélectionner l'élément
                    AppList.SelectedItem = null;
                    this.AddAppButton.Visibility = Visibility.Visible;
                    this.UpdateAppButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.AddAppButton.Visibility = Visibility.Collapsed;
                    this.UpdateAppButton.Visibility = Visibility.Visible;
                }
            }
        }

        
        private void UpdateApp_Click(object sender, RoutedEventArgs e)
        {
            var AppSelected = (AppModel)AppList.SelectedItem;
            if (AppSelected != null)
            {
                AppInfoWindow appInfoWindow = new AppInfoWindow(AppSelected.Id, AppSelected.Name, AppSelected.Description, AppSelected.Version, AppSelected.IsVisible, AppSelected.FilePath);
                bool? result = appInfoWindow.ShowDialog();
                if (result == true)
                {
                    AppSelected.Icone = appInfoWindow.icone;
                    AppSelected.Name = appInfoWindow.name;
                    AppSelected.Version = appInfoWindow.version;
                    AppSelected.Description = appInfoWindow.description;
                    AppSelected.IsVisible = appInfoWindow.IsVisible;

                    (this.DataContext as MainViewModel).SaveApps();
                }
            }
        }

        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            FileService fileService = new FileService();
            string? filePath = fileService.SelectFile();  // Ouvre une boîte de dialogue pour sélectionner un fichier ZIP
            if (filePath != null)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                AppInfoWindow appInfoWindow = new AppInfoWindow(Guid.NewGuid(), name, null,"Alpha 0.0.1 build 0",false,filePath);
                bool? result = appInfoWindow.ShowDialog();
                if (result == true)
                {

                    // Crée un nouvel objet AppModel avec les détails de l'application ZIP
                    AppModel newApp = new AppModel
                    {
                        Id = appInfoWindow.id,
                        Icone = appInfoWindow.icone,
                        Name = appInfoWindow.name,
                        FilePath = filePath,
                        Version = appInfoWindow.version,
                        Description = appInfoWindow.description,
                        IsVisible = appInfoWindow.IsVisible,
                        // Version par défaut, vous pouvez modifier cela si nécessaire
                    };
                    (this.DataContext as MainViewModel).AddApp(newApp);  // Ajoute l'application au ViewModel
                }
            }
        }

        private void RemoveApp_Click(object sender, RoutedEventArgs e)
        {
            var selectedApp = (AppModel)AppList.SelectedItem;  // Récupère l'application sélectionnée dans la liste
            if (selectedApp != null)
            {
                (this.DataContext as MainViewModel).RemoveApp(selectedApp);
                if (AppList != null) 
                {
                    AppList.SelectedItem = null;
                    this.AddAppButton.Visibility = Visibility.Visible;
                    this.UpdateAppButton.Visibility = Visibility.Collapsed;
                }// Supprime l'application du ViewModel
                AppList.SelectedItem = null;
            }
        }

    }
}
