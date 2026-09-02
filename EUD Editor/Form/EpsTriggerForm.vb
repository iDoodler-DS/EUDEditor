Imports System.IO
Imports EUD_Editor.EpsSource

''' <summary>
''' A trigger editor whose source is epScript.
'''
''' The tree is laid out the way the old editor lays its own out. An if is not one
''' line holding a test; it is the construct, an "if :" holding one node for each
''' condition, and a "then :" holding what it does. A condition is a node like any
''' other: it can be picked, edited, moved and taken away on its own.
'''
''' None of that is in the file. The file says "if (A &amp;&amp; B) { ... }", which is
''' what a person writing epScript would write. The clauses are how it is shown.
''' </summary>
Public Class EpsTriggerForm
    Inherits Form

    Private WithEvents tree As New TreeView()
    Private ReadOnly source As New RichTextBox()
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
    Private chosen As Spot
    Private held As List(Of EpsNode)     'what was cut or copied
    Private ReadOnly picked As New List(Of TreeNode)          'every node picked
    Private ReadOnly wasInk As New Dictionary(Of TreeNode, Color)
    Private placed As Boolean

    ''' <summary>Which part of a clause a node stands for.</summary>
    Private Enum Part
        ''' <summary>The node itself.</summary>
        Whole = 0
        ''' <summary>The "if :" of a test, which holds its conditions.</summary>
        Conditions = 1
        ''' <summary>The "then :" of a test, which holds what it does.</summary>
        Body = 2
        ''' <summary>One condition of a test.</summary>
        Condition = 3
    End Enum

    ''' <summary>What a node of the tree stands for in the source.</summary>
    Private Class Spot
        Public ReadOnly Node As EpsNode
        Public ReadOnly Part As Part
        Public ReadOnly At As Integer

        Public Sub New(node As EpsNode, Optional part As Part = Part.Whole, Optional at As Integer = -1)
            Me.Node = node
            Me.Part = part
            Me.At = at
        End Sub
    End Class

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
        'The three blocks stand from the start, so a source asked for before the
        'tab has ever been shown is the three blocks and not nothing at all.
        EnsureHooks()
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
        source.ScrollBars = RichTextBoxScrollBars.Both
        source.WordWrap = False
        source.BorderStyle = BorderStyle.None
        source.DetectUrls = False
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
            newWhile, newFor})

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
        EnsureHooks()
        chosen = Nothing
        RebuildTree()
    End Sub
#End Region

#Region "The tree, laid out the way the old editor lays one out"
    Private Sub RebuildTree()
        Dim wasNode As EpsNode = If(chosen Is Nothing, Nothing, chosen.Node)
        Dim wasPart As Part = If(chosen Is Nothing, Part.Whole, chosen.Part)
        Dim wasAt As Integer = If(chosen Is Nothing, -1, chosen.At)

        'The nodes are made again, so what was picked is gone with the old ones.
        Unpick()

        tree.BeginUpdate()
        tree.Nodes.Clear()
        For Each child As EpsNode In root.Children
            tree.Nodes.Add(MakeNode(child))
        Next
        tree.ExpandAll()
        tree.EndUpdate()

        If wasNode IsNot Nothing Then Select_(wasNode, wasPart, wasAt)
        If tree.SelectedNode Is Nothing AndAlso tree.Nodes.Count > 0 Then
            tree.SelectedNode = tree.Nodes(0)
        End If
        RefreshSource()
        ShowCounts()
    End Sub

    Private Function MakeNode(node As EpsNode, Optional insideOff As Boolean = False) As TreeNode
        Dim off As Boolean = insideOff OrElse node.Off
        Dim shown As New TreeNode(CaptionOf(node)) With {.Tag = New Spot(node)}
        Paint(shown, node, off)

        If EpsHead.ShapeOf(node) = EpsShape.Test Then
            'A test is its conditions and what it does, each under a clause of its
            'own, which is how the old editor shows one.
            Dim keyword As String = EpsHead.KeywordOf(node.Text)
            Dim conditions As New TreeNode(If(keyword = "while", "while :", "if :")) With {
                .Tag = New Spot(node, Part.Conditions)}
            conditions.ForeColor = Color.LightBlue

            Dim terms As List(Of String) = EpsHead.TermsOf(node.Text)
            For i = 0 To terms.Count - 1
                Dim said As String = EpsLines.Sentenced(terms(i))
                Dim term As New TreeNode(If(said = "", terms(i), said)) With {
                    .Tag = New Spot(node, Part.Condition, i)}
                If off Then term.ForeColor = Color.Gray
                conditions.Nodes.Add(term)
            Next
            shown.Nodes.Add(conditions)

            Dim body As New TreeNode("then :") With {.Tag = New Spot(node, Part.Body)}
            body.ForeColor = Color.LightBlue
            For Each child As EpsNode In node.Children
                body.Nodes.Add(MakeNode(child, off))
            Next
            shown.Nodes.Add(body)
            Return shown
        End If

        For Each child As EpsNode In node.Children
            shown.Nodes.Add(MakeNode(child, off))
        Next
        Return shown
    End Function

    'What a node is called in the tree. A construct is named for what it is, the
    'way the old editor names one, rather than for how it is spelled.
    Private Shared Function CaptionOf(node As EpsNode) As String
        Select Case EpsHead.ShapeOf(node)
            Case EpsShape.Test
                Select Case EpsHead.KeywordOf(node.Text)
                    Case "while" : Return "While (Conditions) do (Actions)"
                    Case "else if" : Return "Else if (Conditions) then do (Actions)"
                    Case Else : Return "If (Conditions) then do (Actions)"
                End Select
            Case EpsShape.Plain
                Return "Else do (Actions)"
            Case EpsShape.Folder
                Return node.Text
            Case EpsShape.Function_
                'One of the three reads as when it runs, not as what it is called.
                Dim said As String = ""
                If HookNames.TryGetValue(EpsHead.FunctionName(node.Text), said) Then Return said
                Return node.Caption()
            Case Else
                'A call the editor has words for reads as those words, the same
                'ones the edit window shows. Anything else reads as it is written.
                If node.Kind = EpsKind.Statement Then
                    Dim said As String = EpsLines.Sentenced(node.Text)
                    If said <> "" Then Return said
                End If

                Dim caption As String = node.Caption()
                If node.Kind = EpsKind.Comment AndAlso caption = "" Then Return "(blank line)"
                Return caption
        End Select
    End Function

    'The colours the old editor uses, so the same kind of thing reads the same way.
    Private Shared Sub Paint(shown As TreeNode, node As EpsNode, off As Boolean)
        Select Case EpsHead.ShapeOf(node)
            Case EpsShape.Test, EpsShape.For_, EpsShape.Function_, EpsShape.Plain
                shown.ForeColor = Color.LightPink
            Case EpsShape.Folder
                shown.ForeColor = Color.LightGreen
            Case Else
                If node.Kind = EpsKind.Comment Then
                    shown.ForeColor = Color.FromArgb(120, 160, 120)
                ElseIf EpsSymbols.Find(EpsLines.CallOf(node.Text)) IsNot Nothing Then
                    shown.ForeColor = Color.DodgerBlue
                End If
        End Select
        If off Then shown.ForeColor = Color.Gray
    End Sub

    Private Sub Select_(node As EpsNode, part As Part, at As Integer)
        For Each shown As TreeNode In AllNodes(tree.Nodes)
            Dim spot As Spot = TryCast(shown.Tag, Spot)
            If spot IsNot Nothing AndAlso spot.Node Is node AndAlso spot.Part = part AndAlso spot.At = at Then
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
        chosen = TryCast(e.Node.Tag, Spot)
        'Moving about with the arrow keys picks one thing, as it always did.
        If Not picked.Contains(e.Node) Then
            Unpick()
            picked.Add(e.Node)
        End If
    End Sub

    'A right click picks the node under the pointer, so the menu acts on it.
    ''' <summary>
    ''' Ctrl and the wheel make the tree larger or smaller. A trigger tree gets
    ''' deep, and a person reading one wants to see more of it at once; the box
    ''' beside it zooms the same way of its own accord.
    ''' </summary>
    Private Sub Tree_MouseWheel(sender As Object, e As MouseEventArgs) Handles tree.MouseWheel
        If (Control.ModifierKeys And Keys.Control) <> Keys.Control Then Return

        Dim held As HandledMouseEventArgs = TryCast(e, HandledMouseEventArgs)
        If held IsNot Nothing Then held.Handled = True     'zoom instead of scroll

        Dim size As Single = tree.Font.Size + If(e.Delta > 0, 1.0F, -1.0F)
        If size < 6.0F OrElse size > 30.0F Then Return
        Try
            tree.Font = New Font(tree.Font.FontFamily, size, tree.Font.Style)
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.Tree_MouseWheel")
        End Try
    End Sub

    Private Sub Tree_MouseDown(sender As Object, e As MouseEventArgs) Handles tree.MouseDown
        Dim under As TreeNode = tree.GetNodeAt(e.Location)
        If under Is Nothing Then Return

        If e.Button = MouseButtons.Right Then
            'A right click on something already picked leaves the picking alone,
            'so the menu acts on all of it.
            If Not picked.Contains(under) Then Pick(under, False, False)
            tree.SelectedNode = under
            Return
        End If

        Pick(under, (Control.ModifierKeys And Keys.Control) = Keys.Control,
                    (Control.ModifierKeys And Keys.Shift) = Keys.Shift)
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
            CopyChosen()
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
        Dim text As String = EpsWriter.Write(root).Replace(vbLf, vbCrLf).Replace(vbCr & vbCr, vbCr)
        If source.ReadOnly Then
            EpsPaint.Draw(source, text)
        Else
            'While the text belongs to the user it is left alone; it is coloured
            'again when it is read back into the tree.
            source.Text = text
        End If
    End Sub

    Private Sub Touched()
        ProjectSet.saveStatus = False
        RebuildTree()
    End Sub
