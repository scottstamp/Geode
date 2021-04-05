Class MainWindow
    Public ExtensionChild As Extension

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ExtensionChild = New Extension(Me)
    End Sub

    Private Sub SendAlertButton_Click(sender As Object, e As RoutedEventArgs)
        ExtensionChild.ShowAlert("Hello world!")
    End Sub
End Class
