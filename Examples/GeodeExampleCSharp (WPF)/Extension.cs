using Geode.Extension;

namespace GeodeExampleCSharp
{
    [Module("Geode example", "Lilith", "For testing purposes only.")]
    public class Extension : GService
    {
        public MainWindow MainWindowParent;

        public Extension(MainWindow MainWindowParent)
        {
            this.MainWindowParent = MainWindowParent;
        }

        public void ShowAlert(string Message)
        {
            SendToClientAsync(In.HabboBroadcast, Message);
        }

    }
}