#End Region

#Region "The conditions of a test"
    Private Sub SetTerm(node As EpsNode, at As Integer, text As String)
        Dim terms As List(Of String) = EpsHead.TermsOf(node.Text)
        If at < 0 OrElse at >= terms.Count Then Return
        terms(at) = text
        node.Text = EpsHead.ComposeTest(EpsHead.KeywordOf(node.Text), terms)
    End Sub

    Private Sub AddTerm(node As EpsNode, text As String, Optional at As Integer = -1)
        Dim terms As List(Of String) = EpsHead.TermsOf(node.Text)
        If at >= 0 AndAlso at <= terms.Count Then
            terms.Insert(at, text)
        Else
            terms.Add(text)
        End If
        node.Text = EpsHead.ComposeTest(EpsHead.KeywordOf(node.Text), terms)
    End Sub

    Private Sub RemoveTerm(node As EpsNode, at As Integer)
        Dim terms As List(Of String) = EpsHead.TermsOf(node.Text)
        If at < 0 OrElse at >= terms.Count Then Return
        terms.RemoveAt(at)
        node.Text = EpsHead.ComposeTest(EpsHead.KeywordOf(node.Text), terms)
    End Sub

    Private Sub MoveTerm(node As EpsNode, at As Integer, step_ As Integer)
        Dim terms As List(Of String) = EpsHead.TermsOf(node.Text)
        Dim goes As Integer = at + step_
        If at < 0 OrElse at >= terms.Count OrElse goes < 0 OrElse goes >= terms.Count Then Return
        Dim moved As String = terms(at)
        terms.RemoveAt(at)
        terms.Insert(goes, moved)
        node.Text = EpsHead.ComposeTest(EpsHead.KeywordOf(node.Text), terms)
    End Sub
