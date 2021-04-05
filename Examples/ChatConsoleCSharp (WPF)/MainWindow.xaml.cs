using System;
using System.Windows;

namespace ChatConsoleCSharp
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
            try
            {
                ExtensionChild = new Extension(this); // Try to start extension
            }
            catch
            {
                Environment.Exit(0); // Extension initialization failed.
            }
        }
    }
}
