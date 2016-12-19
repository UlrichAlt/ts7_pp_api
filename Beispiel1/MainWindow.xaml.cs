using System.Windows;
using TopSolid.Kernel.Automating;

namespace Beispiel1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void versionBox_Loaded(object sender, RoutedEventArgs e)
        {
            TopSolidHost.Connect();
            versionBox.Text = TopSolidHost.Application.Version.ToString();
            TopSolidHost.Disconnect();
        }
    }
}
