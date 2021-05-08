Imports System.ComponentModel

Public Class MainWindow
    Public ExtensionChild As Extension

    Private Sub MainWindow_Loaded(sender As Object, e As EventArgs) Handles Me.Loaded
        Try
            ExtensionChild = New Extension(Me) 'Try to start extension
        Catch
            Environment.Exit(0) 'Extension initialization failed.
        End Try
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If ExtensionChild.IsConnected Then
            ExtensionChild.ConsoleBot.HideBot() 'Hide bot before app closes
        End If
    End Sub
End Class