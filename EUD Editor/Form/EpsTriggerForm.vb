Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports EUD_Editor.EpsSource

''' <summary>
''' A trigger editor whose source is epScript.
'''
''' The editor of today keeps a tree of nodes and writes epScript out of it. This
''' one keeps the epScript, and the tree is a way of looking at it. Nothing is
''' lost in the round trip, because nothing is converted: a line the editor cannot
''' draw is still a line, spelled the way it was written.
'''
''' It is built in code rather than in the designer, because every part of it is
''' made from the source that is open.
''' </summary>
Public Class EpsTriggerForm
    Inherits Form

    Private WithEvents tree As New TreeView()
    Private ReadOnly source As New TextBox()
    Private ReadOnly fields As New TableLayoutPanel()
    Private ReadOnly heading As New Label()
    Private ReadOnly note As New Label()
    Private ReadOnly status As New Label()

    Private WithEvents offButton As New Button()
    Private WithEvents deleteButton As New Button()
    Private WithEvents addButton As New Button()
    Private WithEvents folderButton As New Button()
    Private WithEvents buildButton As New Button()
    Private WithEvents sourceButton As New Button()

    Private ReadOnly split As New SplitContainer()
    Private ReadOnly rightSplit As New SplitContainer()

    Private root As EpsNode = New EpsNode(EpsKind.Root)
    Private chosen As EpsNode
    Private filling As Boolean
    Private dirty As Boolean

    ''' <summary>Where the source of a project is kept, beside the project file.</summary>
    Public Shared Function SourcePath(projectFile As String) As String
        If String.IsNullOrEmpty(projectFile) Then Return ""
        Return Path.Combine(Path.GetDirectoryName(projectFile),
                            Path.GetFileNameWithoutExtension(projectFile) & ".triggers.eps")
    End Function

    Public Sub New()
        Me.Text = "epScript triggers"
        Me.Font = SystemFonts.MessageBoxFont
        BuildLayout()
    End Sub

#Region "How it is put together"
    Private Sub BuildLayout()
        Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 32,
                                             .Padding = New Padding(4, 3, 4, 3)}
        For Each pair In {Tuple.Create(addButton, "Add"),
                          Tuple.Create(folderButton, "New folder"),
                          Tuple.Create(offButton, "Turn off"),
                          Tuple.Create(deleteButton, "Delete"),
                          Tuple.Create(sourceButton, "Edit as text"),
                          Tuple.Create(buildButton, "Build map")}
            pair.Item1.Text = pair.Item2
            pair.Item1.AutoSize = True
            pair.Item1.Margin = New Padding(0, 0, 6, 0)
            bar.Controls.Add(pair.Item1)
        Next

        status.AutoSize = True
        status.Margin = New Padding(12, 6, 0, 0)
        bar.Controls.Add(status)

        tree.Dock = DockStyle.Fill
        tree.HideSelection = False
        tree.FullRowSelect = True
        tree.ShowLines = True

        heading.Dock = DockStyle.Top
        heading.AutoSize = False
        heading.Height = 22
        heading.Font = New Font(Me.Font, FontStyle.Bold)
        heading.Padding = New Padding(2, 4, 2, 0)

        note.Dock = DockStyle.Top
        note.AutoSize = False
        note.Height = 32
        note.Padding = New Padding(2, 0, 2, 4)

        fields.Dock = DockStyle.Fill
        fields.ColumnCount = 2
        fields.AutoScroll = True
        fields.Padding = New Padding(2)
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        Dim right As New Panel With {.Dock = DockStyle.Fill}
        right.Controls.Add(fields)
        right.Controls.Add(note)
        right.Controls.Add(heading)

        source.Dock = DockStyle.Fill
        source.Multiline = True
        source.ScrollBars = ScrollBars.Both
        source.WordWrap = False
        source.ReadOnly = True
        source.Font = New Font("Consolas", 9.0F)

        rightSplit.Dock = DockStyle.Fill
        rightSplit.Orientation = Orientation.Horizontal
        rightSplit.Panel1.Controls.Add(right)
        rightSplit.Panel2.Controls.Add(source)

        split.Dock = DockStyle.Fill
        split.Panel1.Controls.Add(tree)
        split.Panel2.Controls.Add(rightSplit)

        Me.Controls.Add(split)
        Me.Controls.Add(bar)
    End Sub
#End Region

