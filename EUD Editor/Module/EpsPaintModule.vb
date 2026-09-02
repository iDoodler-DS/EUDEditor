Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms

''' <summary>
''' epScript, coloured.
'''
''' The source beside the tree is read far more often than it is written, so it
''' is worth reading easily. epScript is spelled the way TypeScript is — the
''' same comments, the same strings, the same braces — so it is coloured the way
''' TypeScript is, with one addition: a call the editor knows and a constant
''' eudplib names are each given a colour of their own, because those are what a
''' person is looking for when they read a trigger.
'''
''' The text is handed over as RTF in one piece. Colouring a box a token at a
''' time means a redraw for each one, which crawls on a source of any size.
''' </summary>
Namespace EpsSource

    Public Module EpsPaint

        'Which colour a piece of the source is given. The order is the order of
        'the colour table written into the RTF, and \cf1 is the first of them.
        Private Enum Ink
            Plain = 1
            Comment = 2
            Text_ = 3
            Number = 4
            Keyword = 5
            Call_ = 6
            Constant = 7
            Punctuation = 8
        End Enum

        'The words epScript keeps for itself. euddraft's own documents and
        'samples use these; anything else is a name.
        Private ReadOnly Keywords As New HashSet(Of String)(StringComparer.Ordinal) From {
            "function", "var", "const", "static", "if", "else", "while", "for",
            "foreach", "return", "break", "continue", "import", "from", "as",
            "true", "false", "null", "in", "of", "new", "del", "class"}

        ''' <summary>Shows the source in the box, coloured.</summary>
        Public Sub Draw(box As RichTextBox, text As String)
            If box Is Nothing Then Return
            Try
                Dim at As Integer = box.GetCharIndexFromPosition(New Point(1, 1))
                box.Rtf = AsRtf(box, If(text, ""))
                'Reading is done from where it was left off, not from the top.
                If at > 0 AndAlso at < box.TextLength Then
                    box.SelectionStart = at
                    box.ScrollToCaret()
                End If
                box.SelectionStart = 0
                box.SelectionLength = 0
            Catch ex As Exception
                LogSuppressed(ex, "EpsPaint.Draw")
                box.Text = text
            End Try
        End Sub

        ''' <summary>The whole source as one RTF document.</summary>
        Private Function AsRtf(box As RichTextBox, text As String) As String
            Dim face As String = If(box.Font Is Nothing, "Consolas", box.Font.Name)
            Dim size As Integer = CInt(If(box.Font Is Nothing, 9.0F, box.Font.Size) * 2)

            Dim out As New StringBuilder()
            out.Append("{\rtf1\ansi\deff0{\fonttbl{\f0\fmodern ").Append(face).Append(";}}")
            out.Append(ColourTable(box))
            out.Append("\f0\fs").Append(size).Append(" ")

            Dim now As Ink = Ink.Plain
            out.Append("\cf").Append(CInt(now)).Append(" ")

            For Each piece As Tuple(Of Ink, String) In Pieces(text)
                If piece.Item1 <> now Then
                    now = piece.Item1
                    out.Append("\cf").Append(CInt(now)).Append(" ")
                End If
                Escaped(out, piece.Item2)
            Next

            out.Append("}")
            Return out.ToString()
        End Function

        ''' <summary>
        ''' The colours, picked to sit on whatever ground the box already has.
        ''' The theme decides that, so the two sets are chosen against it rather
        ''' than written into the form.
        ''' </summary>
        Private Function ColourTable(box As RichTextBox) As String
            Dim ground As Color = box.BackColor
            Dim dark As Boolean = (ground.R * 299 + ground.G * 587 + ground.B * 114) \ 1000 < 128

            Dim ink() As Color
            If dark Then
                ink = {box.ForeColor,
                       Color.FromArgb(106, 153, 85),     'comment
                       Color.FromArgb(206, 145, 120),    'text
                       Color.FromArgb(181, 206, 168),    'number
                       Color.FromArgb(86, 156, 214),     'keyword
                       Color.FromArgb(220, 220, 170),    'a call
                       Color.FromArgb(78, 201, 176),     'a constant
                       Color.FromArgb(160, 160, 160)}    'punctuation
            Else
                ink = {box.ForeColor,
                       Color.FromArgb(0, 128, 0),
                       Color.FromArgb(163, 21, 21),
                       Color.FromArgb(9, 134, 88),
                       Color.FromArgb(0, 0, 255),
                       Color.FromArgb(121, 94, 38),
                       Color.FromArgb(38, 127, 153),
                       Color.FromArgb(96, 96, 96)}
            End If

            Dim out As New StringBuilder("{\colortbl ;")
            For Each one As Color In ink
                out.Append("\red").Append(one.R).Append("\green").Append(one.G).
                    Append("\blue").Append(one.B).Append(";")
            Next
            Return out.Append("}").ToString()
        End Function

        ''' <summary>The source cut into runs, each with the colour it is given.</summary>
        Private Iterator Function Pieces(text As String) As IEnumerable(Of Tuple(Of Ink, String))
            Dim at As Integer = 0
            While at < text.Length
                Dim ch As Char = text(at)

                'A comment runs to the end of its line, or to its closing mark.
                If ch = "/"c AndAlso at + 1 < text.Length AndAlso text(at + 1) = "/"c Then
                    Dim [end] As Integer = text.IndexOf(vbLf, at)
                    If [end] < 0 Then [end] = text.Length
                    Yield Tuple.Create(Ink.Comment, text.Substring(at, [end] - at))
                    at = [end]
                    Continue While
                End If
                If ch = "/"c AndAlso at + 1 < text.Length AndAlso text(at + 1) = "*"c Then
                    Dim [end] As Integer = text.IndexOf("*/", at + 2, StringComparison.Ordinal)
                    [end] = If([end] < 0, text.Length, [end] + 2)
                    Yield Tuple.Create(Ink.Comment, text.Substring(at, [end] - at))
                    at = [end]
                    Continue While
                End If

                'A string runs to its own closing mark, and a backslash inside it
                'takes the next character with it.
                If ch = """"c OrElse ch = "'"c Then
                    Dim [end] As Integer = at + 1
                    While [end] < text.Length
                        If text([end]) = "\"c Then
                            [end] += 2
                            Continue While
                        End If
                        If text([end]) = ch Then
                            [end] += 1
                            Exit While
                        End If
                        [end] += 1
                    End While
                    If [end] > text.Length Then [end] = text.Length
                    Yield Tuple.Create(Ink.Text_, text.Substring(at, [end] - at))
                    at = [end]
                    Continue While
                End If

                If Char.IsDigit(ch) Then
                    Dim [end] As Integer = at
                    While [end] < text.Length AndAlso
                          (Char.IsLetterOrDigit(text([end])) OrElse text([end]) = "."c)
                        [end] += 1
                    End While
                    Yield Tuple.Create(Ink.Number, text.Substring(at, [end] - at))
                    at = [end]
                    Continue While
                End If

                If Char.IsLetter(ch) OrElse ch = "_"c Then
                    Dim [end] As Integer = at
                    While [end] < text.Length AndAlso
                          (Char.IsLetterOrDigit(text([end])) OrElse text([end]) = "_"c)
                        [end] += 1
                    End While
                    Dim word As String = text.Substring(at, [end] - at)
                    Yield Tuple.Create(InkOf(word, text, [end]), word)
                    at = [end]
                    Continue While
                End If

                If Not Char.IsWhiteSpace(ch) AndAlso Not Char.IsLetterOrDigit(ch) Then
                    Yield Tuple.Create(Ink.Punctuation, ch.ToString())
                    at += 1
                    Continue While
                End If

                Yield Tuple.Create(Ink.Plain, ch.ToString())
                at += 1
            End While
        End Function

        ''' <summary>What a word is: a keyword, a call, a constant, or a name.</summary>
        Private Function InkOf(word As String, text As String, after As Integer) As Ink
            If Keywords.Contains(word) Then Return Ink.Keyword

            'A name with a bracket after it is being called. Whether the editor
            'knows the call decides whether it is worth pointing at.
            Dim at As Integer = after
            While at < text.Length AndAlso (text(at) = " "c OrElse text(at) = ChrW(9))
                at += 1
            End While
            If at < text.Length AndAlso text(at) = "("c Then
                Return If(EpsSymbols.Find(word) IsNot Nothing, Ink.Call_, Ink.Plain)
            End If

            If EpsValueLists.IsConstant(word) Then Return Ink.Constant
            Return Ink.Plain
        End Function

        ''' <summary>A run of text, spelled the way RTF wants it.</summary>
        Private Sub Escaped(out As StringBuilder, text As String)
            For Each ch As Char In text
                Select Case ch
                    Case "\"c : out.Append("\\")
                    Case "{"c : out.Append("\{")
                    Case "}"c : out.Append("\}")
                    Case ChrW(10) : out.Append("\par" & vbLf)
                    Case ChrW(13) 'the pair is written once, on the line feed
                    Case ChrW(9) : out.Append("\tab ")
                    Case Else
                        If AscW(ch) > 127 Then
                            out.Append("\u").Append(AscW(ch)).Append("?")
                        Else
                            out.Append(ch)
                        End If
                End Select
            Next
        End Sub
    End Module
End Namespace