#End Region

#Region "The three blocks"
    'The three functions euddraft calls, in the order they are written. They are
    'the whole of the top level: the editor puts them back whenever they are
    'missing, will not take them away, and puts everything new inside one of them.
    Private Shared ReadOnly Hooks As String() = {"onPluginStart", "beforeTriggerExec", "afterTriggerExec"}

    'What each is called on the tree. euddraft wants the names on the left;
    'a person reading a trigger wants to know when it runs.
    Private Shared ReadOnly HookNames As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"onPluginStart", "On map start"},
        {"beforeTriggerExec", "Before map triggers"},
        {"afterTriggerExec", "After map triggers"}}

    ''' <summary>
    ''' Whether a node is one of the three, which are not the editor's to change.
    ''' Wherever it stands: a hand-written file may have put one inside a folder,
    ''' and it is still the block euddraft calls.
    ''' </summary>
    Private Function IsHook(node As EpsNode) As Boolean
        If node Is Nothing OrElse node.Kind = EpsKind.Root Then Return False
        If EpsHead.ShapeOf(node) <> EpsShape.Function_ Then Return False
        Return Array.IndexOf(Hooks, EpsHead.FunctionName(node.Text)) >= 0
    End Function

    ''' <summary>The one of the three that carries a given name, or Nothing.</summary>
    Private Function HookNamed(name As String) As EpsNode
        For Each node As EpsNode In root.Walk()
            If EpsHead.ShapeOf(node) = EpsShape.Function_ AndAlso
               EpsHead.FunctionName(node.Text) = name Then Return node
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Puts back any of the three the source does not have, each after the one
    ''' before it. Anything else already at the top level is left where it is.
    ''' </summary>
    Private Sub EnsureHooks()
        'A blank line between the blocks says nothing and cannot be picked, so it
        'is not carried as a node of its own at the top level.
        For at As Integer = root.Children.Count - 1 To 0 Step -1
            Dim child As EpsNode = root.Children(at)
            If child.Kind = EpsKind.Comment AndAlso child.Text.Trim() = "" Then
                root.Children.RemoveAt(at)
            End If
        Next

        Dim after As EpsNode = Nothing
        For Each name As String In Hooks
            Dim standing As EpsNode = HookNamed(name)
            If standing Is Nothing Then
                standing = Block("function " & name & "()")
                Dim beside As EpsNode = If(after Is Nothing OrElse after.Parent Is Nothing,
                                           root, after.Parent)
                standing.Parent = beside
                Dim at As Integer = If(after Is Nothing, beside.Children.Count,
                                       beside.Children.IndexOf(after) + 1)
                beside.Children.Insert(at, standing)
            End If
            after = standing
        Next
    End Sub

    ''' <summary>
    ''' Which of the three a new node belongs in: the one the selection sits
    ''' under, or the first when the selection is outside them all.
    ''' </summary>
    Private Function HookForNew() As EpsNode
        Dim walk As EpsNode = If(chosen Is Nothing, Nothing, chosen.Node)
        While walk IsNot Nothing AndAlso walk.Kind <> EpsKind.Root
            If IsHook(walk) Then Return walk
            walk = walk.Parent
        End While
        Return If(HookNamed(Hooks(0)), root)
    End Function
