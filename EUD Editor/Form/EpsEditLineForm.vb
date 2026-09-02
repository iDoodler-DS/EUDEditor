Imports EUD_Editor.EpsSource

''' <summary>
''' Edits one node of the source, in a window of its own.
'''
''' Nothing here is edited as text if the editor knows what it is. A call is its
''' values. A function is a name and its arguments. An if, an else if and a while
''' are the conditions they hold, each of those being a call again, so a condition
''' is picked from a list and filled in from lists. A for counts, so it is the
''' four things it counts with.
'''
''' Only a head the editor has no shape for is shown as its own spelling, and even
''' then it is shown, never hidden.
''' </summary>
Public Class EpsEditLineForm
    Inherits Form

    Private ReadOnly rows As New TableLayoutPanel()
    Private ReadOnly heading As New Label()
    Private ReadOnly note As New Label()
    Private ReadOnly preview As New TextBox()
    Private WithEvents okButton As New Button()
    Private WithEvents cancelButton As New Button()

    Private ReadOnly node As EpsNode
    Private ReadOnly shape As EpsShape
    Private filling As Boolean

    'What the shape being edited is made of.
    Private callName As String = ""
    Private ReadOnly values As New List(Of Control)
    Private ReadOnly terms As New List(Of Term)
    Private ReadOnly arguments As New List(Of String)
    Private forParts As EpsHead.ForParts
    Private ReadOnly plain As New TextBox()

    ''' <summary>One condition of a test: which call, and what it was given.</summary>
    Private Class Term
        Public Property Name As String = ""
        Public Property Text As String = ""
        Public ReadOnly Values As New List(Of String)
    End Class

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
        Me.shape = EpsHead.ShapeOf(node)

        Me.Text = "Edit"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(620, 460)
        Me.MinimumSize = New Size(480, 340)
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

        rows.Dock = DockStyle.Fill
        rows.ColumnCount = 3
        rows.AutoScroll = True
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 30))

        preview.Dock = DockStyle.Bottom
        preview.Height = 56
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

        Me.Controls.Add(rows)
        Me.Controls.Add(preview)
        Me.Controls.Add(buttons)
        Me.Controls.Add(note)
        Me.Controls.Add(heading)
        Me.AcceptButton = okButton
        Me.CancelButton = cancelButton

        Read_()
        Draw()
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm")
        End Try
    End Sub

#Region "Taking the node apart"
    Private Sub Read_()
        Select Case shape
            Case EpsShape.Call_
                callName = EpsLines.CallOf(node.Text)

            Case EpsShape.Test
                For Each text As String In EpsHead.TermsOf(node.Text)
                    Dim term As New Term With {.Text = text, .Name = EpsLines.CallOf(text)}
                    term.Values.AddRange(EpsLines.ValuesOf(text))
                    terms.Add(term)
                Next
                If terms.Count = 0 Then terms.Add(New Term With {.Name = "Always", .Text = "Always()"})

            Case EpsShape.Function_
                callName = EpsHead.FunctionName(node.Text)
                arguments.AddRange(EpsHead.FunctionArguments(node.Text))

            Case EpsShape.For_
                forParts = EpsHead.ForOf(node.Text)
        End Select
    End Sub
#End Region

#Region "Drawing it"
    Private Sub Draw()
        filling = True
        rows.SuspendLayout()
        rows.Controls.Clear()
        rows.RowStyles.Clear()
        rows.RowCount = 0
        values.Clear()

        Select Case shape
            Case EpsShape.Call_ : DrawCall()
            Case EpsShape.Test : DrawTest()
            Case EpsShape.Function_ : DrawFunction()
            Case EpsShape.For_ : DrawFor()
            Case EpsShape.Folder : DrawFolder()
            Case EpsShape.Plain : DrawPlain()
            Case Else : DrawRaw()
        End Select

        rows.ResumeLayout()

        'The rows are made again whenever a pick changes what they should be, so
        'the new ones are given the theme here rather than once at the start.
        Try
            ThemeSetForm.SetControlColor(rows)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm.Draw")
        End Try

        filling = False
        ShowPreview()
    End Sub

    Private Sub DrawCall()
        heading.Text = If(callName <> "", callName, "Line")
        Dim known As EpsCall = EpsSymbols.Find(callName)
        note.Text = If(known Is Nothing, "Pick what this line should call.",
                       If(known.Note <> "", known.Note, "from the " & known.Source))

        AddPicker("Call", callName, Sub(picked)
                                        callName = picked
                                        Draw()
                                    End Sub)

        If known Is Nothing Then Return
        Dim now As List(Of String) = EpsLines.ValuesOf(node.Text)
        Dim same As Boolean = String.Equals(EpsLines.CallOf(node.Text), callName, StringComparison.OrdinalIgnoreCase)
        For i = 0 To known.Values.Count - 1
            Dim value As EpsValue = known.Values(i)
            Dim text As String = If(same AndAlso i < now.Count, now(i), EpsLines.DefaultFor(value))
            values.Add(AddValue(value, text))
        Next
        If known.Values.Count = 0 Then AddNote("This call takes nothing.")
    End Sub

    Private Sub DrawTest()
        heading.Text = EpsHead.KeywordOf(node.Text)
        note.Text = "Every condition here must hold. Pick one, and fill it in."

        For index = 0 To terms.Count - 1
            Dim at As Integer = index
            Dim term As Term = terms(at)

            AddPicker("Condition " & (at + 1), term.Name,
                      Sub(picked)
                          term.Name = picked
                          term.Values.Clear()
                          term.Text = ""
                          Draw()
                      End Sub,
                      Sub()
                          terms.RemoveAt(at)
                          If terms.Count = 0 Then terms.Add(New Term With {.Name = "Always", .Text = "Always()"})
                          Draw()
                      End Sub)

            Dim known As EpsCall = EpsSymbols.Find(term.Name)
            If known Is Nothing Then
                AddFreeText("    is", term.Text, Sub(text) term.Text = text)
                Continue For
            End If

            For i = 0 To known.Values.Count - 1
                Dim value As EpsValue = known.Values(i)
                Dim slot As Integer = i
                While term.Values.Count <= slot
                    term.Values.Add(EpsLines.DefaultFor(known.Values(term.Values.Count)))
                End While
                AddValue(value, term.Values(slot), Sub(text) term.Values(slot) = text, "    ")
            Next
        Next

        AddButtonRow("Add a condition", Sub()
                                            terms.Add(New Term With {.Name = "Always", .Text = "Always()"})
                                            Draw()
                                        End Sub)
    End Sub

    Private Sub DrawFunction()
        heading.Text = "Function"
        note.Text = "A name, and the arguments it takes."

        AddFreeText("Name", callName, Sub(text) callName = text)

        For index = 0 To arguments.Count - 1
            Dim at As Integer = index
            AddFreeText("Argument " & (at + 1), arguments(at),
                        Sub(text) arguments(at) = text,
                        Sub()
                            arguments.RemoveAt(at)
                            Draw()
                        End Sub)
        Next

        AddButtonRow("Add an argument", Sub()
                                            arguments.Add("value" & (arguments.Count + 1))
                                            Draw()
                                        End Sub)
    End Sub

    Private Sub DrawFor()
        heading.Text = "For"
        note.Text = "Counts from one number to another."

        AddFreeText("Variable", forParts.Variable, Sub(text) forParts.Variable = text)
        AddFreeText("From", forParts.From, Sub(text) forParts.From = text)
        AddChoice("While it is", forParts.Comparison,
                  New String() {"<", "<=", ">", ">=", "!=", "=="},
                  Sub(text) forParts.Comparison = text)
        AddFreeText("This", forParts.Until, Sub(text) forParts.Until = text)
        AddChoice("Each time", forParts.Step_,
                  New String() {forParts.Variable & "++", forParts.Variable & "--",
                                forParts.Variable & " += 2"},
                  Sub(text) forParts.Step_ = text)
    End Sub

    Private Sub DrawFolder()
        heading.Text = "Folder"
        note.Text = "A folder is the editor's own idea, kept in a comment."
        AddFreeText("Name", node.Text, Sub(text) plain.Tag = text)
        plain.Tag = node.Text
    End Sub

    Private Sub DrawPlain()
        heading.Text = node.Text.Trim()
        note.Text = "There is nothing to fill in here."
    End Sub

    Private Sub DrawRaw()
        heading.Text = If(node.Kind = EpsKind.Block, "Block", "Line")
        note.Text = "The editor has no shape for this one, so it is edited as it stands."
        plain.Text = node.Text
        plain.Multiline = True
        plain.Dock = DockStyle.Fill
        plain.Height = 120
        plain.Font = New Font("Consolas", 9.0F)
        AddHandler plain.TextChanged, AddressOf Anything_Changed
        rows.Controls.Add(plain, 0, rows.RowCount)
        rows.SetColumnSpan(plain, 3)
        rows.RowCount += 1
    End Sub
#End Region

#Region "The pieces a row is made of"
    Private Sub AddNote(text As String)
        Dim shown As New Label With {.Text = text, .AutoSize = True, .Margin = New Padding(3, 6, 3, 3)}
        rows.Controls.Add(shown, 0, rows.RowCount)
        rows.SetColumnSpan(shown, 3)
        rows.RowCount += 1
    End Sub

    Private Sub AddLabel(text As String)
        Dim shown As New Label With {.Text = text, .AutoSize = True, .Margin = New Padding(3, 7, 3, 3)}
        rows.Controls.Add(shown, 0, rows.RowCount)
    End Sub

    'A list of every call the editor knows, so a condition or a line is picked
    'rather than typed.
    Private Sub AddPicker(label As String, now As String, picked As System.Action(Of String),
                          Optional remove As System.Action = Nothing)
        AddLabel(label)
        Dim box As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown,
                                      .AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                                      .AutoCompleteSource = AutoCompleteSource.ListItems}
        box.Items.AddRange(EpsSymbols.Names().ToArray())
        box.Text = now
        AddHandler box.SelectionChangeCommitted, Sub()
                                                     If Not filling Then picked(box.Text.Trim())
                                                 End Sub
        AddHandler box.Leave, Sub()
                                  If Not filling AndAlso box.Text.Trim() <> now Then picked(box.Text.Trim())
                              End Sub
        rows.Controls.Add(box, 1, rows.RowCount)
        If remove IsNot Nothing Then AddRemove(remove)
        rows.RowCount += 1
    End Sub

    Private Function AddValue(value As EpsValue, now As String,
                              Optional apply As System.Action(Of String) = Nothing,
                              Optional indent As String = "") As Control
        Dim label As String = indent & value.Name
        If value.HasList Then label &= "  [" & value.Kind & "]"
        AddLabel(label)

        Dim options As List(Of String) = EpsValueLists.For_(value.Kind)
        Dim box As Control
        If options IsNot Nothing AndAlso options.Count > 0 Then
            Dim combo As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown,
                                            .AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                                            .AutoCompleteSource = AutoCompleteSource.ListItems}
            combo.Items.AddRange(options.ToArray())
            combo.Text = now
            AddHandler combo.TextChanged, Sub()
                                              If filling Then Return
                                              If apply IsNot Nothing Then apply(combo.Text)
                                              ShowPreview()
                                          End Sub
            box = combo
        Else
            Dim text As New TextBox With {.Text = now, .Dock = DockStyle.Fill}
            AddHandler text.TextChanged, Sub()
                                             If filling Then Return
                                             If apply IsNot Nothing Then apply(text.Text)
                                             ShowPreview()
                                         End Sub
            box = text
        End If
        rows.Controls.Add(box, 1, rows.RowCount)
        rows.RowCount += 1
        Return box
    End Function

    Private Sub AddFreeText(label As String, now As String, apply As System.Action(Of String),
                            Optional remove As System.Action = Nothing)
        AddLabel(label)
        Dim box As New TextBox With {.Text = now, .Dock = DockStyle.Fill}
        AddHandler box.TextChanged, Sub()
                                        If filling Then Return
                                        apply(box.Text)
                                        ShowPreview()
                                    End Sub
        rows.Controls.Add(box, 1, rows.RowCount)
        If remove IsNot Nothing Then AddRemove(remove)
        rows.RowCount += 1
    End Sub

    Private Sub AddChoice(label As String, now As String, options As String(), apply As System.Action(Of String))
        AddLabel(label)
        Dim box As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown}
        box.Items.AddRange(options)
        box.Text = now
        AddHandler box.TextChanged, Sub()
                                        If filling Then Return
                                        apply(box.Text)
                                        ShowPreview()
                                    End Sub
        rows.Controls.Add(box, 1, rows.RowCount)
        rows.RowCount += 1
    End Sub

    Private Sub AddRemove(remove As System.Action)
        Dim button As New Button With {.Text = "-", .Width = 24, .Height = 22, .Margin = New Padding(3)}
        AddHandler button.Click, Sub() remove()
        rows.Controls.Add(button, 2, rows.RowCount)
    End Sub

    Private Sub AddButtonRow(label As String, pressed As System.Action)
        Dim button As New Button With {.Text = label, .AutoSize = True, .Margin = New Padding(3, 6, 3, 3)}
        AddHandler button.Click, Sub() pressed()
        rows.Controls.Add(button, 1, rows.RowCount)
        rows.RowCount += 1
    End Sub

    Private Sub Anything_Changed(sender As Object, e As EventArgs)
        If Not filling Then ShowPreview()
    End Sub
#End Region

#Region "Putting it back together"
    Private Function Compose() As String
        Select Case shape
            Case EpsShape.Call_
                Dim known As EpsCall = EpsSymbols.Find(callName)
                If known Is Nothing Then Return If(callName = "", node.Text, callName & "();")
                Dim given As New List(Of String)
                For Each box As Control In values
                    given.Add(box.Text.Trim())
                Next
                Dim ends As String = If(node.Text.TrimEnd().EndsWith(";"), ";", ";")
                Return known.Name & "(" & String.Join(", ", given) & ")" & ends

            Case EpsShape.Test
                Dim written As New List(Of String)
                For Each term As Term In terms
                    Dim known As EpsCall = EpsSymbols.Find(term.Name)
                    If known Is Nothing Then
                        written.Add(If(term.Text.Trim() <> "", term.Text.Trim(),
                                       If(term.Name = "", "", term.Name & "()")))
                    Else
                        Dim given As New List(Of String)
                        For i = 0 To known.Values.Count - 1
                            given.Add(If(i < term.Values.Count, term.Values(i),
                                         EpsLines.DefaultFor(known.Values(i))))
                        Next
                        written.Add(known.Name & "(" & String.Join(", ", given) & ")")
                    End If
                Next
                Return EpsHead.ComposeTest(EpsHead.KeywordOf(node.Text), written)

            Case EpsShape.Function_
                Return EpsHead.ComposeFunction(callName, arguments)

            Case EpsShape.For_
                Return EpsHead.ComposeFor(forParts)

            Case EpsShape.Folder
                Return Convert.ToString(If(plain.Tag, node.Text))

            Case EpsShape.Plain
                Return node.Text

            Case Else
                Return plain.Text.Trim()
        End Select
    End Function

    Private Sub ShowPreview()
        preview.Text = Compose()
    End Sub

    Private Sub OkButton_Click(sender As Object, e As EventArgs) Handles okButton.Click
        Result = Compose()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
#End Region
End Class
