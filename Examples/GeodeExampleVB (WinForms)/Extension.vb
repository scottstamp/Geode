Imports Geode.Extension

<[Module]("Geode example", "Lilith", "For testing purposes only.")>
Public Class Extension
    Inherits GService
    Public MainWindowParent As MainWindow

    Public Sub New(ByVal MainWindowParent As MainWindow)
        Me.MainWindowParent = MainWindowParent
    End Sub

    Public Sub ShowAlert(ByVal Message As String)
        SendToClientAsync([In].HabboBroadcast, Message)
    End Sub
End Class