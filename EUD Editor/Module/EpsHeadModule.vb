Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' The head of a block, taken apart and put back together.
'''
''' A block is not a line of text to the editor. A function has a name and takes
''' arguments; an if, an else if and a while hold a test made of one or more
''' conditions; a for counts. Each of those is edited as what it is, and only a
''' head the editor cannot read falls back to its own spelling.
''' </summary>
Namespace EpsSource

    Public Enum EpsShape
        ''' <summary>A call: Name(value, value).</summary>
        Call_ = 0
        ''' <summary>function name(argument, argument)</summary>
        Function_ = 1
        ''' <summary>if, else if or while, holding a test.</summary>
        Test = 2
        ''' <summary>for (var i = 0; i &lt; 10; i++)</summary>
        For_ = 3
        ''' <summary>else, and anything else with nothing to fill in.</summary>
        Plain = 4
        ''' <summary>A folder, which has a name.</summary>
        Folder = 5
        ''' <summary>Something the editor has no shape for.</summary>
        Raw = 6
    End Enum

    Public Module EpsHead

        ''' <summary>What shape a node has, so it can be edited as that.</summary>
        Public Function ShapeOf(node As EpsNode) As EpsShape
            If node Is Nothing Then Return EpsShape.Raw

            Select Case node.Kind
                Case EpsKind.Folder : Return EpsShape.Folder
                Case EpsKind.Comment : Return EpsShape.Raw
                Case EpsKind.Block
                    Dim head As String = node.Text.Trim()
                    If Regex.IsMatch(head, "^function\s") Then Return EpsShape.Function_
                    If Regex.IsMatch(head, "^for\s*\(") Then Return EpsShape.For_
                    If Regex.IsMatch(head, "^(if|else\s+if|while)\s*\(") Then Return EpsShape.Test
                    If Regex.IsMatch(head, "^else\s*$") Then Return EpsShape.Plain
                    Return EpsShape.Raw
                Case Else
                    If EpsLines.CallOf(node.Text) <> "" Then Return EpsShape.Call_
                    Return EpsShape.Raw
            End Select
        End Function

#Region "A test: if, else if, while"
        ''' <summary>The word a test block opens with.</summary>
        Public Function KeywordOf(head As String) As String
            Dim found As Match = Regex.Match(If(head, "").Trim(), "^(else\s+if|if|while)\s*\(")
            Return If(found.Success, Regex.Replace(found.Groups(1).Value, "\s+", " "), "if")
        End Function

        ''' <summary>The conditions of a test, one for each part joined by and.</summary>
        Public Function TermsOf(head As String) As List(Of String)
            Dim within As String = Bracketed(head)
            Dim out As New List(Of String)
            If within.Trim() = "" Then Return out

            Dim depth As Integer = 0
            Dim quote As Char = ChrW(0)
            Dim current As New StringBuilder()
            Dim i As Integer = 0
            While i < within.Length
                Dim ch As Char = within(i)
                If quote <> ChrW(0) Then
                    current.Append(ch)
                    If ch = quote Then quote = ChrW(0)
                    i += 1
                    Continue While
                End If

                If ch = """"c OrElse ch = "'"c Then
                    quote = ch
                    current.Append(ch)
                ElseIf ch = "("c OrElse ch = "["c Then
                    depth += 1
                    current.Append(ch)
                ElseIf ch = ")"c OrElse ch = "]"c Then
                    depth -= 1
                    current.Append(ch)
                ElseIf depth = 0 AndAlso ch = "&"c AndAlso i + 1 < within.Length AndAlso within(i + 1) = "&"c Then
                    out.Add(current.ToString().Trim())
                    current.Clear()
                    i += 2
                    Continue While
                Else
                    current.Append(ch)
                End If
                i += 1
            End While
            If current.ToString().Trim() <> "" Then out.Add(current.ToString().Trim())
            Return out
        End Function

        Public Function ComposeTest(keyword As String, terms As IEnumerable(Of String)) As String
            Dim kept As New List(Of String)
            For Each term As String In terms
                If term IsNot Nothing AndAlso term.Trim() <> "" Then kept.Add(term.Trim())
            Next
            If kept.Count = 0 Then kept.Add("Always()")
            Return keyword & " (" & String.Join(" && ", kept) & ")"
        End Function
#End Region

#Region "A function"
        Public Function FunctionName(head As String) As String
            Dim found As Match = Regex.Match(If(head, "").Trim(), "^function\s+([A-Za-z_]\w*)")
            Return If(found.Success, found.Groups(1).Value, "newFunction")
        End Function

        Public Function FunctionArguments(head As String) As List(Of String)
            Dim out As New List(Of String)
            For Each piece As String In Bracketed(head).Split(","c)
                If piece.Trim() <> "" Then out.Add(piece.Trim())
            Next
            Return out
        End Function

        Public Function ComposeFunction(name As String, arguments As IEnumerable(Of String)) As String
            Dim kept As New List(Of String)
            For Each argument As String In arguments
                If argument IsNot Nothing AndAlso argument.Trim() <> "" Then kept.Add(argument.Trim())
            Next
            Dim called As String = If(name Is Nothing OrElse name.Trim() = "", "newFunction", name.Trim())
            Return "function " & called & "(" & String.Join(", ", kept) & ")"
        End Function
#End Region

#Region "A for"
        Public Class ForParts
            Public Property Variable As String = "i"
            Public Property From As String = "0"
            Public Property Comparison As String = "<"
            Public Property Until As String = "10"
            Public Property Step_ As String = "i++"
        End Class

        Public Function ForOf(head As String) As ForParts
            Dim parts As New ForParts()
            Dim within As String = Bracketed(head)
            Dim pieces As String() = within.Split(";"c)
            If pieces.Length < 3 Then Return parts

            Dim first As Match = Regex.Match(pieces(0).Trim(), "^(?:var\s+)?([A-Za-z_]\w*)\s*=\s*(.*)$")
            If first.Success Then
                parts.Variable = first.Groups(1).Value
                parts.From = first.Groups(2).Value.Trim()
            End If

            Dim test As Match = Regex.Match(pieces(1).Trim(), "^[A-Za-z_]\w*\s*(<=|>=|<|>|==|!=)\s*(.*)$")
            If test.Success Then
                parts.Comparison = test.Groups(1).Value
                parts.Until = test.Groups(2).Value.Trim()
            End If

            parts.Step_ = pieces(2).Trim()
            Return parts
        End Function

        Public Function ComposeFor(parts As ForParts) As String
            Dim name As String = If(parts.Variable = "", "i", parts.Variable)
            Dim step_ As String = If(parts.Step_ = "", name & "++", parts.Step_)
            Return "for (var " & name & " = " & If(parts.From = "", "0", parts.From) & "; " &
                   name & " " & parts.Comparison & " " & If(parts.Until = "", "10", parts.Until) & "; " &
                   step_ & ")"
        End Function
#End Region

        ''' <summary>What stands between the first bracket and the last.</summary>
        Public Function Bracketed(text As String) As String
            Dim body As String = If(text, "").Trim()
            Dim opened As Integer = body.IndexOf("("c)
            Dim closed As Integer = body.LastIndexOf(")"c)
            If opened < 0 OrElse closed <= opened Then Return ""
            Return body.Substring(opened + 1, closed - opened - 1)
        End Function
    End Module
End Namespace