#Region "Opening and keeping"
    Private placed As Boolean

    Private Sub EpsTriggerForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PlaceSplitters()
        LoadFromProject()
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.Load")
        End Try
    End Sub

    Private Sub EpsTriggerForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not placed Then PlaceSplitters()
    End Sub

    'The tree takes the left half; the fields and the source share the right.
    Private Sub PlaceSplitters()
        Try
            If split.Width > 200 Then
                split.SplitterDistance = CInt(split.Width * 0.5)
                placed = True
            End If
            If rightSplit.Height > 200 Then
                rightSplit.SplitterDistance = CInt(rightSplit.Height * 0.5)
            End If
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.PlaceSplitters")
        End Try
    End Sub

    ''' <summary>Reads the source of the open project, or starts one.</summary>
    Public Sub LoadFromProject()
        Dim path As String = SourcePath(ProjectSet.filename)
        Dim text As String = ""
        If path <> "" AndAlso File.Exists(path) Then
            Try
                text = File.ReadAllText(path)
            Catch ex As Exception
                LogException(ex, "reading " & path)
            End Try
        End If
        SetSource(text)
        dirty = False
    End Sub

    ''' <summary>Writes the source beside the project. Called when the project saves.</summary>
    Public Sub SaveToProject()
        Dim path As String = SourcePath(ProjectSet.filename)
        If path = "" Then Return
        Try
            File.WriteAllText(path, EpsWriter.Write(root))
            dirty = False
        Catch ex As Exception
            LogException(ex, "writing " & path)
        End Try
    End Sub

    Public ReadOnly Property HasSource As Boolean
        Get
            Return root.Children.Count > 0
        End Get
    End Property

    Public Function CurrentSource() As String
        Return EpsWriter.Write(root)
    End Function

    Private Sub SetSource(text As String)
        root = EpsReader.Parse(text)
        RebuildTree()
        RefreshSource()
    End Sub
#End Region

#Region "The tree"
    Private Sub RebuildTree()
        Dim wasChosen As EpsNode = chosen
        tree.BeginUpdate()
        tree.Nodes.Clear()
        For Each child As EpsNode In root.Children
            tree.Nodes.Add(MakeNode(child))
        Next
        tree.ExpandAll()
        tree.EndUpdate()

        If wasChosen IsNot Nothing Then Select_(wasChosen)
        If tree.SelectedNode Is Nothing AndAlso tree.Nodes.Count > 0 Then
            tree.SelectedNode = tree.Nodes(0)
        End If
        ShowCounts()
    End Sub

    Private Function MakeNode(node As EpsNode, Optional insideOff As Boolean = False) As TreeNode
        Dim caption As String = node.Caption()
        If node.Kind = EpsKind.Comment AndAlso caption = "" Then caption = "(blank line)"

        Dim shown As New TreeNode(caption) With {.Tag = node}
        Dim off As Boolean = insideOff OrElse node.Off
        If node.Kind = EpsKind.Comment Then shown.ForeColor = Color.FromArgb(120, 160, 120)
        'A node under one that is off writes no code either, so it reads as off.
        If off Then shown.ForeColor = Color.Gray

        For Each child As EpsNode In node.Children
            shown.Nodes.Add(MakeNode(child, off))
        Next
        Return shown
    End Function

    Private Sub Select_(node As EpsNode)
        For Each shown As TreeNode In AllNodes(tree.Nodes)
            If shown.Tag Is node Then
                tree.SelectedNode = shown
                Return
            End If
        Next
    End Sub

    Private Iterator Function AllNodes(nodes As TreeNodeCollection) As IEnumerable(Of TreeNode)
        For Each shown As TreeNode In nodes
            Yield shown
            For Each child As TreeNode In AllNodes(shown.Nodes)
                Yield child
            Next
        Next
    End Function


    Private Sub Tree_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tree.AfterSelect
        chosen = TryCast(e.Node.Tag, EpsNode)
        ShowFields()
    End Sub

    Private Sub ShowCounts()
        Dim statements As Integer = 0
        Dim drawn As Integer = 0
        For Each node As EpsNode In root.Walk()
            If node.Kind <> EpsKind.Statement Then Continue For
            statements += 1
            If EpsSymbols.Find(CallOf(node.Text)) IsNot Nothing Then drawn += 1
        Next
        status.Text = String.Format("{0} lines, {1} known to the editor, {2} names in the book",
                                    statements, drawn, EpsSymbols.Count)
    End Sub
#End Region

