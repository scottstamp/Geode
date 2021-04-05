Public Class MainWindow
    Public ExtensionChild As Extension

    Private Sub MainWindow_Load(sender As Object, e As EventArgs) Handles Me.Load
        ExtensionChild = New Extension(Me)
    End Sub

    Private Sub SendAlertButton_Click(sender As Object, e As EventArgs) Handles SendAlertButton.Click
        ExtensionChild.ShowAlert("Hello world!")
    End Sub
End Class
