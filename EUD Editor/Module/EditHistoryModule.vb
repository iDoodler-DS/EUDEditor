' Undo and redo for project data.
'
' Scope: one history for each main tab, not one for the whole editor.
' This history serves the data editors, which all change the same .dat family:
' DatEdit, FireGraft and FileManager. The trigger editor keeps its own history
' (CTaskManager) because it edits a tree, not values, and because it records only
' about half of its changes. A merged stack would sometimes undo a data change while
' the user expected a trigger change, in a tab they are not looking at.
'
' The four-tab layout in phase 2 of the roadmap makes this rule visible: the Data tab
' and the Triggers tab each carry their own Edit menu, so Ctrl+Z always means "undo my
' last change in this tab".
'
' Every edit knows where it was made. Undo and redo move the user to that place, then
' change the value, then mark the field. A change is never invisible.

Namespace EditHistory

    ''' <summary>One reversible change. Add a subclass to give another editor a history.</summary>
    Public MustInherit Class Edit
        ''' <summary>Puts the old value back.</summary>
        Public MustOverride Sub Undo()

        ''' <summary>Puts the new value back.</summary>
        Public MustOverride Sub Redo()

        ''' <summary>Short name of the change, for the Edit menu.</summary>
        Public MustOverride Function Describe() As String

        ''' <summary>Moves the user to the field this edit changes. Runs before Undo and Redo.</summary>
        Public Overridable Sub Reveal()
        End Sub

        ''' <summary>Redraws the editor and marks the field. Runs after Undo and Redo.</summary>
        Public Overridable Sub AfterApply()
        End Sub

        ''' <summary>
        ''' True when this edit continues the previous edit and the two can become one,
        ''' for example one more keystroke in the same text box.
        ''' </summary>
        Public Overridable Function CanMergeWith(previous As Edit) As Boolean
            Return False
        End Function

        ''' <summary>Takes the new value of the later edit and keeps the old value of this edit.</summary>
        Public Overridable Sub MergeWith(newer As Edit)
        End Sub

        Public Property Stamp As DateTime = DateTime.UtcNow
    End Class

    ''' <summary>A set of edits that undo as one step, for example a paste.</summary>
    Public Class EditGroup
        Inherits Edit

        Public ReadOnly Edits As New List(Of Edit)
        Private ReadOnly label As String

        Public Sub New(label As String)
            Me.label = label
        End Sub

        Public Overrides Sub Undo()
            'Reverse order, because a later edit can depend on an earlier one.
            For i = Edits.Count - 1 To 0 Step -1
                Edits(i).Undo()
            Next
        End Sub

        Public Overrides Sub Redo()
            For Each e As Edit In Edits
                e.Redo()
            Next
        End Sub

        Public Overrides Function Describe() As String
            Return label
        End Function

        Public Overrides Sub Reveal()
            If Edits.Count > 0 Then Edits(0).Reveal()
        End Sub

        Public Overrides Sub AfterApply()
            If Edits.Count > 0 Then Edits(0).AfterApply()
        End Sub
    End Class

    ''' <summary>A changed value in one of the .dat tables.</summary>
    Public Class DatEdit
        Inherits Edit

        Public ReadOnly DataIndex As Integer   'index into DatEditDATA, and the DatEdit tab index
        Public ReadOnly FieldKey As String     'field name, and the Tag of the control that shows it
        Public ReadOnly EntryIndex As Long     'unit, weapon or other entry number
        Public ReadOnly OldValue As Long
        Public NewValue As Long

        Private Const MergeWindowMs As Integer = 900

        Public Sub New(dataIndex As Integer, fieldKey As String, entryIndex As Long, oldValue As Long, newValue As Long)
            Me.DataIndex = dataIndex
            Me.FieldKey = fieldKey
            Me.EntryIndex = entryIndex
            Me.OldValue = oldValue
            Me.NewValue = newValue
        End Sub

        Public Overrides Sub Undo()
            Apply(OldValue)
        End Sub

        Public Overrides Sub Redo()
            Apply(NewValue)
        End Sub

        Private Sub Apply(value As Long)
            If DataIndex < 0 OrElse DataIndex >= DatEditDATA.Count Then Return
            DatEditDATA(DataIndex).WriteValue(FieldKey, CUInt(EntryIndex), value)
        End Sub

        Public Overrides Function Describe() As String
            Dim entryName As String = ""
            Try
                If DataIndex >= 0 AndAlso DataIndex < CODE.Count AndAlso EntryIndex < CODE(DataIndex).Count Then
                    entryName = CODE(DataIndex)(CInt(EntryIndex))
                End If
            Catch
            End Try
            If entryName = "" Then Return FieldKey
            Return FieldKey & " of " & entryName
        End Function

        Public Overrides Sub Reveal()
            Main.RevealDatField(DataIndex, EntryIndex, FieldKey)
        End Sub

        Public Overrides Sub AfterApply()
            Main.RefreshDatField(FieldKey)
        End Sub

        'Keystrokes in one field become one edit while they are close in time.
        Public Overrides Function CanMergeWith(previous As Edit) As Boolean
            Dim p As DatEdit = TryCast(previous, DatEdit)
            If p Is Nothing Then Return False
            If p.DataIndex <> DataIndex OrElse p.EntryIndex <> EntryIndex OrElse p.FieldKey <> FieldKey Then Return False
            Return (Stamp - p.Stamp).TotalMilliseconds <= MergeWindowMs
        End Function

        Public Overrides Sub MergeWith(newer As Edit)
            NewValue = DirectCast(newer, DatEdit).NewValue
            Stamp = newer.Stamp
        End Sub
    End Class

    ''' <summary>A changed wireframe index. Kind 0 is wireframe, 1 is group, 2 is transport.</summary>
    Public Class WireframeEdit
        Inherits Edit

        Public ReadOnly Kind As Integer
        Public ReadOnly EntryIndex As Integer
        Public ReadOnly OldValue As Byte
        Public NewValue As Byte

        Private Const MergeWindowMs As Integer = 900

        Public Sub New(kind As Integer, entryIndex As Integer, oldValue As Byte, newValue As Byte)
            Me.Kind = kind
            Me.EntryIndex = entryIndex
            Me.OldValue = oldValue
            Me.NewValue = newValue
        End Sub

        Public Overrides Sub Undo()
            Apply(OldValue)
        End Sub

        Public Overrides Sub Redo()
            Apply(NewValue)
        End Sub

        Private Sub Apply(value As Byte)
            Select Case Kind
                Case 0 : wireframData(EntryIndex) = value
                Case 1 : grpwireData(EntryIndex) = value
                Case 2 : tranwireData(EntryIndex) = value
            End Select
        End Sub

        Public Overrides Function Describe() As String
            Return WireframeName() & " of " & UnitName(EntryIndex)
        End Function

        Private Function WireframeName() As String
            Select Case Kind
                Case 0 : Return "Wireframe"
                Case 1 : Return "Group wireframe"
                Case Else : Return "Transport wireframe"
            End Select
        End Function

        Public Overrides Sub Reveal()
            Main.RevealWireframe(Kind, EntryIndex)
        End Sub

        Public Overrides Sub AfterApply()
            Main.RefreshFileManager()
        End Sub

        Public Overrides Function CanMergeWith(previous As Edit) As Boolean
            Dim p As WireframeEdit = TryCast(previous, WireframeEdit)
            If p Is Nothing Then Return False
            If p.Kind <> Kind OrElse p.EntryIndex <> EntryIndex Then Return False
            Return (Stamp - p.Stamp).TotalMilliseconds <= MergeWindowMs
        End Function

        Public Overrides Sub MergeWith(newer As Edit)
            NewValue = DirectCast(newer, WireframeEdit).NewValue
            Stamp = newer.Stamp
        End Sub
    End Class

    ''' <summary>A changed entry in the game string table.</summary>
    Public Class StatTextEdit
        Inherits Edit

        Public ReadOnly Index As Integer
        Public ReadOnly OldText As String
        Public NewText As String

        Public Sub New(index As Integer, oldText As String, newText As String)
            Me.Index = index
            Me.OldText = oldText
            Me.NewText = newText
        End Sub

        Public Overrides Sub Undo()
            StatTextAdd(Index, If(OldText, ""))
        End Sub

        Public Overrides Sub Redo()
            StatTextAdd(Index, If(NewText, ""))
        End Sub

        Public Overrides Function Describe() As String
            Return "Text " & (Index + 1)
        End Function

        Public Overrides Sub Reveal()
            Main.RevealStatText(Index)
        End Sub

        Public Overrides Sub AfterApply()
            Main.RefreshFileManager()
        End Sub
    End Class

    ''' <summary>
    ''' A changed button set. FireGraft has 43 places that change this data, so the
    ''' whole button set of one entry is copied before and after instead.
    ''' </summary>
    Friend Class ButtonSetEdit
        Inherits Edit

        Public ReadOnly EntryIndex As Integer
        Public ReadOnly Before As List(Of SBtnDATA)
        Public After As List(Of SBtnDATA)

        Public Sub New(entryIndex As Integer, before As List(Of SBtnDATA), after As List(Of SBtnDATA))
            Me.EntryIndex = entryIndex
            Me.Before = before
            Me.After = after
        End Sub

        Public Overrides Sub Undo()
            Apply(Before)
        End Sub

        Public Overrides Sub Redo()
            Apply(After)
        End Sub

        Private Sub Apply(source As List(Of SBtnDATA))
            ProjectBtnData(EntryIndex) = CloneButtons(source)
        End Sub

        Public Shared Function CloneButtons(source As List(Of SBtnDATA)) As List(Of SBtnDATA)
            Dim copy As New List(Of SBtnDATA)
            If source Is Nothing Then Return copy
            For Each b As SBtnDATA In source
                copy.Add(b.Clone())
            Next
            Return copy
        End Function

        Public Shared Function ButtonsDiffer(a As List(Of SBtnDATA), b As List(Of SBtnDATA)) As Boolean
            If a Is Nothing OrElse b Is Nothing Then Return Not (a Is Nothing AndAlso b Is Nothing)
            If a.Count <> b.Count Then Return True
            For i = 0 To a.Count - 1
                If Not a(i).SameAs(b(i)) Then Return True
            Next
            Return False
        End Function

        Public Overrides Function Describe() As String
            Return "Button set of " & UnitName(EntryIndex)
        End Function

        Public Overrides Sub Reveal()
            Main.RevealFireGraft(1, EntryIndex)
        End Sub

        Public Overrides Sub AfterApply()
            Main.RefreshFireGraft()
        End Sub
    End Class

    ''' <summary>A changed requirement list. Copied whole, for the same reason.</summary>
    Friend Class RequirementEdit
        Inherits Edit

        Public ReadOnly Kind As Integer
        Public ReadOnly EntryIndex As Integer
        Public ReadOnly Before As SReqDATA
        Public After As SReqDATA

        Public Sub New(kind As Integer, entryIndex As Integer, before As SReqDATA, after As SReqDATA)
            Me.Kind = kind
            Me.EntryIndex = entryIndex
            Me.Before = before
            Me.After = after
        End Sub

        Public Overrides Sub Undo()
            ProjectRequireData(Kind)(EntryIndex) = Before.Clone()
        End Sub

        Public Overrides Sub Redo()
            ProjectRequireData(Kind)(EntryIndex) = After.Clone()
        End Sub

        Public Overrides Function Describe() As String
            Return "Requirement " & (EntryIndex + 1)
        End Function

        Public Overrides Sub Reveal()
            Main.RevealFireGraft(2 + Kind, EntryIndex)
        End Sub

        Public Overrides Sub AfterApply()
            Main.RefreshFireGraft()
        End Sub
    End Class



    Friend Module EditLabels
        'Name of an entry in the unit list, for an undo label.
        Friend Function UnitName(index As Integer) As String
            Try
                If index >= 0 AndAlso CODE.Count > 0 AndAlso index < CODE(DTYPE.units).Count Then
                    Return CODE(DTYPE.units)(index)
                End If
            Catch
            End Try
            Return "entry " & index
        End Function
    End Module

    ''' <summary>The undo and redo stacks of the open project.</summary>
    Public Module History
        Private ReadOnly undoStack As New List(Of Edit)
        Private ReadOnly redoStack As New List(Of Edit)
        Private ReadOnly openGroups As New Stack(Of EditGroup)

        Private Const MaxDepth As Integer = 400

        ''' <summary>
        ''' True while the history changes data, and while a project loads.
        ''' The history records nothing then.
        ''' </summary>
        Public Property Suppressed As Boolean

        Public ReadOnly Property CanUndo As Boolean
            Get
                Return undoStack.Count > 0
            End Get
        End Property

        Public ReadOnly Property CanRedo As Boolean
            Get
                Return redoStack.Count > 0
            End Get
        End Property

        Public ReadOnly Property UndoLabel As String
            Get
                If undoStack.Count = 0 Then Return ""
                Return undoStack(undoStack.Count - 1).Describe()
            End Get
        End Property

        Public ReadOnly Property RedoLabel As String
            Get
                If redoStack.Count = 0 Then Return ""
                Return redoStack(redoStack.Count - 1).Describe()
            End Get
        End Property

        Public Sub Clear()
            undoStack.Clear()
            redoStack.Clear()
            openGroups.Clear()
        End Sub

        ''' <summary>Runs an action with recording off. Use it around a project load.</summary>
        Public Sub WithoutRecording(work As System.Action)
            Dim was As Boolean = Suppressed
            Suppressed = True
            Try
                work()
            Finally
                Suppressed = was
            End Try
        End Sub

        ''' <summary>
        ''' Starts a group. Each edit until EndGroup undoes as one step.
        ''' Groups can contain groups. Only the outer group becomes an undo step.
        ''' </summary>
        Public Sub BeginGroup(label As String)
            openGroups.Push(New EditGroup(label))
        End Sub

        Public Sub EndGroup()
            If openGroups.Count = 0 Then Return
            Dim group As EditGroup = openGroups.Pop()
            If group.Edits.Count = 0 Then Return
            If group.Edits.Count = 1 Then
                Push(group.Edits(0))
            Else
                Push(group)
            End If
        End Sub

        ''' <summary>Records one change. The data class calls this after it writes.</summary>
        Public Sub Record(edit As Edit)
            If Suppressed Then Return
            If openGroups.Count > 0 Then
                openGroups.Peek().Edits.Add(edit)
                Return
            End If
            Push(edit)
        End Sub

        Private Sub Push(edit As Edit)
            'A new edit after an undo removes the redo branch.
            redoStack.Clear()

            If undoStack.Count > 0 AndAlso edit.CanMergeWith(undoStack(undoStack.Count - 1)) Then
                undoStack(undoStack.Count - 1).MergeWith(edit)
            Else
                undoStack.Add(edit)
                If undoStack.Count > MaxDepth Then undoStack.RemoveAt(0)
            End If
        End Sub

        Public Sub Undo()
            If undoStack.Count = 0 Then Return
            Dim edit As Edit = undoStack(undoStack.Count - 1)
            undoStack.RemoveAt(undoStack.Count - 1)

            edit.Reveal()
            WithoutRecording(Sub() edit.Undo())
            edit.AfterApply()

            redoStack.Add(edit)
        End Sub

        Public Sub Redo()
            If redoStack.Count = 0 Then Return
            Dim edit As Edit = redoStack(redoStack.Count - 1)
            redoStack.RemoveAt(redoStack.Count - 1)

            edit.Reveal()
            WithoutRecording(Sub() edit.Redo())
            edit.AfterApply()

            undoStack.Add(edit)
        End Sub
    End Module

End Namespace
