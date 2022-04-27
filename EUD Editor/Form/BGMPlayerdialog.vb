Public Class BGMPlayerdialog
    Private Sub BGMPlayerdialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load, FastColoredTextBox1.Load
        Me.Text = Lan.GetText(Me.Name, "Name")
        Lan.SetLangage(Me)
        ThemeSetForm.SetControlColor(Me)
    End Sub
End Class