#End Region

#Region "Picking more than one"
    'A TreeView holds one selection, and a person editing triggers wants to take
    'a run of them at once. So the picking is the editor's own: the control keeps
    'its idea of the node last clicked, and this keeps the rest.
    '
    'Only kin can be picked together. Picking a node and something inside it, or
    'two nodes from different blocks, has no good meaning when they are copied or
    'taken away, so a click outside the family starts again.

    ''' <summary>Picks a node, adding to what is picked or starting again.</summary>
    Private Sub Pick(one As TreeNode, add As Boolean, upTo As Boolean)
        If one Is Nothing Then Return

        Dim family As TreeNodeCollection = If(one.Parent Is Nothing, tree.Nodes, one.Parent.Nodes)
        Dim kin As Boolean = picked.Count > 0 AndAlso
                             Not (picked(0).Parent Is Nothing Xor one.Parent Is Nothing) AndAlso
                             (picked(0).Parent Is Nothing OrElse picked(0).Parent Is one.Parent)

        If Not kin Then
            add = False
            upTo = False
        End If

        If upTo Then
            'Shift takes everything between what was picked first and this.
            Dim from_ As Integer = family.IndexOf(picked(0))
            Dim [to] As Integer = family.IndexOf(one)
            Dim keep As TreeNode = picked(0)
            Unpick()
            picked.Add(keep)
            For at = Math.Min(from_, [to]) To Math.Max(from_, [to])
                If family(at) IsNot keep Then Show_(family(at))
            Next
        ElseIf add Then
            If picked.Contains(one) Then
                Hide_(one)
                picked.Remove(one)
            Else
                Show_(one)
            End If
        Else
            Unpick()
            picked.Add(one)
        End If

        tree.SelectedNode = one
        chosen = TryCast(one.Tag, Spot)
        ShowCounts()
    End Sub

    Private Sub Show_(one As TreeNode)
        If picked.Contains(one) Then Return
        picked.Add(one)
        If Not wasInk.ContainsKey(one) Then wasInk(one) = one.ForeColor
        one.BackColor = SystemColors.Highlight
        one.ForeColor = SystemColors.HighlightText
    End Sub

    Private Sub Hide_(one As TreeNode)
        one.BackColor = Color.Empty
        Dim ink As Color
        If wasInk.TryGetValue(one, ink) Then
            one.ForeColor = ink
            wasInk.Remove(one)
        End If
    End Sub

    ''' <summary>Lets go of everything picked.</summary>
    Private Sub Unpick()
        For Each one As TreeNode In picked
            Hide_(one)
        Next
        picked.Clear()
        wasInk.Clear()
    End Sub

    ''' <summary>
    ''' What is picked, in the order it stands, as nodes of the source. A run of
    ''' an if and its elses comes back whole however much of it was picked.
    ''' </summary>
    Private Function PickedNodes() As List(Of EpsNode)
        Dim out As New List(Of EpsNode)
        If picked.Count = 0 Then Return out

        Dim family As TreeNodeCollection = If(picked(0).Parent Is Nothing, tree.Nodes, picked(0).Parent.Nodes)
        For at = 0 To family.Count - 1
            If Not picked.Contains(family(at)) Then Continue For
            Dim spot As Spot = TryCast(family(at).Tag, Spot)
            If spot Is Nothing OrElse spot.Part <> Part.Whole OrElse spot.Node Is Nothing Then Continue For
            For Each one As EpsNode In Chain(spot.Node)
                If Not out.Contains(one) Then out.Add(one)
            Next
        Next
        Return out
    End Function
