Imports Geode.Extension
Imports Geode.Network
Imports Geode.Network.Protocol

<[Module]("ConsoleBotVB", "Lilith", "Geode examples.", True, False)>
Public Class Extension
    Inherits GService
    Public MainWindowParent As MainWindow
    Public WithEvents ConsoleBot As ConsoleBot

    Public Sub New(MainWindowParent As MainWindow)
        Me.MainWindowParent = MainWindowParent 'Set main window.
        ConsoleBot = New ConsoleBot(Me, "VB example") 'Instantiate a new ConsoleBot
    End Sub

    Sub BotShowAndWelcome()
        ConsoleBot.ShowBot()
        BotWelcome()
    End Sub

    Sub BotWelcome()
        ConsoleBot.BotSendMessage("Welcome |")
        ConsoleBot.BotSendMessage("Use /help to get info.")
    End Sub

    Private Sub ConsoleBot_OnMessageReceived(sender As Object, e As String) Handles ConsoleBot.OnMessageReceived
        Select Case e.ToLower 'Handle received message
            Case "/help"
                ConsoleBot.BotSendMessage("Commands:")
                ConsoleBot.BotSendMessage("/look1 and /look2 to change current look.")
                ConsoleBot.BotSendMessage("/sit to force sit.")
                ConsoleBot.BotSendMessage("/fx to get light sabber fx.")
                ConsoleBot.BotSendMessage("/exit to exit extension.")
            Case "/look1"
                SendToServerAsync(Out.UpdateFigureData, "F", "hr-515-45.ch-665-71.lg-3216-73.hd-600-10.fa-3276-72")
            Case "/look2"
                SendToServerAsync(Out.UpdateFigureData, "M", "hr-893-45.ch-235-71.lg-3290-82.hd-180-10.fa-3276-72")
            Case "/sit"
                SendToServerAsync(Out.ChangePosture, 1)
            Case "/fx"
                SendToServerAsync(Out.Chat, ":yyxxabxa", 0, -1)
            Case Else
                BotWelcome()
        End Select
    End Sub

    Private Sub Extension_OnDataInterceptEvent(sender As Object, e As DataInterceptedEventArgs) Handles Me.OnDataInterceptEvent
        If e.Packet.Id = [In].FriendRequests.Id Then 'Show Bot when the initial console load is complete.
            BotShowAndWelcome()
        End If
    End Sub

    Private Sub Extension_OnDoubleClickEvent(sender As Object, e As HPacket) Handles Me.OnDoubleClickEvent 'G-Earth extension play button clicked.
        If IsConnected Then
            BotShowAndWelcome()
        End If
    End Sub

    Private Sub Extension_OnConnectedEvent(sender As Object, e As HPacket) Handles Me.OnConnectedEvent 'G-Earth is connected.
        BotShowAndWelcome()
    End Sub

    Private Sub Extension_OnCriticalErrorEvent(sender As Object, e As String) Handles Me.OnCriticalErrorEvent 'G-Earth is probably closed or the connection was rejected.
        Environment.Exit(0)
    End Sub

End Class