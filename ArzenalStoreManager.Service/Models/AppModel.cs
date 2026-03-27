namespace Arzenal.StoreManager.Core.Models
{

    public class AppModel
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Version { get; set; }
        public string? FilePath { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public string? IconePath { get; set; }
        public List<string>? Languages { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? OperatingSystems { get; set; }
        public string Category { get; set; }
        // Catégorie de l'application
        public DateTime? ReleaseDate { get; set; }  // Date de publication
        public DateTime? LastUpdated { get; set; }  // Dernière mise à jour
        public long AppSize { get; set; }  // Taille de l'application en octets
                                           // Langues disponibles
        public string? Requirements { get; set; }  // Conditions requises (ex: version minimum de l'OS)


    }
}