#End Region

#Region "An if and what follows it"
    'An else has no meaning without the if before it, so the two are one thing
    'as far as moving, cutting and deleting go. Anything else would leave the
    'source unbuildable, and the tree is not the place to find that out.

    ''' <summary>Whether a block carries on from the one before it.</summary>
    Private Shared Function Continues(node As EpsNode) As Boolean
        If node Is Nothing OrElse node.Kind <> EpsKind.Block Then Return False
        Dim head As String = node.Text.Trim()
        Return head = "else" OrElse head.StartsWith("else ") OrElse head.StartsWith("else{") OrElse
               head.StartsWith("else(")
    End Function

    ''' <summary>
    ''' The whole run a node belongs to: the if it starts from, and every else if
    ''' and else that follows. A node that is not part of one is a run of itself.
    ''' </summary>
    Private Shared Function Chain(node As EpsNode) As List(Of EpsNode)
        Dim run As New List(Of EpsNode)
        If node Is Nothing Then Return run

        Dim kin As List(Of EpsNode) = If(node.Parent Is Nothing, Nothing, node.Parent.Children)
        If kin Is Nothing Then
            run.Add(node)
            Return run
        End If

        Dim at As Integer = kin.IndexOf(node)
        If at < 0 Then
            run.Add(node)
            Return run
        End If

        Dim first As Integer = at
        While first > 0 AndAlso Continues(kin(first))
            first -= 1
        End While

        Dim last As Integer = first
        While last + 1 < kin.Count AndAlso Continues(kin(last + 1))
            last += 1
        End While

        For i = first To last
            run.Add(kin(i))
        Next
        Return run
    End Function

    ''' <summary>Where a run ends among its kin, so nothing is put inside it.</summary>
    Private Shared Function AfterChain(node As EpsNode) As Integer
        Dim run As List(Of EpsNode) = Chain(node)
        Dim last As EpsNode = run(run.Count - 1)
        If last.Parent Is Nothing Then Return -1
        Return last.Parent.Children.IndexOf(last) + 1
    End Function
