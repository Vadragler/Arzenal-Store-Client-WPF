using Magasin_PC.Settings.Database;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

public class AppModel : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string FilePath { get; set; }
    public string? Description { get; set; }
    public bool IsVisible { get; set; }
    public string? IconePath { get; set; }

    public ImageSource? Icone { get; set; }
    public ObservableCollection<OperatingSystemModel> OS { get; set; }
    public ObservableCollection<TagModel>? Tag { get; set; }

    public int CategoryId { get; set; } // Propriété pour l'identifiant de la catégorie
    // Propriétés supplémentaires
    public CategorieModel? Category { get; set; }  // Catégorie de l'application
    public DateTime ReleaseDate { get; set; } = DateTime.Now;  // Date de publication
    public DateTime? LastUpdated { get; set; }  // Dernière mise à jour
    public long AppSize { get; set; }  // Taille de l'application en octets
    public ObservableCollection<LanguageModel>? Languages { get; set; }  // Langues disponibles
    public string? Requirements { get; set; }  // Conditions requises (ex: version minimum de l'OS)

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
