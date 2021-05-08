using Geode.Network;
using System;

namespace Geode.Extension
{
    public class ConsoleBot
    {
        private GService ExtensionChild;
        public event EventHandler<string> OnMessageReceived;
        private readonly int BotID = new Random().Next(999000000, 999999999);
        public string BotName { get; private set; }
        public string BotMotto { get; private set; }
        public string BotLook { get; private set; }
        public string BotBadges { get; private set; }
        public string BotCreationDate { get; private set; }
        public string BotCreatorName { get; private set; }
        public string BotCreatorLook { get; private set; }

        public ConsoleBot(GService ExtensionChild, string BotName, string BotMotto = "Console bot.", string BotLook = "hd-3704-29.ch-3135-95.lg-3136-95", string BotBadges = "BOT FR17A NO83 ITB26 NL446", string BotCreationDate = "W-W-1984", string BotCreatorName = "Lilith", string BotCreatorLook = "hr-3870-45.hd-600-10.ch-665-71.lg-3781-100-71.ha-3614-91-95.he-3469-1412.fa-3276-1412.ca-3702-71-71")
        {
            {
                this.ExtensionChild = ExtensionChild;
                this.ExtensionChild.OnDataInterceptEvent += ExtensionChild_OnDataInterceptEvent;
                this.BotName = BotName;
                this.BotMotto = BotMotto;
                this.BotLook = BotLook;
                this.BotBadges = BotBadges;
                this.BotCreationDate = BotCreationDate;
                this.BotCreatorName = BotCreatorName;
                this.BotCreatorLook = BotCreatorLook;
            }
        }

        private void ExtensionChild_OnDataInterceptEvent(object sender, DataInterceptedEventArgs e)
        {
            if (e.Packet.Id == ExtensionChild.Out.SendMsg.Id) // User sent a message.
            {
                int RequestedFriendID = e.Packet.ReadInt32();
                string RequestedMessage = e.Packet.ReadUTF8();
                if (RequestedFriendID == BotID) // Bot received a message.
                {
                    e.IsBlocked = true;
                    OnMessageReceived?.Invoke(this, RequestedMessage);
                    if (RequestedMessage.ToLower() == "/exit")
                    {
                        HideBot();
                        ExtensionChild.DisableEventHandlers = true; // To avoid infinite event handler loop
                        ExtensionChild.OnDataIntercept(e);
                        Environment.Exit(0);
                    }
                }
            }

            if (e.Packet.Id == ExtensionChild.Out.RemoveFriend.Id) // User requested a friend remove.
            {
                e.Packet.ReadInt32();
                int RequestedFriendID = e.Packet.ReadInt32();
                if (RequestedFriendID == BotID) // Bot remove was requested.
                {
                    e.IsBlocked = true;
                    HideBot();
                    ExtensionChild.DisableEventHandlers = true; // To avoid infinite event handler loop
                    ExtensionChild.OnDataIntercept(e);
                    Environment.Exit(0);
                }
            }

            if (e.Packet.Id == ExtensionChild.Out.GetExtendedProfile.Id) // Bot profile was requested.
            {
                int RequestedFriendID = e.Packet.ReadInt32();
                if (RequestedFriendID == BotID)
                {
                    e.IsBlocked = true;
                    var BotBadgesArray = new string[] { "", "", "", "", "" };
                    for (int i = 0; i <= BotBadges.Split(' ').Length - 1; i++)
                    {
                        BotBadgesArray[i] = BotBadges.Split(' ')[i];
                    }
                    ExtensionChild.SendToClientAsync(ExtensionChild.In.ExtendedProfile, BotID, BotName, BotLook, BotMotto, BotCreationDate, 0, 1, true, false, true, 0, -255, true);
                    ExtensionChild.SendToClientAsync(ExtensionChild.In.HabboUserBadges, BotID, BotBadgesArray.Length, 1, BotBadgesArray[0], 2, BotBadgesArray[1], 3, BotBadgesArray[2], 4, BotBadgesArray[3], 5, BotBadgesArray[4]);
                    ExtensionChild.SendToClientAsync(ExtensionChild.In.RelationshipStatusInfo, BotID, 1, 1, 1, 0, BotCreatorName, BotCreatorLook);
                }
            }
        }

        public void BotSendMessage(string Message)
        {
            ExtensionChild.SendToClientAsync(ExtensionChild.In.NewConsole, BotID, Message, 0, "");
        }

        public void ShowBot()
        {
            HideBot();
            int CreatorRelation = 65537;
            ExtensionChild.SendToClientAsync(ExtensionChild.In.FriendListUpdate, 0, 1, false, false, "", BotID, '\u0001' + "[BOT] " + BotName, 1, true, false, BotLook, 0, "", 0, true, true, true, CreatorRelation);
        }

        public void HideBot()
        {
            ExtensionChild.SendToClientAsync(ExtensionChild.In.FriendListUpdate, 0, 1, -1, BotID);
        }
    }
}
