using System.Globalization;
using System.Windows.Data;

namespace Arzenal.StoreManager.WPF.Converters
{
    public class MaxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height)
            {
                // Ajustez ce facteur selon vos besoins pour la hauteur maximale
                return height * 0.6; // Exemple : 60% de la hauteur disponible
            }
            return 200.0; // Valeur par défaut si la hauteur n'est pas disponible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
