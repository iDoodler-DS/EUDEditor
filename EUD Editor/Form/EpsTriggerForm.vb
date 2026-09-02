Imports System.IO
Imports EUD_Editor.EpsSource

''' <summary>
''' A trigger editor whose source is epScript.
'''
''' The editor of today keeps a tree of nodes and writes epScript out of it. This
''' one keeps the epScript, and the tree is a way of looking at it. Nothing is
''' lost in the round trip, because nothing is converted: a line the editor cannot
''' draw is still a line, spelled the way it was written.
'''
''' The commands are the ones the old editor puts on its own tree: new things,
''' fold and unfold, edit, cut, copy, paste, delete, move. A line is edited in a
''' window of its own, opened by a double click or by Enter, which leaves the room
''' to the tree and the source.
''' </summary>
Public Class EpsTriggerForm
    Inherits Form

    Private WithEvents tree As New TreeView()
    Private ReadOnly source As New TextBox()
    Private ReadOnly status As New Label()
    Private ReadOnly split As New SplitContainer()

    Private WithEvents editButton As New Button()
    Private WithEvents sourceButton As New Button()
    Private WithEvents buildButton As New Button()

    Private WithEvents menu As New ContextMenuStrip()
    Private WithEvents newFolder As New ToolStripMenuItem("Folder")
    Private WithEvents newComment As New ToolStripMenuItem("Comment")
    Private WithEvents newAction As New ToolStripMenuItem("Action")
    Private WithEvents newCondition As New ToolStripMenuItem("Condition")
    Private WithEvents newIf As New ToolStripMenuItem("If")
    Private WithEvents newElseIf As New ToolStripMenuItem("Else if")
    Private WithEvents newElse As New ToolStripMenuItem("Else")
    Private WithEvents newWhile As New ToolStripMenuItem("While")
    Private WithEvents newFor As New ToolStripMenuItem("For")
    Private WithEvents newFunction As New ToolStripMenuItem("Function")
    Private WithEvents foldItem As New ToolStripMenuItem("Fold")
    Private WithEvents unfoldItem As New ToolStripMenuItem("Unfold")
    Private WithEvents foldAllItem As New ToolStripMenuItem("Fold all")
    Private WithEvents unfoldAllItem As New ToolStripMenuItem("Unfold all")
    Private WithEvents editItem As New ToolStripMenuItem("Edit")
    Private WithEvents offItem As New ToolStripMenuItem("Turn off")
    Private WithEvents cutItem As New ToolStripMenuItem("Cut")
    Private WithEvents copyItem As New ToolStripMenuItem("Copy")
    Private WithEvents codeCopyItem As New ToolStripMenuItem("Copy as text")
    Private WithEvents pasteItem As New ToolStripMenuItem("Paste")
    Private WithEvents deleteItem As New ToolStripMenuItem("Delete")
    Private WithEvents upItem As New ToolStripMenuItem("Move up")
    Private WithEvents downItem As New ToolStripMenuItem("Move down")

    Private root As EpsNode = New EpsNode(EpsKind.Root)
    Private chosen As EpsNode
    Private held As EpsNode          'what was cut or copied
    Private placed As Boolean

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
        BuildMenu()
    End Sub

#Region "How it is put together"
    Private Sub BuildLayout()
        Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 32,
                                             .Padding = New Padding(4, 3, 4, 3)}
        For Each pair In {Tuple.Create(editButton, "Edit"),
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

        source.Dock = DockStyle.Fill
        source.Multiline = True
        source.ScrollBars = ScrollBars.Both
        source.WordWrap = False
        source.ReadOnly = True
        source.Font = New Font("Consolas", 9.0F)

        split.Dock = DockStyle.Fill
        split.Panel1.Controls.Add(tree)
        split.Panel2.Controls.Add(source)

        Me.Controls.Add(split)
        Me.Controls.Add(bar)
    End Sub

    'The commands the old editor puts on its tree, in the order it puts them.
    Private Sub BuildMenu()
        Dim newItem As New ToolStripMenuItem("New")
        newItem.DropDownItems.AddRange(New ToolStripItem() {
            newFolder, newComment, New ToolStripSeparator(),
            newAction, newCondition, New ToolStripSeparator(),
            newIf, newElseIf, newElse, New ToolStripSeparator(),
            newWhile, newFor, New ToolStripSeparator(), newFunction})

        menu.Items.AddRange(New ToolStripItem() {
            newItem, New ToolStripSeparator(),
            foldItem, unfoldItem, foldAllItem, unfoldAllItem, New ToolStripSeparator(),
            editItem, offItem, New ToolStripSeparator(),
            cutItem, copyItem, codeCopyItem, pasteItem, deleteItem, New ToolStripSeparator(),
            upItem, downItem})
        tree.ContextMenuStrip = menu
    End Sub
#End Region

#Region "Opening and keeping"
    Private Sub EpsTriggerForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PlaceSplitter()
        LoadFromProject()
        Try
            ThemeSetForm.SetControlColor(Me)
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.Load")
        End Try
    End Sub

    Private Sub EpsTriggerForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not placed Then PlaceSplitter()
    End Sub

    Private Sub PlaceSplitter()
        Try
            If split.Width > 200 Then
                split.SplitterDistance = CInt(split.Width * 0.55)
                placed = True
            End If
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.PlaceSplitter")
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
    End Sub

    ''' <summary>Writes the source beside the project. Called when the project saves.</summary>
    Public Sub SaveToProject()
        Dim path As String = SourcePath(ProjectSet.filename)
        If path = "" Then Return
        Try
            File.WriteAllText(path, EpsWriter.Write(root))
        Catch ex As Exception
            LogException(ex, "writing " & path)
        End Try
    End Sub

    Public Function CurrentSource() As String
        Return EpsWriter.Write(root)
    End Function

    Private Sub SetSource(text As String)
        root = EpsReader.Parse(text)
        chosen = Nothing
        RebuildTree()
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
        RefreshSource()
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
    End Sub

    'A right click picks the node under the pointer, so the menu acts on it.
    Private Sub Tree_MouseDown(sender As Object, e As MouseEventArgs) Handles tree.MouseDown
        If e.Button <> MouseButtons.Right Then Return
        Dim under As TreeNode = tree.GetNodeAt(e.Location)
        If under IsNot Nothing Then tree.SelectedNode = under
    End Sub

    Private Sub Tree_DoubleClick(sender As Object, e As EventArgs) Handles tree.DoubleClick
        EditChosen()
    End Sub

    Private Sub Tree_KeyDown(sender As Object, e As KeyEventArgs) Handles tree.KeyDown
        If e.KeyCode = Keys.Enter Then
            EditChosen()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Delete Then
            DeleteChosen()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.C Then
            If chosen IsNot Nothing Then held = chosen.Clone()
        ElseIf e.Control AndAlso e.KeyCode = Keys.X Then
            CutChosen()
        ElseIf e.Control AndAlso e.KeyCode = Keys.V Then
            PasteHeld()
        ElseIf e.Control AndAlso e.KeyCode = Keys.Up Then
            Move(-1)
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.Down Then
            Move(1)
            e.Handled = True
        End If
    End Sub



    Private Sub ShowCounts()
        Dim lines As Integer = 0
        Dim drawn As Integer = 0
        For Each node As EpsNode In root.Walk()
            If node.Kind <> EpsKind.Statement Then Continue For
            lines += 1
            If EpsSymbols.Find(EpsLines.CallOf(node.Text)) IsNot Nothing Then drawn += 1
        Next
        status.Text = String.Format("{0} lines, {1} known to the editor, {2} names in the book",
                                    lines, drawn, EpsSymbols.Count)
    End Sub

    Private Sub RefreshSource()
        source.Text = EpsWriter.Write(root).Replace(vbLf, vbCrLf).Replace(vbCr & vbCr, vbCr)
    End Sub

    Private Sub Touched()
        ProjectSet.saveStatus = False
        RebuildTree()
    End Sub
