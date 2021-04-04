Imports Geode.Extension
Imports Geode.Network

<[Module]("ChatConsole", "Lilith", "For testing purposes only.")>
Public Class Extension
    Inherits GService
    Public MainWindowParent As MainWindow
    Dim BotFriendID As Integer = 999999999
    Dim BotFriendCreatorName As String = "Lilith"
    Dim BotFriendCreatorLook As String = "hr-3870-45.hd-600-10.ch-665-71.lg-3781-100-71.ha-3614-91-95.he-3469-1412.fa-3276-1412.ca-3702-71-71"
    Dim BotFriendName As String = "ChatConsole"
    Dim BotFriendMotto As String = "For testing purposes only."
    Dim BotFriendCreationDate As String = "04-04-2021"
    Dim BotFriendLook As String = "hd-3704-29.ch-3135-95.lg-3136-95"
    Dim BotFriendBadges As String() = New String() {"BOT", "FR17A", "NO83", "ITB26", "NL446"}

    Public Sub New(MainWindowParent As MainWindow)
        Me.MainWindowParent = MainWindowParent
    End Sub

    Public Overrides Sub OnDataIntercept(data As DataInterceptedEventArgs)
        If data.Packet.Id = Out.GetExtendedProfile.Id Then
            Dim RequestedFriendID As Integer = data.Packet.ReadInt32()
            If RequestedFriendID = BotFriendID Then
                SendToClientAsync([In].ExtendedProfile, BotFriendID, BotFriendName, BotFriendLook, BotFriendMotto, BotFriendCreationDate, 0, 1, True, False, True, 0, -255, True)
                SendToClientAsync([In].HabboUserBadges, BotFriendID, BotFriendBadges.Length, 1, BotFriendBadges(0), 2, BotFriendBadges(1), 3, BotFriendBadges(2), 4, BotFriendBadges(3), 5, BotFriendBadges(4))
                SendToClientAsync([In].RelationshipStatusInfo, BotFriendID, 1, 1, 1, 0, BotFriendCreatorName, BotFriendCreatorLook)
            End If
        End If
        If data.Packet.Id = Out.SendMsg.Id Then
            Dim RequestedFriendID As Integer = data.Packet.ReadInt32()
            Dim RequestedFriendText As String = data.Packet.ReadUTF8()
            If RequestedFriendID = BotFriendID Then
                data.IsBlocked = True
                Dim CommandHandled As Boolean = False
                If RequestedFriendText = "/exit" Then
                    CommandHandled = True
                    HideBotFriend()
                    MyBase.OnDataIntercept(data)
                    Environment.Exit(0)
                End If
                If RequestedFriendText.ToLower = "/help" Then
                    CommandHandled = True
                    BotFriendSendMessage("Commands:")
                    BotFriendSendMessage("/look1 and /look2 to change current look.")
                    BotFriendSendMessage("/sit to force sit.")
                    BotFriendSendMessage("/fx to get light sabber fx.")
                    BotFriendSendMessage("/exit to exit extension.")
                End If
                If RequestedFriendText.ToLower = "/look1" Then
                    CommandHandled = True
                    SendToServerAsync(Out.UpdateFigureData, "F", "hr-515-45.ch-665-71.lg-3216-73.hd-600-10.fa-3276-72")
                End If
                If RequestedFriendText.ToLower = "/look2" Then
                    CommandHandled = True
                    SendToServerAsync(Out.UpdateFigureData, "M", "hr-893-45.ch-235-71.lg-3290-82.hd-180-10.fa-3276-72")
                End If
                If RequestedFriendText.ToLower = "/sit" Then
                    CommandHandled = True
                    SendToServerAsync(Out.ChangePosture, 1)
                End If
                If RequestedFriendText.ToLower = "/fx" Then
                    CommandHandled = True
                    SendToServerAsync(Out.Chat, ":yyxxabxa", 0, -1)
                End If
                If CommandHandled = False Then
                    BotFriendWelcome()
                End If
            End If
        End If
        MyBase.OnDataIntercept(data)
    End Sub

    <InDataCapture("FriendRequests")>
    Public Sub OnFriendRequests(ByVal e As DataInterceptedEventArgs)
        ShowBotFriend()
        BotFriendWelcome()
    End Sub

    Sub BotFriendWelcome()
        BotFriendSendMessage("Welcome |")
        BotFriendSendMessage("Use /help to get info.")
    End Sub

    Sub BotFriendSendMessage(ByVal Message As String)
        SendToClientAsync([In].NewConsole, BotFriendID, Message, 0, "")
    End Sub

    Sub ShowBotFriend()
        Dim CreatorRelation As Integer = 65537
        SendToClientAsync([In].FriendListUpdate, 0, 1, False, False, "", BotFriendID, Chr(1) & "[BOT] " & BotFriendName, 1, True, False, BotFriendLook, 0, "", 0, True, True, True, CreatorRelation)
    End Sub

    Sub HideBotFriend()
        SendToClientAsync([In].FriendListUpdate, 0, 1, -1, BotFriendID)
    End Sub

    Public Overrides Sub OnConnected(packet As Protocol.HPacket)
        MyBase.OnConnected(packet)
        ShowBotFriend()
        BotFriendWelcome()
    End Sub

    Public Overrides Sub OnCriticalError(error_desc As String)
        MyBase.OnCriticalError(error_desc)
        Environment.Exit(0)
    End Sub

End Class