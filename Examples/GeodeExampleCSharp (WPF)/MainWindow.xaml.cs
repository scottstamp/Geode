using System.Windows;
namespace GeodeExampleCSharp
{
    public partial class MainWindow : Window
    {
        public Extension ExtensionChild;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExtensionChild = new Extension(this);
        }

        private void SendAlertButton_Click(object sender, RoutedEventArgs e)
        {
            ExtensionChild.ShowAlert("Hello world!");
        }
    }
}
