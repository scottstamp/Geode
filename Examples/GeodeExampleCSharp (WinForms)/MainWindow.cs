using System;
using System.Windows.Forms;

namespace GeodeExampleCSharp
{
    public partial class MainWindow : Form
    {
        public Extension ExtensionChild;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            ExtensionChild = new Extension(this);
        }

        private void SendAlertButton_Click(object sender, EventArgs e)
        {
            ExtensionChild.ShowAlert("Hello world!");
        }
    }
}