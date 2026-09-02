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
                If options IsNot Nothing AndAlso options.Count > 0 Then Return options(0)
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

        ''' <summary>The same, without the semicolon, for the head of an if or a while.</summary>
        Public Function EmptyTest(known As EpsCall) As String
            Return EmptyCall(known).TrimEnd(";"c)
        End Function
    End Module
End Namespace
