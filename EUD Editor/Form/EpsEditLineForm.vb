Imports System.Text
Imports EUD_Editor.EpsSource

''' <summary>
''' Edits one line of the source, in a window of its own.
'''
''' A line the editor knows is shown as its values, each with the list that fills
''' it. A line it does not know is shown as itself, in a box, so nothing is ever
''' out of reach.
''' </summary>
Public Class EpsEditLineForm
    Inherits Form

    Private ReadOnly fields As New TableLayoutPanel()
    Private ReadOnly heading As New Label()
    Private ReadOnly note As New Label()
    Private ReadOnly preview As New TextBox()
    Private WithEvents okButton As New Button()
    Private WithEvents cancelButton As New Button()

    Private ReadOnly node As EpsNode
    Private ReadOnly known As EpsCall
    Private ReadOnly boxes As New List(Of Control)
    Private ReadOnly whole As New TextBox()
    Private filling As Boolean

    ''' <summary>The text the user settled on.</summary>
    Public Property Result As String = ""

    ''' <summary>Edits one node. Gives back True when something changed.</summary>
    Public Shared Function Edit(owner As IWin32Window, node As EpsNode) As Boolean
        Using dialog As New EpsEditLineForm(node)
            If dialog.ShowDialog(owner) <> DialogResult.OK Then Return False
            If dialog.Result = node.Text Then Return False
            node.Text = dialog.Result
            Return True
        End Using
    End Function

    Public Sub New(node As EpsNode)
        Me.node = node
        Me.known = EpsSymbols.Find(EpsLines.CallOf(node.Text))

        Me.Text = "Edit"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(560, 400)
        Me.MinimumSize = New Size(420, 300)
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowIcon = False
        Me.Padding = New Padding(10)
        Me.Font = SystemFonts.MessageBoxFont

        heading.Dock = DockStyle.Top
        heading.Height = 22
        heading.Font = New Font(Me.Font, FontStyle.Bold)

        note.Dock = DockStyle.Top
        note.Height = 34

        fields.Dock = DockStyle.Fill
        fields.ColumnCount = 2
        fields.AutoScroll = True
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        preview.Dock = DockStyle.Bottom
        preview.Height = 54
        preview.Multiline = True
        preview.ReadOnly = True
        preview.Font = New Font("Consolas", 9.0F)

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 38,
                                                 .FlowDirection = FlowDirection.RightToLeft}
        okButton.Text = "OK"
        okButton.Width = 90
        cancelButton.Text = "Cancel"
        cancelButton.Width = 90
        cancelButton.DialogResult = DialogResult.Cancel
        buttons.Controls.Add(cancelButton)
        buttons.Controls.Add(okButton)

        Me.Controls.Add(fields)
        Me.Controls.Add(preview)
        Me.Controls.Add(buttons)
        Me.Controls.Add(note)
        Me.Controls.Add(heading)
        Me.AcceptButton = okButton
        Me.CancelButton = cancelButton

        Build()
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm")
        End Try
    End Sub

    Private Sub Build()
        filling = True

        If node.Kind = EpsKind.Folder Then
            heading.Text = "Folder"
            note.Text = "A folder is the editor's own idea, kept in a comment."
            AddWhole(node.Text)
        ElseIf known IsNot Nothing Then
            heading.Text = known.Name
            note.Text = If(known.Note <> "", known.Note, "from the " & known.Source)
            Dim values As List(Of String) = EpsLines.ValuesOf(node.Text)
            For i = 0 To known.Values.Count - 1
                Dim value As EpsValue = known.Values(i)
                AddValue(value, If(i < values.Count, values(i), EpsLines.DefaultFor(value)))
            Next
            If known.Values.Count = 0 Then
                note.Text &= "  This call takes nothing."
            End If
        Else
            heading.Text = If(node.Kind = EpsKind.Block, "Block", "Line")
            note.Text = "The editor has no description of this one, so it is edited as it stands."
            AddWhole(node.Text)
        End If

        filling = False
        ShowPreview()
    End Sub

    Private Sub AddWhole(text As String)
        whole.Text = text
        whole.Multiline = True
        whole.Dock = DockStyle.Fill
        whole.Height = 90
        whole.Font = New Font("Consolas", 9.0F)
        AddHandler whole.TextChanged, AddressOf Anything_Changed
        fields.Controls.Add(whole, 0, fields.RowCount)
        fields.SetColumnSpan(whole, 2)
        fields.RowCount += 1
    End Sub

    Private Sub AddValue(value As EpsValue, now As String)
        Dim label As String = value.Name
        If value.HasList Then label &= "  [" & value.Kind & "]"
        Dim shown As New Label With {.Text = label, .AutoSize = True, .Margin = New Padding(3, 7, 3, 3)}
        fields.Controls.Add(shown, 0, fields.RowCount)

        Dim options As List(Of String) = EpsValueLists.For_(value.Kind)
        Dim box As Control
        If options IsNot Nothing AndAlso options.Count > 0 Then
            Dim combo As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown}
            combo.Items.AddRange(options.ToArray())
            combo.Text = now
            AddHandler combo.TextChanged, AddressOf Anything_Changed
            box = combo
        Else
            Dim text As New TextBox With {.Text = now, .Dock = DockStyle.Fill}
            AddHandler text.TextChanged, AddressOf Anything_Changed
            box = text
        End If
        boxes.Add(box)
        fields.Controls.Add(box, 1, fields.RowCount)
        fields.RowCount += 1
    End Sub

    Private Sub Anything_Changed(sender As Object, e As EventArgs)
        If Not filling Then ShowPreview()
    End Sub

    Private Function Compose() As String
        If node.Kind = EpsKind.Folder OrElse known Is Nothing Then Return whole.Text.Trim()

        Dim values As New List(Of String)
        For Each box As Control In boxes
            values.Add(box.Text.Trim())
        Next
        Dim ends As String = If(node.Text.TrimEnd().EndsWith(";"), ";", "")
        Return known.Name & "(" & String.Join(", ", values) & ")" & ends
    End Function

    Private Sub ShowPreview()
        preview.Text = Compose()
    End Sub

    Private Sub OkButton_Click(sender As Object, e As EventArgs) Handles okButton.Click
        Result = Compose()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class
