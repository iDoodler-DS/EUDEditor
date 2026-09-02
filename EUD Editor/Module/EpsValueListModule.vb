''' <summary>
''' What fills the drop-down of a value.
'''
''' The lists are the editor's own, the same ones the trigger editor of today
''' shows, so a unit is picked from the units of this project and a location from
''' the locations of this map. Nothing here is a second copy of anything.
''' </summary>
Namespace EpsSource

    Public Module EpsValueLists

        Private ReadOnly held As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

        ''' <summary>The choices for a kind of value, or Nothing when it has none.</summary>
        Public Function For_(kind As String) As List(Of String)
            If String.IsNullOrEmpty(kind) Then Return Nothing

            Dim ready As List(Of String) = Nothing
            If held.TryGetValue(kind, ready) Then Return ready

            'The names the tables use are the names the editor's own definitions use,
            'so the kind is asked for as it stands. A kind with nothing behind it
            'gives nothing back, and the value is typed in instead.
            Dim out As List(Of String) = Nothing
            Try
                Dim defs As ValueDefs = GetDefValueDefs(kind)
                If defs IsNot Nothing Then
                    Dim values As String() = defs.GetValues()
                    If values IsNot Nothing AndAlso values.Length > 1 Then
                        out = New List(Of String)(values)
                    End If
                End If
            Catch ex As Exception
                LogSuppressed(ex, "EpsValueLists.For_")
            End Try

            held(kind) = out
            Return out
        End Function

        ''' <summary>Forgets the lists, for when a project opens and they change.</summary>
        Public Sub Forget()
            held.Clear()
        End Sub
    End Module
End Namespace
