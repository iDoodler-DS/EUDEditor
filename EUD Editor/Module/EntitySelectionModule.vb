' The entry the user is working on, shared between the editors in the Data tab.
'
' DatEdit owns the list of units, weapons, upgrades and the rest. FireGraft shows
' more fields of the same entries: status flags, requirements and button sets. Each
' editor used to keep its own list and its own selection, so a user moving from the
' stats of a unit to its requirements had to find that unit twice.
'
' The selection is kept per kind of data, because the editors do not always show the
' same kind at the same time. Selecting an upgrade in DatEdit and opening FireGraft's
' upgrade requirements shows that upgrade.
'
' Button sets are the exception. They are numbered in their own list (DTYPE.btnunit),
' not by unit, so that pane keeps a list of its own.

Module EntitySelection

    Private ReadOnly current As New Dictionary(Of Integer, Integer)

    ''' <summary>Raised when the user picks another entry of a kind of data.</summary>
    Public Event Changed(dataType As Integer, index As Integer)

    ''' <summary>Records the entry the user picked, and tells the other editors.</summary>
    Public Sub SetCurrent(dataType As Integer, index As Integer)
        If index < 0 Then Return
        Dim existing As Integer
        If current.TryGetValue(dataType, existing) AndAlso existing = index Then Return
        current(dataType) = index
        RaiseEvent Changed(dataType, index)
    End Sub

    ''' <summary>The entry last picked for this kind of data, or 0.</summary>
    Public Function GetCurrent(dataType As Integer) As Integer
        Dim index As Integer
        If current.TryGetValue(dataType, index) Then Return index
        Return 0
    End Function

    Public Sub Clear()
        current.Clear()
    End Sub

End Module
