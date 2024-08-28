using System.ComponentModel;

public class AppModel : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();
    private string name;
    private string version;
    private string filePath;
    private string? description;
    private bool isVisible;
    private string? icone;

    public string Name
    {
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public string Version
    {
        get => version;
        set
        {
            if (version != value)
            {
                version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
    }

    public string FilePath
    {
        get => filePath;
        set
        {
            if (filePath != value)
            {
                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }
    }

    public string? Description
    {
        get => description;
        set
        {
            if (description != value)
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible != value)
            {
                isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public string? Icone
    {
        get => icone;
        set
        {
            if (icone != value) 
            {
                icone = value;
                OnPropertyChanged(nameof(Icone));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