#Region "The fields of one line"
    'The name of the call a line makes, if it makes one.
    Private Shared Function CallOf(text As String) As String
        Dim head As Match = Regex.Match(If(text, "").Trim(), "^([A-Za-z_]\w*)\s*\(")
        Return If(head.Success, head.Groups(1).Value, "")
    End Function

    'What a call was given, split on the commas that are not inside something.
    Private Shared Function ValuesOf(text As String) As List(Of String)
        Dim out As New List(Of String)
        Dim body As String = If(text, "").Trim().TrimEnd(";"c).Trim()
        Dim opened As Integer = body.IndexOf("("c)
        If opened < 0 OrElse Not body.EndsWith(")") Then Return out
        body = body.Substring(opened + 1, body.Length - opened - 2)

        Dim depth As Integer = 0
        Dim quote As Char = ChrW(0)
        Dim current As New StringBuilder()
        For Each ch As Char In body
            If quote <> ChrW(0) Then
                current.Append(ch)
                If ch = quote Then quote = ChrW(0)
                Continue For
            End If
            Select Case ch
                Case """"c, "'"c
                    quote = ch
                    current.Append(ch)
                Case "("c, "["c, "{"c
                    depth += 1
                    current.Append(ch)
                Case ")"c, "]"c, "}"c
                    depth -= 1
                    current.Append(ch)
                Case ","c
                    If depth = 0 Then
                        out.Add(current.ToString().Trim())
                        current.Clear()
                    Else
                        current.Append(ch)
                    End If
                Case Else
                    current.Append(ch)
            End Select
        Next
        If current.Length > 0 OrElse out.Count > 0 Then out.Add(current.ToString().Trim())
        Return out
    End Function

    Private Sub ShowFields()
        filling = True
        fields.SuspendLayout()
        fields.Controls.Clear()
        fields.RowStyles.Clear()

        If chosen Is Nothing Then
            heading.Text = ""
            note.Text = ""
            fields.ResumeLayout()
            filling = False
            Return
        End If

        Dim name As String = CallOf(chosen.Text)
        Dim known As EpsCall = EpsSymbols.Find(name)

        Select Case chosen.Kind
            Case EpsKind.Root : heading.Text = "The whole source"
            Case EpsKind.Folder : heading.Text = "Folder"
            Case EpsKind.Block : heading.Text = "Block"
            Case EpsKind.Comment : heading.Text = "Comment"
            Case Else : heading.Text = If(name <> "", name, "Line")
        End Select
        If chosen.Off Then heading.Text &= "  (off)"

        note.Text = If(known IsNot Nothing,
                       If(known.Note <> "", known.Note, "from the " & known.Source),
                       "This line is kept as it was written.")

        If chosen.Kind = EpsKind.Folder Then
            AddTextField("Name", chosen.Text, Sub(value)
                                                  chosen.Text = value
                                                  Changed()
                                              End Sub)
        ElseIf known IsNot Nothing Then
            Dim values As List(Of String) = ValuesOf(chosen.Text)
            For i = 0 To known.Values.Count - 1
                Dim at As Integer = i
                Dim value As EpsValue = known.Values(i)
                Dim now As String = If(i < values.Count, values(i), "")
                Dim label As String = value.Name
                If value.HasList Then label &= "  [" & value.Kind & "]"
                AddValueField(label, now, value, Sub(text)
                                                     WriteValue(known, at, text)
                                                 End Sub)
            Next
            If known.Values.Count = 0 Then AddNote("This call takes nothing.")
        Else
            AddTextField("Line", chosen.Text, Sub(value)
                                                  chosen.Text = value
                                                  Changed()
                                              End Sub)
        End If

        fields.ResumeLayout()
        filling = False
    End Sub

    Private Sub AddNote(text As String)
        Dim shown As New Label With {.Text = text, .AutoSize = True, .Margin = New Padding(3, 6, 3, 3)}
        fields.Controls.Add(shown, 0, fields.RowCount)
        fields.SetColumnSpan(shown, 2)
        fields.RowCount += 1
    End Sub

    Private Sub AddTextField(label As String, value As String, apply As Action(Of String))
        Dim shown As New Label With {.Text = label, .AutoSize = True, .Margin = New Padding(3, 6, 3, 3)}
        Dim box As New TextBox With {.Text = value, .Dock = DockStyle.Fill}
        AddHandler box.TextChanged, Sub()
                                        If Not filling Then apply(box.Text)
                                    End Sub
        fields.Controls.Add(shown, 0, fields.RowCount)
        fields.Controls.Add(box, 1, fields.RowCount)
        fields.RowCount += 1
    End Sub

    Private Sub AddValueField(label As String, value As String, kind As EpsValue, apply As Action(Of String))
        Dim shown As New Label With {.Text = label, .AutoSize = True, .Margin = New Padding(3, 6, 3, 3)}
        fields.Controls.Add(shown, 0, fields.RowCount)

        Dim options As List(Of String) = EpsValueLists.For_(kind.Kind)
        If options IsNot Nothing AndAlso options.Count > 0 Then
            Dim box As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown}
            box.Items.AddRange(options.ToArray())
            box.Text = value
            AddHandler box.TextChanged, Sub()
                                            If Not filling Then apply(box.Text)
                                        End Sub
            fields.Controls.Add(box, 1, fields.RowCount)
        Else
            Dim box As New TextBox With {.Text = value, .Dock = DockStyle.Fill}
            AddHandler box.TextChanged, Sub()
                                            If Not filling Then apply(box.Text)
                                        End Sub
            fields.Controls.Add(box, 1, fields.RowCount)
        End If
        fields.RowCount += 1
    End Sub

    'Puts one value back into the line, leaving the rest of it alone.
    Private Sub WriteValue(known As EpsCall, at As Integer, text As String)
        If chosen Is Nothing Then Return
        Dim values As List(Of String) = ValuesOf(chosen.Text)
        While values.Count < known.Values.Count
            values.Add("")
        End While
        If at >= values.Count Then Return
        values(at) = text

        Dim ends As String = If(chosen.Text.TrimEnd().EndsWith(";"), ";", "")
        chosen.Text = known.Name & "(" & String.Join(", ", values) & ")" & ends
        Changed()
    End Sub

    Private Sub Changed()
        dirty = True
        ProjectSet.saveStatus = False
        If tree.SelectedNode IsNot Nothing AndAlso chosen IsNot Nothing Then
            tree.SelectedNode.Text = chosen.Caption()
            tree.SelectedNode.ForeColor = If(chosen.Off, Color.Gray, tree.ForeColor)
        End If
        RefreshSource()
        ShowCounts()
    End Sub

    Private Sub RefreshSource()
        source.Text = EpsWriter.Write(root).Replace(vbLf, vbCrLf).Replace(vbCr & vbCr, vbCr)
    End Sub
#End Region

#Region "What the buttons do"
    Private Sub OffButton_Click(sender As Object, e As EventArgs) Handles offButton.Click
        If chosen Is Nothing OrElse chosen.Kind = EpsKind.Root Then Return
        chosen.Off = Not chosen.Off
        offButton.Text = If(chosen.Off, "Turn on", "Turn off")
        Changed()
    End Sub

    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles deleteButton.Click
        If chosen Is Nothing OrElse chosen.Parent Is Nothing Then Return
        Dim parent As EpsNode = chosen.Parent
        parent.Remove(chosen)
        chosen = parent
        dirty = True
        ProjectSet.saveStatus = False
        RebuildTree()
        RefreshSource()
    End Sub

    Private Sub AddButton_Click(sender As Object, e As EventArgs) Handles addButton.Click
        Dim picked As String = EpsPickCallForm.Ask(Me)
        If picked = "" Then Return

        Dim known As EpsCall = EpsSymbols.Find(picked)
        Dim empty As String = If(known Is Nothing, picked,
                                 known.Name & "(" & String.Join(", ",
                                     known.Values.Select(Function(v) "0")) & ");")
        AddBeside(New EpsNode(EpsKind.Statement, empty))
    End Sub

    Private Sub FolderButton_Click(sender As Object, e As EventArgs) Handles folderButton.Click
        AddBeside(New EpsNode(EpsKind.Folder, "New folder"))
    End Sub

    Private Sub AddBeside(node As EpsNode)
        Dim parent As EpsNode = root
        Dim at As Integer = -1
        If chosen IsNot Nothing AndAlso chosen.Kind <> EpsKind.Root Then
            If chosen.Kind = EpsKind.Folder OrElse chosen.Kind = EpsKind.Block Then
                parent = chosen
            ElseIf chosen.Parent IsNot Nothing Then
                parent = chosen.Parent
                at = parent.Children.IndexOf(chosen) + 1
            End If
        End If

        node.Parent = parent
        If at >= 0 AndAlso at <= parent.Children.Count Then
            parent.Children.Insert(at, node)
        Else
            parent.Children.Add(node)
        End If

        chosen = node
        dirty = True
        ProjectSet.saveStatus = False
        RebuildTree()
        RefreshSource()
    End Sub

    Private Sub SourceButton_Click(sender As Object, e As EventArgs) Handles sourceButton.Click
        If source.ReadOnly Then
            source.ReadOnly = False
            source.BackColor = ProgramSet.colorFieldBackground
            sourceButton.Text = "Read the text back"
            status.Text = "The text is yours. Press again to read it back into the tree."
        Else
            SetSource(source.Text)
            source.ReadOnly = True
            sourceButton.Text = "Edit as text"
            dirty = True
            ProjectSet.saveStatus = False
        End If
    End Sub

    Private Sub BuildButton_Click(sender As Object, e As EventArgs) Handles buildButton.Click
        SaveToProject()
        EpsBuild.Run(CurrentSource())
    End Sub
#End Region
End Class
