''' <summary>
''' Shows what stopped the editor, and lets the user copy the details. The editor
''' stays open after an error on the interface thread, so this window says what
''' happened and where the recovery copy is.
''' </summary>
Public Class CrashForm
    Inherits Form

    Private ReadOnly detailText As String
    Private WithEvents copyButton As Button
    Private WithEvents closeButton As Button

    Public Sub New(ex As Exception, recoverySaved As Boolean)
        detailText = Describe(ex)

        Me.Text = "EUD Editor"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowIcon = False
        Me.ClientSize = New Size(560, 380)
        Me.MinimumSize = New Size(420, 300)
        Me.Padding = New Padding(14)

        Dim headline As New Label With {
            .Dock = DockStyle.Top, .AutoSize = False, .Height = 26,
            .Text = "The editor met an error it did not expect.",
            .Font = New Font(Me.Font.FontFamily, 10.0F, FontStyle.Bold)}

        Dim summary As New Label With {
            .Dock = DockStyle.Top, .AutoSize = False, .Height = 58,
            .Text = ex.GetType().Name & ": " & ex.Message}

        Dim advice As New Label With {
            .Dock = DockStyle.Top, .AutoSize = False, .Height = 42,
            .Text = If(recoverySaved,
                       "Your work is still open. A recovery copy was written to:" & Environment.NewLine &
                       RecoveryModule.RecoveryFile,
                       "Your work is still open. Save it now." & Environment.NewLine &
                       "The details below are also in " & LogPath)}

        Dim details As New TextBox With {
            .Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True,
            .ScrollBars = ScrollBars.Vertical, .WordWrap = False,
            .Font = New Font("Consolas", 8.5F), .Text = detailText}

        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom, .Height = 40, .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 8, 0, 0)}
        closeButton = New Button With {.Text = "Close", .Width = 90, .DialogResult = DialogResult.OK}
        copyButton = New Button With {.Text = "Copy details", .Width = 110}
        buttons.Controls.Add(closeButton)
        buttons.Controls.Add(copyButton)

        Me.Controls.Add(details)
        Me.Controls.Add(advice)
        Me.Controls.Add(summary)
        Me.Controls.Add(headline)
        Me.Controls.Add(buttons)
        Me.AcceptButton = closeButton
        Me.CancelButton = closeButton

        Try
            ThemeSetForm.SetControlColor(Me)
        Catch sup1 As Exception
            LogSuppressed(sup1, "CrashForm.New")
            'A theme is a small thing next to an error report.
        End Try
    End Sub

    Private Shared Function Describe(ex As Exception) As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("EUD Editor SE " & ProgramSet.Version)
        sb.AppendLine(Date.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        sb.AppendLine()
        Dim current As Exception = ex
        While current IsNot Nothing
            sb.AppendLine(current.GetType().FullName & ": " & current.Message)
            sb.AppendLine(current.StackTrace)
            current = current.InnerException
            If current IsNot Nothing Then sb.AppendLine("-- inner --")
        End While
        Return sb.ToString()
    End Function

    Private Sub CopyButton_Click(sender As Object, e As EventArgs) Handles copyButton.Click
        Try
            Clipboard.SetText(detailText)
            copyButton.Text = "Copied"
        Catch ex As Exception
            LogException(ex, "copying the error details")
        End Try
    End Sub
End Class
