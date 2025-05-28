using System.Windows;
using System.Windows.Media.Imaging;


namespace Arzenal_Store_Client_WPF
{


    public partial class ConfirmationWindow : Window
    {
        private Window _ownerWindow;

        public ConfirmationWindow()
        {

            InitializeComponent();



            Uri iconUri = new Uri("pack://application:,,,/Ressources/Image/Warning.ico", UriKind.Absolute);
            BitmapImage iconBitmap = new BitmapImage(iconUri);
            this.Icon = iconBitmap;



            this.Activated += ConfirmationWindow_Activated!;
            this.Deactivated += ConfirmationWindow_Deactivated!;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ConfirmationWindow_Activated(object sender, EventArgs e)
        {
            // Assurez-vous qu'elle reste au-dessus si la fenêtre principale est active
            /*if (_ownerWindow.IsActive)
            {
                this.Topmost = true;
            }*/
        }

        private void ConfirmationWindow_Deactivated(object sender, EventArgs e)
        {
            // Ne récupérez le focus que si l'application principale est active
            /*if (_ownerWindow.IsActive)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Topmost = true;  // Assurez-vous qu'elle reste au-dessus
                    this.Activate();      // Récupérez le focus si l'application est active
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }*/
        }
    }



}
