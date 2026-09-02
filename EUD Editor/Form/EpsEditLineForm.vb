Imports EUD_Editor.EpsSource

''' <summary>
''' Edits one node of the source, laid out the way the old editor lays out an
''' action or a condition.
'''
''' At the top, which call it is. Under that, what it does said as a sentence,
''' with each value standing in it and able to be clicked. Under that, a tab for
''' each value, and for the value picked, the ways of giving it: the list it comes
''' from, a variable of this source, or something written by hand.
'''
''' A construct that is not a call - a function, a for, a folder - is shown as the
''' parts it is made of instead. Only a head with no shape at all falls back to
''' its own spelling.
''' </summary>
Public Class EpsEditLineForm
    Inherits Form

    'The top of the window: which call, and what it does.
    Private WithEvents whichCall As New ComboBox()
    Private ReadOnly sentence As New FlowLayoutPanel()

    'The middle: a tab for each value, and the ways of giving the one picked.
    Private WithEvents valueTabs As New TabControl()
    Private WithEvents modeTabs As New TabControl()
    Private WithEvents filter As New TextBox()
    Private WithEvents choices As New ListBox()
    Private WithEvents typed As New TextBox()

    'For a construct that is not a call.
    Private ReadOnly rows As New TableLayoutPanel()

    Private ReadOnly preview As New TextBox()
    Private WithEvents okButton As New Button()
    Private WithEvents cancelButton As New Button()

    Private ReadOnly node As EpsNode
    Private ReadOnly shape As EpsShape
    Private ReadOnly variables As List(Of String)

    Private known As EpsCall
    Private ReadOnly values As New List(Of String)
    Private at As Integer                      'which value is being given
    Private filling As Boolean

    'For a construct.
    Private headName As String = ""
    Private ReadOnly arguments As New List(Of String)
    Private forParts As EpsHead.ForParts
    Private ReadOnly plain As New TextBox()

    Private Const ModeDefault As Integer = 0
    Private Const ModeVariable As Integer = 1
    Private Const ModeCustom As Integer = 2

    Public Property Result As String = ""

    ''' <summary>Edits one node. Gives back True when something changed.</summary>
    Public Shared Function Edit(owner As IWin32Window, node As EpsNode,
                                Optional root As EpsNode = Nothing) As Boolean
        Using dialog As New EpsEditLineForm(node, root)
            If dialog.ShowDialog(owner) <> DialogResult.OK Then Return False
            If dialog.Result = node.Text Then Return False
            node.Text = dialog.Result
            Return True
        End Using
    End Function

    Public Sub New(node As EpsNode, Optional root As EpsNode = Nothing)
        Me.node = node
        Me.shape = EpsHead.ShapeOf(node)
        Me.variables = EpsLines.VariablesIn(root)

        Me.Text = If(shape = EpsShape.Test OrElse node.Kind = EpsKind.Statement,
                     "Edit", "Edit")
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(520, 620)
        Me.MinimumSize = New Size(440, 480)
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowIcon = False
        Me.Padding = New Padding(8)
        Me.Font = SystemFonts.MessageBoxFont

        BuildLayout()
        Read_()
        DrawAll()
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm")
        End Try
    End Sub

