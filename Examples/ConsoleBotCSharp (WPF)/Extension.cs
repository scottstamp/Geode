using Geode.Extension;
using Geode.Network;
using Geode.Network.Protocol;
using System;

namespace ConsoleBotCSharp
{
    [Module("ConsoleBotCSharp", "Lilith", "Geode examples.", true, false)]
    public class Extension : GService
    {
        public MainWindow MainWindowParent;
        public ConsoleBot ConsoleBot;

        public Extension(MainWindow MainWindowParent)
        {
            this.MainWindowParent = MainWindowParent; // Set main window.
            //Add extension event handlers
            OnDataInterceptEvent += Extension_OnDataInterceptEvent;
            OnDoubleClickEvent += Extension_OnDoubleClickEvent;
            OnConnectedEvent += Extension_OnConnectedEvent;
            OnCriticalErrorEvent += Extension_OnCriticalErrorEvent;
            //
            ConsoleBot = new ConsoleBot(this, "VB example"); // Instantiate a new ConsoleBot
            ConsoleBot.OnMessageReceived += ConsoleBot_OnMessageReceived; //Add ConsoleBot event handler
        }

        public void BotShowAndWelcome()
        {
            ConsoleBot.ShowBot();
            BotWelcome();
        }

        public void BotWelcome()
        {
            ConsoleBot.BotSendMessage("Welcome |");
            ConsoleBot.BotSendMessage("Use /help to get info.");
        }

        private void ConsoleBot_OnMessageReceived(object sender, string e)
        {
            switch (e.ToLower() ?? "") // Handle received message
            {
                case "/help":
                    {
                        ConsoleBot.BotSendMessage("Commands:");
                        ConsoleBot.BotSendMessage("/look1 and /look2 to change current look.");
                        ConsoleBot.BotSendMessage("/sit to force sit.");
                        ConsoleBot.BotSendMessage("/fx to get light sabber fx.");
                        ConsoleBot.BotSendMessage("/exit to exit extension.");
                        break;
                    }

                case "/look1":
                    {
                        SendToServerAsync(Out.UpdateFigureData, "F", "hr-515-45.ch-665-71.lg-3216-73.hd-600-10.fa-3276-72");
                        break;
                    }

                case "/look2":
                    {
                        SendToServerAsync(Out.UpdateFigureData, "M", "hr-893-45.ch-235-71.lg-3290-82.hd-180-10.fa-3276-72");
                        break;
                    }

                case "/sit":
                    {
                        SendToServerAsync(Out.ChangePosture, 1);
                        break;
                    }

                case "/fx":
                    {
                        SendToServerAsync(Out.Chat, ":yyxxabxa", 0, -1);
                        break;
                    }

                default:
                    {
                        BotWelcome();
                        break;
                    }
            }
        }

        private void Extension_OnDataInterceptEvent(object sender, DataInterceptedEventArgs e)
        {
            if (e.Packet.Id == In.FriendRequests.Id) // Show Bot when the initial console load is complete.
            {
                BotShowAndWelcome();
            }
        }

        private void Extension_OnConnectedEvent(object sender, HPacket e) // G-Earth is connected.
        {
            BotShowAndWelcome();
        }

        private void Extension_OnDoubleClickEvent(object sender, HPacket e) // G-Earth extension play button clicked.
        {
            if (IsConnected)
            {
                BotShowAndWelcome();
            }
        }

        private void Extension_OnCriticalErrorEvent(object sender, string e) // G-Earth is probably closed or the connection was rejected.
        {
            Environment.Exit(0);
        }
    }
}