#End Region

#Region "Putting something new in"
    ''' <summary>
    ''' Where a new node goes. A clause takes what belongs in it: what a test does
    ''' goes under its "then :", and everything else beside the node picked.
    ''' </summary>
    Private Sub Insert(node As EpsNode)
        Dim parent As EpsNode = root
        Dim at As Integer = -1

        If chosen IsNot Nothing AndAlso chosen.Node IsNot Nothing AndAlso chosen.Node.Kind <> EpsKind.Root Then
            Select Case chosen.Part
                Case Part.Body, Part.Conditions, Part.Condition
                    parent = chosen.Node
                Case Else
                    If chosen.Node.Kind = EpsKind.Folder OrElse chosen.Node.Kind = EpsKind.Block Then
                        parent = chosen.Node
                    ElseIf chosen.Node.Parent IsNot Nothing Then
                        parent = chosen.Node.Parent
                        'After the whole of an if ... else, never between its parts.
                        at = AfterChain(chosen.Node)
                    End If
            End Select
        End If

        'The top level is the three blocks and nothing else, so anything that
        'would have landed there goes inside one of them instead.
        If parent Is root Then
            parent = HookForNew()
            at = -1
        End If

        node.Parent = parent
        If at >= 0 AndAlso at <= parent.Children.Count Then
            parent.Children.Insert(at, node)
        Else
            parent.Children.Add(node)
        End If
        chosen = New Spot(node)
        Touched()
    End Sub

    ''' <summary>
    ''' Puts a node beside the one picked, never inside it. An else if goes after
    ''' the last of its kind and before the else; an else goes last of all, which
    ''' is the only order epScript will take.
    ''' </summary>
    Private Sub InsertBeside(node As EpsNode)
        Dim after As EpsNode = If(chosen Is Nothing, Nothing, chosen.Node)
        If after Is Nothing OrElse after.Kind = EpsKind.Root OrElse
           after.Parent Is Nothing OrElse after.Parent Is root Then
            Insert(node)
            Return
        End If

        Dim parent As EpsNode = after.Parent
        Dim run As List(Of EpsNode) = Chain(after)
        Dim at As Integer = parent.Children.IndexOf(run(run.Count - 1)) + 1

        If Continues(node) AndAlso node.Text.Trim() <> "else" Then
            'An else if belongs before an else that is already there.
            For at2 As Integer = 0 To run.Count - 1
                If run(at2).Text.Trim() = "else" Then
                    at = parent.Children.IndexOf(run(at2))
                    Exit For
                End If
            Next
        End If

        node.Parent = parent
        parent.Children.Insert(at, node)
        chosen = New Spot(node)
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
    ''' A condition belongs to a test. Picked on one, or on its conditions, or on a
    ''' condition of it, it joins that test. Anywhere else it starts an if of its
    ''' own, which is where a condition can live.
    ''' </summary>
    Private Sub NewCondition_Click(sender As Object, e As EventArgs) Handles newCondition.Click
        Dim picked As String = EpsPickCallForm.Ask(Me)
        If picked = "" Then Return
        Dim known As EpsCall = EpsSymbols.Find(picked)
        Dim test As String = If(known Is Nothing, picked & "()", EpsLines.EmptyTest(known))

        Dim node As EpsNode = If(chosen Is Nothing, Nothing, chosen.Node)
        If node IsNot Nothing AndAlso EpsHead.ShapeOf(node) = EpsShape.Test Then
            Dim at As Integer = If(chosen.Part = Part.Condition, chosen.At + 1, -1)
            AddTerm(node, test, at)
            chosen = New Spot(node, Part.Condition,
                              If(at >= 0, at, EpsHead.TermsOf(node.Text).Count - 1))
            Touched()
            Return
        End If

        Insert(Block("if (" & test & ")"))
    End Sub

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

#End Region

#Region "What else the menu does"
    Private Sub EditItem_Click(sender As Object, e As EventArgs) Handles editItem.Click, editButton.Click
        EditChosen()
    End Sub

    Private Sub EditChosen()
        If chosen Is Nothing OrElse chosen.Node Is Nothing Then Return
        If chosen.Node.Kind = EpsKind.Root Then Return

        If chosen.Part = Part.Condition Then
            'A condition is edited on its own, then put back into the test.
            Dim terms As List(Of String) = EpsHead.TermsOf(chosen.Node.Text)
            If chosen.At < 0 OrElse chosen.At >= terms.Count Then Return
            Dim standing As New EpsNode(EpsKind.Statement, terms(chosen.At))
            If EpsEditLineForm.Edit(Me, standing, root) Then
                SetTerm(chosen.Node, chosen.At, standing.Text.TrimEnd(";"c).Trim())
                Touched()
            End If
            Return
        End If

        If chosen.Part <> Part.Whole Then Return    'a clause holds things; it is not one
        If IsHook(chosen.Node) Then Return
        If EpsEditLineForm.Edit(Me, chosen.Node, root) Then Touched()
    End Sub

    Private Sub OffItem_Click(sender As Object, e As EventArgs) Handles offItem.Click
        If chosen Is Nothing OrElse chosen.Part <> Part.Whole Then Return
        If chosen.Node Is Nothing OrElse chosen.Node.Kind = EpsKind.Root Then Return
        If IsHook(chosen.Node) Then Return
        chosen.Node.Off = Not chosen.Node.Off
        Touched()
    End Sub

    Private Sub Menu_Opening(sender As Object, e As ComponentModel.CancelEventArgs) Handles menu.Opening
        Dim whole As Boolean = chosen IsNot Nothing AndAlso chosen.Part = Part.Whole AndAlso
                               chosen.Node IsNot Nothing AndAlso chosen.Node.Kind <> EpsKind.Root
        Dim condition As Boolean = chosen IsNot Nothing AndAlso chosen.Part = Part.Condition

        'One of the three blocks is shown and filled, but never changed.
        Dim standing As Boolean = whole AndAlso IsHook(chosen.Node)
        whole = whole AndAlso Not standing

        editItem.Enabled = whole OrElse condition
        'A single condition cannot be commented out on its own; it is part of a line.
        offItem.Enabled = whole
        offItem.Text = If(whole AndAlso chosen.Node.Off, "Turn on", "Turn off")
        Dim many As Integer = PickedNodes().Count
        cutItem.Enabled = whole
        copyItem.Enabled = whole OrElse standing
        codeCopyItem.Enabled = whole OrElse standing
        cutItem.Text = If(many > 1, "Cut " & many & " lines", "Cut")
        copyItem.Text = If(many > 1, "Copy " & many & " lines", "Copy")
        deleteItem.Text = If(many > 1, "Delete " & many & " lines", "Delete")
        deleteItem.Enabled = whole OrElse condition
        upItem.Enabled = whole OrElse condition
        downItem.Enabled = whole OrElse condition
        pasteItem.Enabled = held IsNot Nothing AndAlso held.Count > 0
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
        CopyChosen()
    End Sub

    Private Sub CopyChosen()
        Dim taking As List(Of EpsNode) = PickedNodes()
        If taking.Count = 0 Then
            If chosen Is Nothing OrElse chosen.Part <> Part.Whole OrElse chosen.Node Is Nothing Then Return
            taking = Chain(chosen.Node)
        End If

        held = New List(Of EpsNode)
        For Each one As EpsNode In taking
            held.Add(one.Clone())
        Next
    End Sub

    Private Sub CodeCopyItem_Click(sender As Object, e As EventArgs) Handles codeCopyItem.Click
        If chosen Is Nothing OrElse chosen.Node Is Nothing Then Return
        Try
            Clipboard.SetText(String.Join(vbCrLf, EpsWriter.Lines(chosen.Node, 0)))
        Catch ex As Exception
            LogSuppressed(ex, "EpsTriggerForm.CodeCopy")
        End Try
    End Sub

    Private Sub CutItem_Click(sender As Object, e As EventArgs) Handles cutItem.Click
        CutChosen()
    End Sub

    Private Sub CutChosen()
        If chosen Is Nothing OrElse chosen.Part <> Part.Whole Then Return
        If chosen.Node Is Nothing OrElse chosen.Node.Parent Is Nothing Then Return
        CopyChosen()
        DeleteChosen()
    End Sub

    Private Sub PasteItem_Click(sender As Object, e As EventArgs) Handles pasteItem.Click
        PasteHeld()
    End Sub

    Private Sub PasteHeld()
        If held Is Nothing OrElse held.Count = 0 Then Return
        'An if and what follows it are put back together, in order.
        Insert(held(0).Clone())
        For at = 1 To held.Count - 1
            InsertBeside(held(at).Clone())
        Next
    End Sub

    Private Sub DeleteItem_Click(sender As Object, e As EventArgs) Handles deleteItem.Click
        DeleteChosen()
    End Sub

    Private Sub DeleteChosen()
        If chosen Is Nothing OrElse chosen.Node Is Nothing Then Return

        If chosen.Part = Part.Condition Then
            RemoveTerm(chosen.Node, chosen.At)
            chosen = New Spot(chosen.Node, Part.Conditions)
            Touched()
            Return
        End If

        If chosen.Part <> Part.Whole OrElse chosen.Node.Parent Is Nothing Then Return
        If IsHook(chosen.Node) Then Return
        Dim parent As EpsNode = chosen.Node.Parent

        'Everything picked goes. An else cannot stand without its if, so taking
        'the if away takes the rest of the run with it; taking an else away on
        'its own leaves the if standing.
        Dim taking As List(Of EpsNode) = PickedNodes()
        If taking.Count = 0 Then
            taking = If(Continues(chosen.Node),
                        New List(Of EpsNode) From {chosen.Node},
                        Chain(chosen.Node))
        ElseIf taking.Count = 1 AndAlso Continues(chosen.Node) Then
            taking = New List(Of EpsNode) From {chosen.Node}
        End If

        For Each one As EpsNode In taking
            If IsHook(one) Then Continue For
            If one.Parent IsNot Nothing Then one.Parent.Remove(one)
        Next
        Unpick()
        chosen = New Spot(parent)
        Touched()
    End Sub

    Private Sub UpItem_Click(sender As Object, e As EventArgs) Handles upItem.Click
        Move(-1)
    End Sub

    Private Sub DownItem_Click(sender As Object, e As EventArgs) Handles downItem.Click
        Move(1)
    End Sub

    Private Sub Move(step_ As Integer)
        If chosen Is Nothing OrElse chosen.Node Is Nothing Then Return

        If chosen.Part = Part.Condition Then
            Dim goes As Integer = chosen.At + step_
            MoveTerm(chosen.Node, chosen.At, step_)
            chosen = New Spot(chosen.Node, Part.Condition, goes)
            Touched()
            Return
        End If

        If chosen.Part <> Part.Whole OrElse chosen.Node.Parent Is Nothing Then Return
        If IsHook(chosen.Node) Then Return

        'An if and its elses move as one, and step over the whole of whatever
        'stands next to them, so neither run is ever broken open.
        Dim parent As EpsNode = chosen.Node.Parent
        Dim run As List(Of EpsNode) = Chain(chosen.Node)
        Dim first As Integer = parent.Children.IndexOf(run(0))
        Dim last As Integer = first + run.Count - 1

        Dim lands As Integer
        If step_ < 0 Then
            If first = 0 Then Return
            lands = parent.Children.IndexOf(Chain(parent.Children(first - 1))(0))
        Else
            If last = parent.Children.Count - 1 Then Return
            Dim beyond As List(Of EpsNode) = Chain(parent.Children(last + 1))
            lands = parent.Children.IndexOf(beyond(beyond.Count - 1)) + 1 - run.Count
        End If

        parent.Children.RemoveRange(first, run.Count)
        parent.Children.InsertRange(lands, run)
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
