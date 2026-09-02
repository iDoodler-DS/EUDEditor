Imports System.Text

''' <summary>
''' epScript as a tree, and back again.
'''
''' The trigger editor of today keeps a tree of its own and writes epScript out of
''' it. This reads epScript itself, so the text is the thing that is kept and the
''' tree is one way of looking at it.
'''
''' The reader is not a compiler. It finds the shape of the text: what is a block,
''' what is a statement, what is a comment. That is all a tree needs, and it means
''' a line it does not understand still has a place, spelled the way it was
''' written.
''' </summary>
Namespace EpsSource

    Public Enum EpsKind
        ''' <summary>Something with a head and a body: function, if, while, for.</summary>
        Block = 0
        ''' <summary>One line that does something.</summary>
        Statement = 1
        ''' <summary>A line the writer left for a reader.</summary>
        Comment = 2
        ''' <summary>A folder, which is the editor's own idea, kept in a comment.</summary>
        Folder = 3
        ''' <summary>The whole file.</summary>
        Root = 4
    End Enum

    ''' <summary>One piece of the source.</summary>
    Public Class EpsNode
        Public Property Kind As EpsKind
        ''' <summary>The head of a block, or the whole of a statement.</summary>
        Public Property Text As String = ""
        ''' <summary>Set when this node writes no code, because it is switched off.</summary>
        Public Property Off As Boolean
        Public ReadOnly Children As New List(Of EpsNode)
        Public Property Parent As EpsNode

        'What the head says, when it is a call this editor knows.
        Public Property CallName As String
        Public ReadOnly Values As New List(Of String)

        Public Sub New(kind As EpsKind, Optional text As String = "")
            Me.Kind = kind
            Me.Text = text
        End Sub

        Public Function Add(child As EpsNode) As EpsNode
            child.Parent = Me
            Children.Add(child)
            Return child
        End Function

        Public Sub Remove(child As EpsNode)
            Children.Remove(child)
            child.Parent = Nothing
        End Sub

        Public Iterator Function Walk() As IEnumerable(Of EpsNode)
            Yield Me
            For Each child As EpsNode In Children
                For Each node As EpsNode In child.Walk()
                    Yield node
                Next
            Next
        End Function

        ''' <summary>A short line for a tree, saying what this node is.</summary>
        Public Function Caption() As String
            Dim head As String = Text.Trim()
            Select Case Kind
                Case EpsKind.Root : Return "Triggers"
                Case EpsKind.Folder : Return head
                Case Else
                    If head.Length > 120 Then head = head.Substring(0, 117) & "..."
                    Return head
            End Select
        End Function

        Public Function Clone() As EpsNode
            Dim copy As New EpsNode(Kind, Text) With {.Off = Off, .CallName = CallName}
            copy.Values.AddRange(Values)
            For Each child As EpsNode In Children
                copy.Add(child.Clone())
            Next
            Return copy
        End Function
    End Class


    ''' <summary>Reads epScript into a tree.</summary>
    Public Module EpsReader

        Public Const FolderMark As String = "//@folder "
        Public Const FolderOffMark As String = "//@folder-off "
        Public Const FolderEnd As String = "//@end"
        Public Const OffMark As String = "//@off "

        ''' <summary>The whole of a source file, as a tree.</summary>
        Public Function Parse(text As String) As EpsNode
            Dim root As New EpsNode(EpsKind.Root)
            Dim lines As String() = If(text, "").Replace(vbCrLf, vbLf).Split(CChar(vbLf))
            Dim at As Integer = 0
            Fill(root, lines, at, lines.Length)
            Return root
        End Function

        'Reads lines from `at` up to `stop` into `parent`.
        Private Sub Fill(parent As EpsNode, lines As String(), ByRef at As Integer, stop_ As Integer)
            While at < stop_
                Dim raw As String = lines(at)
                Dim line As String = raw.Trim()
                at += 1

                'A blank line is how a person spaces their work out, so it is kept
                'rather than tidied away.
                If line.Length = 0 Then
                    parent.Add(New EpsNode(EpsKind.Comment, ""))
                    Continue While
                End If

                'A folder is the editor's own idea, written as a comment.
                If line.StartsWith(FolderMark) OrElse line.StartsWith(FolderOffMark) Then
                    Dim off As Boolean = line.StartsWith(FolderOffMark)
                    Dim mark As String = If(off, FolderOffMark, FolderMark)
                    Dim folder As New EpsNode(EpsKind.Folder, line.Substring(mark.Length).Trim()) With {.Off = off}

                    Dim ends As Integer = FindFolderEnd(lines, at, stop_)
                    Dim inside As New List(Of String)
                    For i = at To ends - 1
                        Dim body As String = lines(i)
                        'A folder that is off wrote one mark on each line of its
                        'block. One comes off here; a folder inside takes its own.
                        If off AndAlso body.StartsWith("//") Then body = body.Substring(2)
                        inside.Add(body)
                    Next

                    Dim inner As Integer = 0
                    Dim arr As String() = inside.ToArray()
                    Fill(folder, arr, inner, arr.Length)
                    parent.Add(folder)
                    at = ends + 1
                    Continue While
                End If

                If line = FolderEnd Then Continue While

                'A statement that is switched off keeps its place as a comment.
                If line.StartsWith(OffMark) Then
                    Dim body As String = line.Substring(OffMark.Length).Trim()
                    parent.Add(New EpsNode(EpsKind.Statement, body) With {.Off = True})
                    Continue While
                End If

                'A comment that is a comment.
                If line.StartsWith("//") Then
                    parent.Add(New EpsNode(EpsKind.Comment, line))
                    Continue While
                End If

                If line.StartsWith("/*") Then
                    Dim block As New StringBuilder(raw)
                    While Not line.Contains("*/") AndAlso at < stop_
                        block.AppendLine()
                        block.Append(lines(at))
                        line = lines(at)
                        at += 1
                    End While
                    parent.Add(New EpsNode(EpsKind.Comment, block.ToString()))
                    Continue While
                End If

                'Everything else is a statement, or the head of a block. A head is
                'a line that opens a brace this line does not close.
                Dim gathered As String = raw
                While BraceDepth(gathered) > 0 AndAlso Not OpensBlock(gathered) AndAlso at < stop_
                    gathered = gathered & vbLf & lines(at)
                    at += 1
                End While

                If OpensBlock(gathered) Then
                    Dim head As String = gathered.Trim()
                    If head.EndsWith("{") Then head = head.Substring(0, head.Length - 1).Trim()
                    Dim block As New EpsNode(EpsKind.Block, head)
                    Dim ends As Integer = FindBlockEnd(lines, at, stop_)
                    Dim inner As Integer = at
                    Fill(block, lines, inner, ends)
                    parent.Add(block)
                    at = ends + 1
                    Continue While
                End If

                parent.Add(New EpsNode(EpsKind.Statement, gathered.Trim()))
            End While
        End Sub

        'Where the block that has just opened ends: the line holding its close.
        Private Function FindBlockEnd(lines As String(), at As Integer, stop_ As Integer) As Integer
            Dim open_ As Integer = 1
            While at < stop_
                open_ += BraceDepth(lines(at))
                If open_ <= 0 Then Return at
                at += 1
            End While
            Return stop_
        End Function

        Private Function FindFolderEnd(lines As String(), at As Integer, stop_ As Integer) As Integer
            Dim open_ As Integer = 1
            While at < stop_
                Dim line As String = lines(at).Trim()
                If line.StartsWith(FolderMark) OrElse line.StartsWith(FolderOffMark) Then
                    open_ += 1
                ElseIf line = FolderEnd Then
                    open_ -= 1
                    If open_ = 0 Then Return at
                End If
                at += 1
            End While
            Return stop_
        End Function

        'How many braces a line opens, less how many it closes, with the braces
        'inside text and comments left out of the count.
        Public Function BraceDepth(line As String) As Integer
            Dim depth As Integer = 0
            Dim quote As Char = ChrW(0)
            Dim i As Integer = 0
            While i < line.Length
                Dim ch As Char = line(i)
                If quote <> ChrW(0) Then
                    If ch = "\"c Then
                        i += 2
                        Continue While
                    End If
                    If ch = quote Then quote = ChrW(0)
                ElseIf ch = """"c OrElse ch = "'"c Then
                    quote = ch
                ElseIf ch = "/"c AndAlso i + 1 < line.Length AndAlso line(i + 1) = "/"c Then
                    Exit While
                ElseIf ch = "{"c Then
                    depth += 1
                ElseIf ch = "}"c Then
                    depth -= 1
                End If
                i += 1
            End While
            Return depth
        End Function

        Private Function OpensBlock(text As String) As Boolean
            Return BraceDepth(text) > 0
        End Function
    End Module


    ''' <summary>Writes a tree back as epScript.</summary>
    Public Module EpsWriter

        Public Function Write(root As EpsNode) As String
            Dim out As New List(Of String)
            For Each child As EpsNode In root.Children
                out.AddRange(Lines(child, 0))
            Next
            Return String.Join(vbCrLf, out)
        End Function

        Public Function Lines(node As EpsNode, depth As Integer) As List(Of String)
            Dim pad As String = New String(" "c, depth * 4)
            Dim out As New List(Of String)

            Select Case node.Kind
                Case EpsKind.Folder
                    Dim inside As New List(Of String)
                    For Each child As EpsNode In node.Children
                        inside.AddRange(Lines(child, depth))
                    Next
                    If node.Off Then
                        'A folder that is off takes everything in it with it, so the
                        'whole block is commented, one line at a time.
                        out.Add(pad & EpsReader.FolderOffMark & node.Text)
                        For Each line As String In inside
                            out.Add("//" & line)
                        Next
                    Else
                        out.Add(pad & EpsReader.FolderMark & node.Text)
                        out.AddRange(inside)
                    End If
                    out.Add(pad & EpsReader.FolderEnd)

                Case EpsKind.Block
                    out.Add(pad & node.Text & " {")
                    For Each child As EpsNode In node.Children
                        out.AddRange(Lines(child, depth + 1))
                    Next
                    out.Add(pad & "}")

                Case EpsKind.Comment
                    If node.Text = "" Then
                        out.Add("")
                    Else
                        For Each line As String In node.Text.Replace(vbCrLf, vbLf).Split(CChar(vbLf))
                            out.Add(pad & line.Trim())
                        Next
                    End If

                Case Else
                    Dim body As String = node.Text.Trim()
                    If node.Off Then
                        out.Add(pad & EpsReader.OffMark & body)
                    Else
                        For Each line As String In body.Replace(vbCrLf, vbLf).Split(CChar(vbLf))
                            out.Add(pad & line)
                        Next
                    End If
            End Select

            Return out
        End Function
    End Module
End Namespace
