Imports System.Linq

''' <summary>
''' Puts a filter above the list a trigger uses to pick a value. Some of those lists
''' hold every unit, every string or every location of the map, and the user had to
''' find one entry by eye.
'''
''' The list shows only the entries that match what the user types, but a value is
''' still the place of the entry in the whole list, so this module keeps the whole list
''' and turns a place in the shown list back into a place in the whole one.
''' </summary>
Module PickerFilterModule

    Private Class PickerState
        Public Property Box As TextBox
        Public Property All As New List(Of String)
        'The place in the whole list of each entry now shown.
        Public Property Shown As New List(Of Integer)
        Public Property Updating As Boolean
    End Class

    Private ReadOnly states As New Dictionary(Of ListBox, PickerState)

    ''' <summary>True while the list is being filled again, so a form can skip its handler.</summary>
    Public Function IsUpdating(list As ListBox) As Boolean
        Dim state As PickerState = Nothing
        Return states.TryGetValue(list, state) AndAlso state.Updating
    End Function

    ''' <summary>Gives the list its entries, and a filter box the first time.</summary>
    Public Sub SetItems(list As ListBox, items() As String)
        Dim state As PickerState = Attach(list)
        state.All.Clear()
        state.All.AddRange(If(items, New String() {}))
        Apply(list, state)
    End Sub

    ''' <summary>Selects the entry at this place in the whole list.</summary>
    Public Sub SelectValue(list As ListBox, index As Integer)
        Dim state As PickerState = Nothing
        If Not states.TryGetValue(list, state) Then
            list.SelectedIndex = index
            Return
        End If

        Dim shown As Integer = state.Shown.IndexOf(index)
        If shown < 0 AndAlso index >= 0 AndAlso index < state.All.Count Then
            'The entry the value names is filtered out. Show everything again, so the
            'user sees the value that is set.
            state.Box.Text = ""
            Apply(list, state)
            shown = state.Shown.IndexOf(index)
        End If
        list.SelectedIndex = shown
    End Sub

    ''' <summary>Selects the entry with this text, and says whether it was there.</summary>
    Public Function SelectText(list As ListBox, text As String) As Boolean
        Dim state As PickerState = Nothing
        If Not states.TryGetValue(list, state) Then
            Dim at As Integer = list.Items.IndexOf(text)
            list.SelectedIndex = at
            Return at >= 0
        End If

        Dim index As Integer = state.All.IndexOf(text)
        If index < 0 Then Return False
        SelectValue(list, index)
        Return True
    End Function

    ''' <summary>The place in the whole list of what is selected, or -1.</summary>
    Public Function SelectedValue(list As ListBox) As Integer
        Dim state As PickerState = Nothing
        If Not states.TryGetValue(list, state) Then Return list.SelectedIndex
        If list.SelectedIndex < 0 OrElse list.SelectedIndex >= state.Shown.Count Then Return -1
        Return state.Shown(list.SelectedIndex)
    End Function

    Private Function Attach(list As ListBox) As PickerState
        Dim state As PickerState = Nothing
        If states.TryGetValue(list, state) Then Return state

        state = New PickerState()
        state.Box = New TextBox With {.Dock = DockStyle.Top, .Visible = list.Visible}
        AddHandler state.Box.TextChanged, Sub() Apply(list, state)
        AddHandler list.VisibleChanged, Sub() state.Box.Visible = list.Visible

        If list.Parent IsNot Nothing Then
            list.Parent.Controls.Add(state.Box)
            'A filling control must sit in front of a docked one to keep its room.
            list.BringToFront()
        End If

        Try
            ThemeSetForm.SetControlColor(state.Box)
        Catch ex As Exception
            LogSuppressed(ex, "PickerFilterModule.Attach")
        End Try

        states(list) = state
        Return state
    End Function

    'Fills the list with the entries that match, and keeps what was selected.
    Private Sub Apply(list As ListBox, state As PickerState)
        Dim keep As Integer = SelectedValue(list)
        Dim needle As String = state.Box.Text.Trim()

        state.Updating = True
        list.BeginUpdate()
        Try
            list.Items.Clear()
            state.Shown.Clear()
            For i = 0 To state.All.Count - 1
                If needle = "" OrElse state.All(i).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    state.Shown.Add(i)
                    list.Items.Add(state.All(i))
                End If
            Next
        Finally
            list.EndUpdate()
            state.Updating = False
        End Try

        If keep >= 0 Then
            Dim shown As Integer = state.Shown.IndexOf(keep)
            If shown >= 0 Then list.SelectedIndex = shown
        End If
    End Sub
End Module