#End Region

#Region "Putting something new in"
    'Where a new node goes: inside the chosen one when it can hold things, and
    'after it when it cannot.
    Private Sub Insert(node As EpsNode)
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
        Touched()
    End Sub

    ''' <summary>
    ''' Puts a node beside the chosen one, never inside it. An else follows the if
    ''' it belongs to; it does not live in it.
    ''' </summary>
    Private Sub InsertBeside(node As EpsNode)
        Dim after As EpsNode = chosen
        If after Is Nothing OrElse after.Kind = EpsKind.Root OrElse after.Parent Is Nothing Then
            Insert(node)
            Return
        End If

        Dim parent As EpsNode = after.Parent
        node.Parent = parent
        parent.Children.Insert(parent.Children.IndexOf(after) + 1, node)
        chosen = node
        Touched()
    End Sub

    Private Shared Function Block(head As String) As EpsNode
        Return New EpsNode(EpsKind.Block, head)
    End Function

    Private Sub NewFolder_Click(sender As Object, e As EventArgs) Handles newFolder.Click
        Insert(New EpsNode(EpsKind.Folder, "New folder"))
    End Sub

    Private Sub NewComment_Click(sender As Object, e As EventArgs) Handles newComment.Click
        Insert(New EpsNode(EpsKind.Comment, "// "))
    End Sub

    Private Sub NewAction_Click(sender As Object, e As EventArgs) Handles newAction.Click
        Dim picked As String = EpsPickCallForm.Ask(Me)
        If picked = "" Then Return
        Dim known As EpsCall = EpsSymbols.Find(picked)
        Insert(New EpsNode(EpsKind.Statement,
                           If(known Is Nothing, picked & "();", EpsLines.EmptyCall(known))))
    End Sub

    ''' <summary>
    ''' A condition is a test, so it joins the head of the block it belongs to. On
    ''' anything else it starts an if of its own, which is where a test belongs.
    ''' </summary>
    Private Sub NewCondition_Click(sender As Object, e As EventArgs) Handles newCondition.Click
        Dim picked As String = EpsPickCallForm.Ask(Me)
        If picked = "" Then Return
        Dim known As EpsCall = EpsSymbols.Find(picked)
        Dim test As String = If(known Is Nothing, picked & "()", EpsLines.EmptyTest(known))

        If chosen IsNot Nothing AndAlso chosen.Kind = EpsKind.Block AndAlso HasTest(chosen.Text) Then
            Dim head As String = chosen.Text.Trim()
            Dim opened As Integer = head.IndexOf("("c)
            Dim closed As Integer = head.LastIndexOf(")"c)
            If opened >= 0 AndAlso closed > opened Then
                Dim inside As String = head.Substring(opened + 1, closed - opened - 1).Trim()
                inside = If(inside = "", test, inside & " && " & test)
                chosen.Text = head.Substring(0, opened + 1) & inside & head.Substring(closed)
                Touched()
                Return
            End If
        End If

        Insert(Block("if (" & test & ")"))
    End Sub

    Private Shared Function HasTest(head As String) As Boolean
        Dim body As String = If(head, "").TrimStart()
        Return body.StartsWith("if") OrElse body.StartsWith("else if") OrElse body.StartsWith("while")
    End Function

    Private Sub NewIf_Click(sender As Object, e As EventArgs) Handles newIf.Click
        Insert(Block("if (Always())"))
    End Sub

    Private Sub NewElseIf_Click(sender As Object, e As EventArgs) Handles newElseIf.Click
        InsertBeside(Block("else if (Always())"))
    End Sub

    Private Sub NewElse_Click(sender As Object, e As EventArgs) Handles newElse.Click
        InsertBeside(Block("else"))
    End Sub

    Private Sub NewWhile_Click(sender As Object, e As EventArgs) Handles newWhile.Click
        Insert(Block("while (Always())"))
    End Sub

    Private Sub NewFor_Click(sender As Object, e As EventArgs) Handles newFor.Click
        'The spelling euddraft's own sample uses.
        Insert(Block("for (var i = 0; i < 10; i++)"))
    End Sub

    Private Sub NewFunction_Click(sender As Object, e As EventArgs) Handles newFunction.Click
        Insert(Block("function newFunction()"))
    End Sub
