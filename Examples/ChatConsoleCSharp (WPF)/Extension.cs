using Geode.Extension;
using Geode.Network;
using System;

namespace ChatConsoleCSharp
{
    [Module("ChatConsole", "Lilith", "For testing purposes only.",true,false)]
    public class Extension : GService
    {
        public MainWindow MainWindowParent;
        private int BotFriendID = 999999999;
        private string BotFriendCreatorName = "Lilith";
        private string BotFriendCreatorLook = "hr-3870-45.hd-600-10.ch-665-71.lg-3781-100-71.ha-3614-91-95.he-3469-1412.fa-3276-1412.ca-3702-71-71";
        private string BotFriendName = "ChatConsole";
        private string BotFriendMotto = "For testing purposes only.";
        private string BotFriendCreationDate = "05-04-2021";
        private string BotFriendLook = "hd-3704-29.ch-3135-95.lg-3136-95";
        private string[] BotFriendBadges = new string[] { "BOT", "FR17A", "NO83", "ITB26", "NL446" };

        public Extension(MainWindow MainWindowParent)
        {
            this.MainWindowParent = MainWindowParent; // Set main window.
        }

        public override void OnDataIntercept(DataInterceptedEventArgs data)
        {
            if (data.Packet.Id == Out.SendMsg.Id) // User sent a message.
            {
                int RequestedFriendID = data.Packet.ReadInt32();
                string RequestedMessage = data.Packet.ReadUTF8();
                if (RequestedFriendID == BotFriendID) // Bot received a message.
                {
                    data.IsBlocked = true;
                    switch (RequestedMessage.ToLower()) // Handle received message
                    {
                        case "/exit":
                            {
                                HideBotFriend();
                                base.OnDataIntercept(data);
                                Environment.Exit(0);
                                break;
                            }
                        case "/help":
                            {
                                BotFriendSendMessage("Commands:");
                                BotFriendSendMessage("/look1 and /look2 to change current look.");
                                BotFriendSendMessage("/sit to force sit.");
                                BotFriendSendMessage("/fx to get light sabber fx.");
                                BotFriendSendMessage("/exit to exit extension.");
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
                                BotFriendWelcome();
                                break;
                            }
                    }
                }
            }
            if (data.Packet.Id == Out.RemoveFriend.Id) //User requested a friend remove.
            {
                data.Packet.ReadInt32();
                int RequestedFriendID = data.Packet.ReadInt32();
                if (RequestedFriendID == BotFriendID) //Bot remove was requested.
                {
                    data.IsBlocked = true;
                    HideBotFriend();
                    base.OnDataIntercept(data);
                    Environment.Exit(0);
                }
            }
            base.OnDataIntercept(data);
        }

        [OutDataCapture("GetExtendedProfile")] // Bot profile was requested.
        public void OnGetExtendedProfile(DataInterceptedEventArgs e)
        {
            int RequestedFriendID = e.Packet.ReadInt32();
            if (RequestedFriendID == BotFriendID)
            {
                SendToClientAsync(In.ExtendedProfile, BotFriendID, BotFriendName, BotFriendLook, BotFriendMotto, BotFriendCreationDate, 0, 1, true, false, true, 0, -255, true);
                SendToClientAsync(In.HabboUserBadges, BotFriendID, BotFriendBadges.Length, 1, BotFriendBadges[0], 2, BotFriendBadges[1], 3, BotFriendBadges[2], 4, BotFriendBadges[3], 5, BotFriendBadges[4]);
                SendToClientAsync(In.RelationshipStatusInfo, BotFriendID, 1, 1, 1, 0, BotFriendCreatorName, BotFriendCreatorLook);
            }
        }

        [InDataCapture("FriendRequests")] // Show Bot when the initial console load is complete.
        public void OnFriendRequests(DataInterceptedEventArgs e)
        {
            ShowBotFriend();
            BotFriendWelcome();
        }

        public void BotFriendWelcome()
        {
            BotFriendSendMessage("Welcome |");
            BotFriendSendMessage("Use /help to get info.");
        }

        public void BotFriendSendMessage(string Message)
        {
            SendToClientAsync(In.NewConsole, BotFriendID, Message, 0, "");
        }

        public void ShowBotFriend()
        {
            HideBotFriend();
            int CreatorRelation = 65537;
            SendToClientAsync(In.FriendListUpdate, 0, 1, false, false, "", BotFriendID, (char)1 + "[BOT] " + BotFriendName, 1, true, false, BotFriendLook, 0, "", 0, true, true, true, CreatorRelation);
        }

        public void HideBotFriend()
        {
            SendToClientAsync(In.FriendListUpdate, 0, 1, -1, BotFriendID);
        }

        public override void OnDoubleClick(Geode.Network.Protocol.HPacket packet) // G-Earth extension play button clicked.
        {
            base.OnDoubleClick(packet);
            ShowBotFriend();
        }

        public override void OnConnected(Geode.Network.Protocol.HPacket packet)  // G-Earth is connected.
        {
            base.OnConnected(packet);
            ShowBotFriend();
            BotFriendWelcome();
        }

        public override void OnDisconnected(Geode.Network.Protocol.HPacket packet) // G-Earth is open, but disconnected.
        {
            base.OnDisconnected(packet);
        }

        public override void OnCriticalError(string error_desc) // G-Earth is probably closed or the connection was rejected.
        {
            base.OnCriticalError(error_desc);
            Environment.Exit(0);
        }
    }
}
