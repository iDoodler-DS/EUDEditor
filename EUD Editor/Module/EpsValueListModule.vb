''' <summary>
''' What fills the drop-down of a value, and how the choice is spelled in code.
'''
''' The lists are the editor's own, the same ones the trigger editor of today
''' shows, so a unit is picked from the units of this project and a location from
''' the locations of this map. Nothing here is a second copy of anything.
'''
''' A list shows a person one thing and epScript wants another. "Player 1" is
''' read by a person; euddraft wants the 1. So each kind carries a spelling, and
''' every choice goes through it on the way in and on the way out.
''' </summary>
Namespace EpsSource

    ''' <summary>How a choice from a list is written in epScript.</summary>
    Public Enum EpsSpelling
        ''' <summary>As it stands: a number, a name, an expression.</summary>
        Plain = 0
        ''' <summary>As its place in the list. eudplib takes a number for these.</summary>
        Index = 1
        ''' <summary>In quotes: a unit, a location, a line of text.</summary>
        Quoted = 2
    End Enum

    Public Module EpsValueLists

        Private ReadOnly held As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly spelt As New Dictionary(Of String, EpsSpelling)(StringComparer.OrdinalIgnoreCase)

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

        ''' <summary>
        ''' How a kind is written. The editor's own definitions already say it: a
        ''' ListNum holds a number and shows a word, a Combobox holds a name.
        ''' </summary>
        Public Function SpellingOf(kind As String) As EpsSpelling
            If String.IsNullOrEmpty(kind) Then Return EpsSpelling.Plain

            Dim ready As EpsSpelling
            If spelt.TryGetValue(kind, ready) Then Return ready

            Dim out As EpsSpelling = EpsSpelling.Plain
            Try
                Dim defs As ValueDefs = GetDefValueDefs(kind)
                If defs IsNot Nothing Then
                    Select Case defs.type
                        Case ValueDefs.OutPutType.ListNum, ValueDefs.OutPutType.ComboboxNum
                            out = EpsSpelling.Index
                        Case ValueDefs.OutPutType.Combobox, ValueDefs.OutPutType.ComboboxString,
                             ValueDefs.OutPutType.Text, ValueDefs.OutPutType.CText,
                             ValueDefs.OutPutType.RawString
                            out = EpsSpelling.Quoted
                    End Select
                End If
            Catch ex As Exception
                LogSuppressed(ex, "EpsValueLists.SpellingOf")
            End Try

            spelt(kind) = out
            Return out
        End Function

        ''' <summary>What a choice from the list is written as.</summary>
        Public Function CodeFor(kind As String, choice As String) As String
            If choice Is Nothing Then Return ""
            Select Case SpellingOf(kind)
                Case EpsSpelling.Index
                    Dim options As List(Of String) = For_(kind)
                    If options IsNot Nothing Then
                        Dim at As Integer = options.FindIndex(
                            Function(one) String.Equals(one, choice, StringComparison.Ordinal))
                        If at >= 0 Then Return at.ToString(Globalization.CultureInfo.InvariantCulture)
                    End If
                    Return choice
                Case EpsSpelling.Quoted
                    Return """" & choice.Replace("""", "\""") & """"
                Case Else
                    Return choice
            End Select
        End Function

        ''' <summary>
        ''' Which choice a written value stands for, or "" when it stands for none
        ''' and the value was typed in by hand.
        ''' </summary>
        Public Function ChoiceFor(kind As String, code As String) As String
            Dim options As List(Of String) = For_(kind)
            If options Is Nothing OrElse code Is Nothing Then Return ""
            Dim text As String = code.Trim()

            Select Case SpellingOf(kind)
                Case EpsSpelling.Index
                    Dim at As Integer
                    If Integer.TryParse(text, Globalization.NumberStyles.Integer,
                                        Globalization.CultureInfo.InvariantCulture, at) AndAlso
                       at >= 0 AndAlso at < options.Count Then
                        Return options(at)
                    End If
                Case EpsSpelling.Quoted
                    Dim bare As String = Unquoted(text)
                    If options.Contains(bare) Then Return bare
                Case Else
                    If options.Contains(text) Then Return text
            End Select
            Return ""
        End Function

        ''' <summary>A written value with its quotes taken off, if it had any.</summary>
        Public Function Unquoted(code As String) As String
            Dim text As String = If(code, "").Trim()
            If text.Length >= 2 AndAlso
               ((text.StartsWith("""") AndAlso text.EndsWith("""")) OrElse
                (text.StartsWith("'") AndAlso text.EndsWith("'"))) Then
                Return text.Substring(1, text.Length - 2).Replace("\""", """")
            End If
            Return text
        End Function

        ''' <summary>Forgets the lists, for when a project opens and they change.</summary>
        Public Sub Forget()
            held.Clear()
            spelt.Clear()
        End Sub
    End Module
End Namespace
