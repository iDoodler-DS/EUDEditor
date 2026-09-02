Imports EUD_Editor.EpsSource

''' <summary>
''' Picks one of the calls the editor knows, to put a new line in the source.
''' The list is everything the tables and eudplib between them describe, so it
''' holds far more than the classic trigger set.
''' </summary>
Public Class EpsPickCallForm
    Inherits Form

    Private WithEvents filter As New TextBox()
    Private WithEvents list As New ListBox()
    Private ReadOnly note As New Label()
    Private WithEvents okButton As New Button()
    Private WithEvents cancelButton As New Button()

    Private ReadOnly every As List(Of String)

    Public Property Picked As String = ""

    ''' <summary>Shows the picker and gives back the name chosen, or "".</summary>
    Public Shared Function Ask(owner As IWin32Window) As String
        Using dialog As New EpsPickCallForm()
            If dialog.ShowDialog(owner) = DialogResult.OK Then Return dialog.Picked
        End Using
        Return ""
    End Function

    Public Sub New()
        every = EpsSymbols.Names()

        Me.Text = "Add a line"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(430, 460)
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowIcon = False
        Me.Padding = New Padding(10)
        Me.Font = SystemFonts.MessageBoxFont

        filter.Dock = DockStyle.Top
        list.Dock = DockStyle.Fill
        list.IntegralHeight = False

        note.Dock = DockStyle.Bottom
        note.AutoSize = False
        note.Height = 48

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 36,
                                                 .FlowDirection = FlowDirection.RightToLeft}
        okButton.Text = "Add"
        okButton.Width = 90
        cancelButton.Text = "Cancel"
        cancelButton.Width = 90
        cancelButton.DialogResult = DialogResult.Cancel
        buttons.Controls.Add(cancelButton)
        buttons.Controls.Add(okButton)

        Me.Controls.Add(list)
        Me.Controls.Add(note)
        Me.Controls.Add(buttons)
        Me.Controls.Add(filter)
        Me.AcceptButton = okButton
        Me.CancelButton = cancelButton

        Fill("")
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsPickCallForm")
        End Try
    End Sub

    Private Sub Fill(needle As String)
        list.BeginUpdate()
        list.Items.Clear()
        For Each name As String In every
            If needle = "" OrElse name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 Then
                list.Items.Add(name)
            End If
        Next
        list.EndUpdate()
        If list.Items.Count > 0 Then list.SelectedIndex = 0
    End Sub

    Private Sub Filter_TextChanged(sender As Object, e As EventArgs) Handles filter.TextChanged
        Fill(filter.Text.Trim())
    End Sub

    Private Sub List_SelectedIndexChanged(sender As Object, e As EventArgs) Handles list.SelectedIndexChanged
        Dim name As String = Convert.ToString(list.SelectedItem)
        Dim known As EpsCall = EpsSymbols.Find(name)
        If known Is Nothing Then
            note.Text = ""
            Return
        End If
        Dim values As String = String.Join(", ", known.Values.Select(
            Function(v) If(v.HasList, v.Name & ": " & v.Kind, v.Name)))
        note.Text = known.Name & "(" & values & ")" &
                    If(known.Note <> "", Environment.NewLine & known.Note, "")
    End Sub

    Private Sub List_DoubleClick(sender As Object, e As EventArgs) Handles list.DoubleClick
        Choose()
    End Sub

    Private Sub OkButton_Click(sender As Object, e As EventArgs) Handles okButton.Click
        Choose()
    End Sub

    Private Sub Choose()
        If list.SelectedItem Is Nothing Then Return
        Picked = Convert.ToString(list.SelectedItem)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class
