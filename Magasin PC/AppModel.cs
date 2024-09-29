using System.Collections.ObjectModel;
using System.ComponentModel;

public class AppModel : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string FilePath { get; set; }
    public string? Description { get; set; }
    public bool IsVisible { get; set; }
    public string? Icone { get; set; }
    public required ObservableCollection<string> Platform { get; set; }
    public ObservableCollection<string>? Tag { get; set; }

    // Propriétés supplémentaires
    public string? Category { get; set; }  // Catégorie de l'application
    public DateTime ReleaseDate { get; set; } = DateTime.Now;  // Date de publication
    public DateTime? LastUpdated { get; set; }  // Dernière mise à jour
    public long AppSize { get; set; }  // Taille de l'application en octets
    public ObservableCollection<string>? Languages { get; set; }  // Langues disponibles
    public string? Requirements { get; set; }  // Conditions requises (ex: version minimum de l'OS)

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
