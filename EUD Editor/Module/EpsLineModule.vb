Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' Reading and writing one line of epScript: the call it makes, the values it was
''' given, and what a value should be when there is nothing to go on yet.
''' </summary>
Namespace EpsSource

    Public Module EpsLines

        ''' <summary>The name of the call a line makes, or "" when it makes none.</summary>
        Public Function CallOf(text As String) As String
            Dim head As Match = Regex.Match(If(text, "").Trim(), "^([A-Za-z_]\w*)\s*\(")
            Return If(head.Success, head.Groups(1).Value, "")
        End Function

        ''' <summary>What a call was given, split on the commas that stand alone.</summary>
        Public Function ValuesOf(text As String) As List(Of String)
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

        ''' <summary>
        ''' What a value starts as. The first of its list, if it has one, because that
        ''' is a value the game will accept; a plain 0 when it has no list.
        ''' </summary>
        Public Function DefaultFor(value As EpsValue) As String
            If value IsNot Nothing Then
                Dim options As List(Of String) = EpsValueLists.For_(value.Kind)
                If options IsNot Nothing AndAlso options.Count > 0 Then
                    Return EpsValueLists.CodeFor(value.Kind, options(0))
                End If
            End If
            Return "0"
        End Function

        ''' <summary>A whole call, with every value at what it starts as.</summary>
        Public Function EmptyCall(known As EpsCall) As String
            If known Is Nothing Then Return ""
            Dim values As New List(Of String)
            For Each value As EpsValue In known.Values
                values.Add(DefaultFor(value))
            Next
            Return known.Name & "(" & String.Join(", ", values) & ");"
        End Function

        ''' <summary>
        ''' The variables the source declares, so a value can be given one by name
        ''' instead of a number. This is what the old editor calls a Variable.
        ''' </summary>
        Public Function VariablesIn(root As EpsNode) As List(Of String)
            Dim out As New List(Of String)
            If root Is Nothing Then Return out
            For Each node As EpsNode In root.Walk()
                If node.Kind <> EpsKind.Statement Then Continue For
                Dim found As Match = Regex.Match(node.Text.Trim(), "^(?:var|const)\s+([A-Za-z_]\w*)")
                If found.Success AndAlso Not out.Contains(found.Groups(1).Value) Then
                    out.Add(found.Groups(1).Value)
                End If
            Next
            out.Sort(StringComparer.OrdinalIgnoreCase)
            Return out
        End Function

        ''' <summary>
        ''' The words of a description, each with the value it stands for, or -1 when
        ''' it is only words. The editor's tables carry a sentence for the calls of
        ''' the classic set: "Modify death counts for $Player$: $Modifier$ ...".
        ''' </summary>
        Public Function Describe(known As EpsCall, values As IList(Of String)) As List(Of Tuple(Of String, Integer))
            Dim out As New List(Of Tuple(Of String, Integer))
            If known Is Nothing Then Return out

            Dim sentence As String = known.Sentence
            If sentence = "" Then
                sentence = known.Name & "(" &
                           String.Join(", ", known.Values.Select(Function(v) "$" & v.Name & "$")) & ")"
            End If

            Dim at As Integer = 0
            For Each piece As Match In Regex.Matches(sentence, "\$(\w+)\$")
                If piece.Index > at Then
                    out.Add(Tuple.Create(sentence.Substring(at, piece.Index - at), -1))
                End If

                Dim which As Integer = -1
                For i = 0 To known.Values.Count - 1
                    If String.Equals(known.Values(i).Name, piece.Groups(1).Value,
                                     StringComparison.OrdinalIgnoreCase) Then
                        which = i
                        Exit For
                    End If
                Next

                Dim shown As String = piece.Groups(1).Value
                If which >= 0 AndAlso which < values.Count AndAlso values(which) <> "" Then
                    shown = Spoken(known.Values(which).Kind, values(which))
                End If
                out.Add(Tuple.Create(shown, which))
                at = piece.Index + piece.Length
            Next
            If at < sentence.Length Then out.Add(Tuple.Create(sentence.Substring(at), -1))
            Return out
        End Function

        ''' <summary>
        ''' A written value as a person reads it: the word a number stands for, or
        ''' the text without its quotes, or the value itself when it stands for
        ''' nothing and was typed in by hand.
        ''' </summary>
        Public Function Spoken(kind As String, code As String) As String
            Dim choice As String = EpsValueLists.ChoiceFor(kind, code)
            If choice <> "" Then Return choice
            If EpsValueLists.SpellingOf(kind) = EpsSpelling.Quoted Then
                Return EpsValueLists.Unquoted(code)
            End If
            Return If(code, "").Trim()
        End Function

        ''' <summary>
        ''' A line said the way the edit window says it: "Modify death counts for
        ''' Player 2 : Set To 0 for Terran Marine." Gives back "" when the editor has
        ''' no sentence for the call, so the line itself is shown instead.
        ''' </summary>
        Public Function Sentenced(text As String) As String
            Dim known As EpsCall = EpsSymbols.Find(CallOf(text))
            If known Is Nothing OrElse known.Sentence = "" Then Return ""

            Dim out As New StringBuilder()
            For Each piece As Tuple(Of String, Integer) In Describe(known, ValuesOf(text))
                out.Append(piece.Item1)
            Next
            Return Regex.Replace(out.ToString().Trim(), "\s+", " ")
        End Function

        ''' <summary>The same, without the semicolon, for the head of an if or a while.</summary>
        Public Function EmptyTest(known As EpsCall) As String
            Return EmptyCall(known).TrimEnd(";"c)
        End Function
    End Module
End Namespace