#Region "How it is put together"
    Private Sub BuildLayout()
        whichCall.Dock = DockStyle.Top
        whichCall.DropDownStyle = ComboBoxStyle.DropDown
        whichCall.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        whichCall.AutoCompleteSource = AutoCompleteSource.ListItems

        sentence.Dock = DockStyle.Top
        sentence.Height = 62
        sentence.AutoScroll = True
        sentence.Padding = New Padding(2, 4, 2, 4)

        valueTabs.Dock = DockStyle.Top
        valueTabs.Height = 26
        valueTabs.ItemSize = New Size(0, 20)

        filter.Dock = DockStyle.Top

        choices.Dock = DockStyle.Fill
        choices.IntegralHeight = False

        typed.Dock = DockStyle.Fill
        typed.Multiline = True
        typed.Font = New Font("Consolas", 9.0F)

        'The list, the variables and what is written by hand share one area, the
        'way the old editor shares one.
        Dim listPanel As New Panel With {.Dock = DockStyle.Fill}
        listPanel.Controls.Add(choices)
        listPanel.Controls.Add(filter)

        modeTabs.Dock = DockStyle.Fill
        modeTabs.TabPages.Add(New TabPage("Default"))
        modeTabs.TabPages.Add(New TabPage("Variable"))
        modeTabs.TabPages.Add(New TabPage("Custom"))
        modeTabs.TabPages(ModeDefault).Controls.Add(listPanel)
        modeTabs.TabPages(ModeCustom).Controls.Add(typed)

        rows.Dock = DockStyle.Fill
        rows.ColumnCount = 3
        rows.AutoScroll = True
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        rows.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 30))
        rows.Visible = False

        preview.Dock = DockStyle.Bottom
        preview.Height = 46
        preview.Multiline = True
        preview.ReadOnly = True
        preview.Font = New Font("Consolas", 9.0F)

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 38,
                                                 .FlowDirection = FlowDirection.RightToLeft}
        okButton.Text = "Ok"
        okButton.Width = 100
        cancelButton.Text = "Cancel"
        cancelButton.Width = 100
        cancelButton.DialogResult = DialogResult.Cancel
        buttons.Controls.Add(cancelButton)
        buttons.Controls.Add(okButton)

        Dim middle As New Panel With {.Dock = DockStyle.Fill}
        middle.Controls.Add(modeTabs)
        middle.Controls.Add(rows)
        middle.Controls.Add(valueTabs)

        Me.Controls.Add(middle)
        Me.Controls.Add(preview)
        Me.Controls.Add(buttons)
        Me.Controls.Add(sentence)
        Me.Controls.Add(whichCall)
        Me.AcceptButton = okButton
        Me.CancelButton = cancelButton
    End Sub
#End Region

#Region "Taking the node apart"
    Private Sub Read_()
        Select Case shape
            Case EpsShape.Call_
                known = EpsSymbols.Find(EpsLines.CallOf(node.Text))
                values.AddRange(EpsLines.ValuesOf(node.Text))

            Case EpsShape.Function_
                headName = EpsHead.FunctionName(node.Text)
                arguments.AddRange(EpsHead.FunctionArguments(node.Text))

            Case EpsShape.For_
                forParts = EpsHead.ForOf(node.Text)
        End Select
        FillOutValues()
    End Sub

    'Every value a call takes has something in it, even a new one.
    Private Sub FillOutValues()
        If known Is Nothing Then Return
        While values.Count < known.Values.Count
            values.Add(EpsLines.DefaultFor(known.Values(values.Count)))
        End While
        While values.Count > known.Values.Count
            values.RemoveAt(values.Count - 1)
        End While
    End Sub
#End Region

