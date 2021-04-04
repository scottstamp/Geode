Public Class MainWindow
    Public ExtensionChild As Extension

    Private Sub MainWindow_Loaded(sender As Object, e As EventArgs) Handles Me.Loaded
        ExtensionChild = New Extension(Me)
    End Sub

    Private Sub ShowCriticalError()
        Environment.Exit(0)
    End Sub

End Class