#End Region

#Region "What else the menu does"
    Private Sub EditItem_Click(sender As Object, e As EventArgs) Handles editItem.Click, editButton.Click
        EditChosen()
    End Sub

    Private Sub EditChosen()
        If chosen Is Nothing OrElse chosen.Kind = EpsKind.Root Then Return
        If EpsEditLineForm.Edit(Me, chosen) Then Touched()
    End Sub

    Private Sub OffItem_Click(sender As Object, e As EventArgs) Handles offItem.Click
        If chosen Is Nothing OrElse chosen.Kind = EpsKind.Root Then Return
        chosen.Off = Not chosen.Off
        Touched()
    End Sub

    Private Sub Menu_Opening(sender As Object, e As ComponentModel.CancelEventArgs) Handles menu.Opening
        Dim any As Boolean = chosen IsNot Nothing AndAlso chosen.Kind <> EpsKind.Root
        editItem.Enabled = any
        offItem.Enabled = any
        offItem.Text = If(any AndAlso chosen.Off, "Turn on", "Turn off")
        cutItem.Enabled = any
        copyItem.Enabled = any
        codeCopyItem.Enabled = any
        deleteItem.Enabled = any
        upItem.Enabled = any
        downItem.Enabled = any
        pasteItem.Enabled = held IsNot Nothing
        foldItem.Enabled = tree.SelectedNode IsNot Nothing
        unfoldItem.Enabled = tree.SelectedNode IsNot Nothing
        Try
            ThemeSetForm.SetControlColor(menu)
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.Menu_Opening")
        End Try
    End Sub

    Private Sub FoldItem_Click(sender As Object, e As EventArgs) Handles foldItem.Click
        If tree.SelectedNode IsNot Nothing Then tree.SelectedNode.Collapse()
    End Sub

    Private Sub UnfoldItem_Click(sender As Object, e As EventArgs) Handles unfoldItem.Click
        If tree.SelectedNode IsNot Nothing Then tree.SelectedNode.ExpandAll()
    End Sub

    Private Sub FoldAllItem_Click(sender As Object, e As EventArgs) Handles foldAllItem.Click
        tree.CollapseAll()
    End Sub

    Private Sub UnfoldAllItem_Click(sender As Object, e As EventArgs) Handles unfoldAllItem.Click
        tree.ExpandAll()
    End Sub

    Private Sub CopyItem_Click(sender As Object, e As EventArgs) Handles copyItem.Click
        If chosen IsNot Nothing Then held = chosen.Clone()
    End Sub

    Private Sub CodeCopyItem_Click(sender As Object, e As EventArgs) Handles codeCopyItem.Click
        If chosen Is Nothing Then Return
        Try
            Clipboard.SetText(String.Join(vbCrLf, EpsWriter.Lines(chosen, 0)))
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.CodeCopy")
        End Try
    End Sub

    Private Sub CutItem_Click(sender As Object, e As EventArgs) Handles cutItem.Click
        CutChosen()
    End Sub

    Private Sub CutChosen()
        If chosen Is Nothing OrElse chosen.Parent Is Nothing Then Return
        held = chosen.Clone()
        DeleteChosen()
    End Sub

    Private Sub PasteItem_Click(sender As Object, e As EventArgs) Handles pasteItem.Click
        PasteHeld()
    End Sub

    Private Sub PasteHeld()
        If held Is Nothing Then Return
        Insert(held.Clone())
    End Sub

    Private Sub DeleteItem_Click(sender As Object, e As EventArgs) Handles deleteItem.Click
        DeleteChosen()
    End Sub

    Private Sub DeleteChosen()
        If chosen Is Nothing OrElse chosen.Parent Is Nothing Then Return
        Dim parent As EpsNode = chosen.Parent
        parent.Remove(chosen)
        chosen = parent
        Touched()
    End Sub

    Private Sub UpItem_Click(sender As Object, e As EventArgs) Handles upItem.Click
        Move(-1)
    End Sub

    Private Sub DownItem_Click(sender As Object, e As EventArgs) Handles downItem.Click
        Move(1)
    End Sub

    Private Sub Move(step_ As Integer)
        If chosen Is Nothing OrElse chosen.Parent Is Nothing Then Return
        Dim parent As EpsNode = chosen.Parent
        Dim at As Integer = parent.Children.IndexOf(chosen)
        Dim goes As Integer = at + step_
        If goes < 0 OrElse goes >= parent.Children.Count Then Return
        parent.Children.RemoveAt(at)
        parent.Children.Insert(goes, chosen)
        Touched()
    End Sub

    Private Sub SourceButton_Click(sender As Object, e As EventArgs) Handles sourceButton.Click
        If source.ReadOnly Then
            source.ReadOnly = False
            sourceButton.Text = "Read the text back"
            status.Text = "The text is yours. Press again to read it back into the tree."
        Else
            SetSource(source.Text)
            source.ReadOnly = True
            sourceButton.Text = "Edit as text"
            ProjectSet.saveStatus = False
        End If
    End Sub

    Private Sub BuildButton_Click(sender As Object, e As EventArgs) Handles buildButton.Click
        SaveToProject()
        EpsBuild.Run(CurrentSource())
    End Sub
#End Region
End Class