#Region "Drawing"
    Private Sub DrawAll()
        filling = True

        Dim isCall As Boolean = (shape = EpsShape.Call_)
        whichCall.Visible = isCall
        sentence.Visible = isCall
        valueTabs.Visible = isCall
        modeTabs.Visible = isCall
        rows.Visible = Not isCall

        If isCall Then
            If whichCall.Items.Count = 0 Then whichCall.Items.AddRange(EpsSymbols.Names().ToArray())
            whichCall.Text = If(known Is Nothing, EpsLines.CallOf(node.Text), known.Name)
            DrawSentence()
            DrawValueTabs()
        Else
            DrawParts()
        End If

        filling = False
        ShowPreview()
    End Sub

    'What the call does, with each value standing in the words and able to be
    'clicked to go to it.
    Private Sub DrawSentence()
        sentence.Controls.Clear()
        If known Is Nothing Then
            sentence.Controls.Add(New Label With {.Text = "The editor has no description of this call.",
                                                  .AutoSize = True, .Margin = New Padding(0, 3, 0, 0)})
            Return
        End If

        For Each piece In EpsLines.Describe(known, values)
            Dim which As Integer = piece.Item2
            If which < 0 Then
                sentence.Controls.Add(New Label With {.Text = piece.Item1, .AutoSize = True,
                                                      .Margin = New Padding(0, 3, 0, 0)})
                Continue For
            End If

            Dim link As New LinkLabel With {.Text = piece.Item1, .AutoSize = True,
                                            .Margin = New Padding(0, 3, 0, 0), .Tag = which}
            AddHandler link.LinkClicked, Sub()
                                             If which < valueTabs.TabCount Then valueTabs.SelectedIndex = which
                                         End Sub
            sentence.Controls.Add(link)
        Next
    End Sub

    Private Sub DrawValueTabs()
        valueTabs.TabPages.Clear()
        If known Is Nothing Then
            modeTabs.Visible = False
            Return
        End If

        modeTabs.Visible = known.Values.Count > 0
        For i = 0 To known.Values.Count - 1
            valueTabs.TabPages.Add(New TabPage("1." & known.Values(i).Name))
        Next
        If valueTabs.TabCount > 0 Then
            at = Math.Min(Math.Max(at, 0), valueTabs.TabCount - 1)
            valueTabs.SelectedIndex = at
            DrawValue()
        End If
    End Sub

    'The ways of giving the value that is picked.
    Private Sub DrawValue()
        If known Is Nothing OrElse at < 0 OrElse at >= known.Values.Count Then Return
        Dim was As Boolean = filling
        filling = True

        Dim value As EpsValue = known.Values(at)
        Dim now As String = If(at < values.Count, values(at), "")
        Dim options As List(Of String) = EpsValueLists.For_(value.Kind)

        modeTabs.TabPages(ModeDefault).Text = If(value.HasList, value.Kind, "Default")
        choices.BeginUpdate()
        choices.Items.Clear()
        If options IsNot Nothing Then
            For Each choice As String In options
                If filter.Text.Trim() = "" OrElse
                   choice.IndexOf(filter.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0 Then
                    choices.Items.Add(choice)
                End If
            Next
        End If
        choices.EndUpdate()

        'The list shows a person "Player 1" where the code says 1, so the written
        'value is asked what choice it stands for before it is looked for.
        Dim stands As String = EpsValueLists.ChoiceFor(value.Kind, now)
        If stands <> "" AndAlso choices.Items.Contains(stands) Then choices.SelectedItem = stands

        typed.Text = now

        'A value with no list of its own is written by hand, so that is where it
        'opens; one that stands for a choice opens on the list.
        If options Is Nothing OrElse options.Count = 0 Then
            modeTabs.SelectedIndex = ModeCustom
        ElseIf variables.Contains(now) Then
            modeTabs.SelectedIndex = ModeVariable
        ElseIf stands = "" AndAlso now <> "" Then
            modeTabs.SelectedIndex = ModeCustom
        Else
            modeTabs.SelectedIndex = ModeDefault
        End If

        DrawVariables(now)
        filling = was
    End Sub

    Private Sub DrawVariables(now As String)
        Dim page As TabPage = modeTabs.TabPages(ModeVariable)
        page.Controls.Clear()
        Dim list As New ListBox With {.Dock = DockStyle.Fill, .IntegralHeight = False}
        If variables.Count = 0 Then
            page.Controls.Add(New Label With {.Dock = DockStyle.Fill,
                                              .Text = "This source declares no variables yet."})
            Return
        End If
        list.Items.AddRange(variables.ToArray())
        If variables.Contains(now) Then list.SelectedItem = now
        AddHandler list.SelectedIndexChanged, Sub()
                                                  If filling OrElse list.SelectedItem Is Nothing Then Return
                                                  Take(Convert.ToString(list.SelectedItem))
                                              End Sub
        page.Controls.Add(list)
        Try
            ThemeSetForm.SetControlColor(list)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm.DrawVariables")
        End Try
    End Sub

    Private Sub Take(text As String)
        If at < 0 OrElse at >= values.Count Then Return
        values(at) = text
        DrawSentence()
        ShowPreview()
    End Sub
#End Region

#Region "A construct, which is its parts"
    Private Sub DrawParts()
        rows.Controls.Clear()
        rows.RowStyles.Clear()
        rows.RowCount = 0

        Select Case shape
            Case EpsShape.Function_
                AddRow("Name", headName, Sub(text) headName = text)
                For index = 0 To arguments.Count - 1
                    Dim slot As Integer = index
                    AddRow("Argument " & (slot + 1), arguments(slot),
                           Sub(text) arguments(slot) = text,
                           Sub()
                               arguments.RemoveAt(slot)
                               DrawParts()
                               ShowPreview()
                           End Sub)
                Next
                AddButton("Add an argument", Sub()
                                                 arguments.Add("value" & (arguments.Count + 1))
                                                 DrawParts()
                                                 ShowPreview()
                                             End Sub)

            Case EpsShape.For_
                AddRow("Variable", forParts.Variable, Sub(text) forParts.Variable = text)
                AddRow("From", forParts.From, Sub(text) forParts.From = text)
                AddChoice("While it is", forParts.Comparison,
                          New String() {"<", "<=", ">", ">=", "!=", "=="},
                          Sub(text) forParts.Comparison = text)
                AddRow("This", forParts.Until, Sub(text) forParts.Until = text)
                AddChoice("Each time", forParts.Step_,
                          New String() {forParts.Variable & "++", forParts.Variable & "--"},
                          Sub(text) forParts.Step_ = text)

            Case EpsShape.Folder
                headName = node.Text
                AddRow("Name", headName, Sub(text) headName = text)

            Case EpsShape.Plain
                rows.Controls.Add(New Label With {.Text = "There is nothing to fill in here.",
                                                  .AutoSize = True}, 0, 0)

            Case Else
                plain.Text = node.Text
                plain.Multiline = True
                plain.Dock = DockStyle.Fill
                plain.Height = 160
                plain.Font = New Font("Consolas", 9.0F)
                AddHandler plain.TextChanged, Sub()
                                                  If Not filling Then ShowPreview()
                                              End Sub
                rows.Controls.Add(plain, 0, 0)
                rows.SetColumnSpan(plain, 3)
        End Select

        Try
            ThemeSetForm.SetControlColor(rows)
        Catch ex As Exception
            LogSuppressed(ex, "EpsEditLineForm.DrawParts")
        End Try
    End Sub

    Private Sub AddRow(label As String, now As String, apply As System.Action(Of String),
                       Optional remove As System.Action = Nothing)
        rows.Controls.Add(New Label With {.Text = label, .AutoSize = True,
                                          .Margin = New Padding(3, 7, 3, 3)}, 0, rows.RowCount)
        Dim box As New TextBox With {.Text = now, .Dock = DockStyle.Fill}
        AddHandler box.TextChanged, Sub()
                                        If filling Then Return
                                        apply(box.Text)
                                        ShowPreview()
                                    End Sub
        rows.Controls.Add(box, 1, rows.RowCount)
        If remove IsNot Nothing Then
            Dim button As New Button With {.Text = "-", .Width = 24, .Height = 22}
            AddHandler button.Click, Sub() remove()
            rows.Controls.Add(button, 2, rows.RowCount)
        End If
        rows.RowCount += 1
    End Sub

    Private Sub AddChoice(label As String, now As String, options As String(),
                          apply As System.Action(Of String))
        rows.Controls.Add(New Label With {.Text = label, .AutoSize = True,
                                          .Margin = New Padding(3, 7, 3, 3)}, 0, rows.RowCount)
        Dim box As New ComboBox With {.Dock = DockStyle.Fill}
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

    Private Sub AddButton(label As String, pressed As System.Action)
        Dim button As New Button With {.Text = label, .AutoSize = True}
        AddHandler button.Click, Sub() pressed()
        rows.Controls.Add(button, 1, rows.RowCount)
        rows.RowCount += 1
    End Sub
#End Region

#Region "What the user does"
    Private Sub WhichCall_Changed(sender As Object, e As EventArgs) Handles whichCall.SelectionChangeCommitted
        If filling Then Return
        Dim picked As EpsCall = EpsSymbols.Find(whichCall.Text.Trim())
        If picked Is Nothing Then Return
        known = picked
        values.Clear()
        at = 0
        FillOutValues()
        DrawAll()
    End Sub

    Private Sub ValueTabs_Changed(sender As Object, e As EventArgs) Handles valueTabs.SelectedIndexChanged
        If filling Then Return
        at = valueTabs.SelectedIndex
        DrawValue()
    End Sub

    Private Sub Filter_Changed(sender As Object, e As EventArgs) Handles filter.TextChanged
        If filling Then Return
        DrawValue()
    End Sub

    Private Sub Choices_Changed(sender As Object, e As EventArgs) Handles choices.SelectedIndexChanged
        If filling OrElse choices.SelectedItem Is Nothing Then Return
        If known Is Nothing OrElse at < 0 OrElse at >= known.Values.Count Then Return
        Take(EpsValueLists.CodeFor(known.Values(at).Kind, Convert.ToString(choices.SelectedItem)))
    End Sub

    Private Sub Typed_Changed(sender As Object, e As EventArgs) Handles typed.TextChanged
        If filling OrElse modeTabs.SelectedIndex <> ModeCustom Then Return
        Take(typed.Text.Trim())
    End Sub
#End Region

#Region "Putting it back together"
    Private Function Compose() As String
        Select Case shape
            Case EpsShape.Call_
                If known Is Nothing Then Return node.Text
                Dim ends As String = If(node.Kind = EpsKind.Statement, ";", "")
                Return known.Name & "(" & String.Join(", ", values) & ")" & ends
            Case EpsShape.Function_
                Return EpsHead.ComposeFunction(headName, arguments)
            Case EpsShape.For_
                Return EpsHead.ComposeFor(forParts)
            Case EpsShape.Folder
                Return headName
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
