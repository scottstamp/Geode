<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainWindow
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.SendAlertButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'SendAlertButton
        '
        Me.SendAlertButton.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SendAlertButton.Location = New System.Drawing.Point(0, 0)
        Me.SendAlertButton.Name = "SendAlertButton"
        Me.SendAlertButton.Size = New System.Drawing.Size(282, 153)
        Me.SendAlertButton.TabIndex = 1
        Me.SendAlertButton.Text = "Send alert"
        Me.SendAlertButton.UseVisualStyleBackColor = True
        '
        'MainWindow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(282, 153)
        Me.Controls.Add(Me.SendAlertButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "MainWindow"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Example"
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents SendAlertButton As Button
End Class
