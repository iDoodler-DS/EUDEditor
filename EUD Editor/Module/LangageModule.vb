Imports System.IO
Imports Newtonsoft.Json

Namespace Lan
    Module LanguageModule

        Dim textCache = New Dictionary(Of String, Dictionary(Of String, String))
        Dim msgTextCache = New Dictionary(Of String, String)
        Dim arrayCache = New Dictionary(Of String, Dictionary(Of String, String))


        Private Function getcontrolname(controls As Control)
            Dim _str As New Text.StringBuilder

            If controls.Text <> "" Then
                _str.AppendLine("    """ & controls.Name & """: """ & controls.Text & """,")
            End If


            For i = 0 To controls.Controls.Count - 1
                _str.Append(getcontrolname(controls.Controls(i)))
            Next
            Return _str.ToString
        End Function

        Public Sub GetLanguage(baseform As Form)
            Dim _str As New Text.StringBuilder
            _str.AppendLine("{")
            For i = 0 To baseform.Controls.Count - 1
                _str.Append(getcontrolname(baseform.Controls(i)))
            Next
            _str.Remove(_str.Length - 3, 1)
            _str.AppendLine("}")

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & baseform.Name & ".json"


            Dim filestream As New FileStream(Languagepath, FileMode.Create)
            Dim streamwriter As New StreamWriter(filestream, System.Text.Encoding.UTF8)
            streamwriter.Write(_str.ToString)

            streamwriter.Close()
            filestream.Close()
        End Sub


        Private Function getmeunitem(meunitem As ToolStripMenuItem) As String
            Dim _str As New Text.StringBuilder


            _str.AppendLine("    """ & meunitem.Name & """: """ & meunitem.Text & """,")



            For i = 0 To meunitem.DropDownItems.Count - 1
                Try
                    _str.Append(getmeunitem(meunitem.DropDownItems(i)))
                Catch ex As Exception

                End Try
            Next
            Return _str.ToString
        End Function

        Public Sub GetMenu(baseform As Form, meun As Object, Optional name As String = "")
            Dim _str As New Text.StringBuilder
            _str.AppendLine("{")
            For i = 0 To meun.Items.Count - 1
                Try
                    _str.Append(getmeunitem(meun.Items(i)))
                Catch ex As Exception

                End Try
                '_str.Append(getcontrolname(baseform.Controls(i)))
            Next
            _str.Remove(_str.Length - 3, 1)
            _str.AppendLine("}")

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & baseform.Name & meun.Name & name & ".json"

            Dim filestream As New FileStream(Languagepath, FileMode.Create)
            Dim streamwriter As New StreamWriter(filestream, System.Text.Encoding.UTF8)
            streamwriter.Write(_str.ToString)

            streamwriter.Close()
            filestream.Close()
        End Sub



        Public Sub GetTooltip(baseform As Form, meun As ToolStrip)
            Dim _str As New Text.StringBuilder
            _str.AppendLine("{")
            For Each i In meun.Items
                Try
                    _str.AppendLine("    """ & i.Name & """: """ & i.Text & """,")
                Catch ex As Exception

                End Try
            Next



            _str.AppendLine("}")

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & baseform.Name & meun.Name & ".json"

            Dim filestream As New FileStream(Languagepath, FileMode.Create)
            Dim streamwriter As New StreamWriter(filestream, System.Text.Encoding.UTF8)
            streamwriter.Write(_str.ToString)

            streamwriter.Close()
            filestream.Close()
        End Sub



        Private Sub setcontrols(controls As Control)

            If labels.Keys.Contains(controls.Name) Then
                If labels(controls.Name) <> "" Then
                    controls.Text = labels(controls.Name)
                End If
            End If

            controls.SuspendLayout()
            For i = 0 To controls.Controls.Count - 1
                setcontrols(controls.Controls(i))
            Next
            controls.ResumeLayout()

        End Sub

        Dim labels As Dictionary(Of String, String)
        Public Sub SetLanguage(ByRef forms As Form)
            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & forms.Name & ".json"


            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            labels = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()

            forms.SuspendLayout()
            For i = 0 To forms.Controls.Count - 1
                setcontrols(forms.Controls(i))
                '_str.Append(getcontrolname(baseform.Controls(i)))
            Next
            forms.ResumeLayout()

        End Sub


        Private Sub setmeunitem(meunitem As ToolStripMenuItem)

            If labels.Keys.Contains(meunitem.Name) Then
                If labels(meunitem.Name) <> "" Then
                    meunitem.Text = labels(meunitem.Name)
                End If
            End If

            For i = 0 To meunitem.DropDownItems.Count - 1
                Try
                    setmeunitem(meunitem.DropDownItems(i))
                Catch ex As Exception

                End Try
            Next
        End Sub

        Public Sub SetMenu(ByRef forms As Form, meun As Object, Optional name As String = "")
            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & forms.Name & meun.Name & name & ".json"


            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            labels = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()


            For i = 0 To meun.Items.Count - 1
                Try
                    setmeunitem(meun.Items(i))
                Catch ex As Exception

                End Try
                '_str.Append(getcontrolname(baseform.Controls(i)))
            Next
        End Sub

        Public Sub SetTooltip(forms As Form, meun As ToolStrip)
            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & forms.Name & meun.Name & ".json"


            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            labels = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()


            For i = 0 To meun.Items.Count - 1
                Try
                    meun.Items(i).Text = labels(meun.Items(i).Name)
                Catch ex As Exception

                End Try
                '_str.Append(getcontrolname(baseform.Controls(i)))
            Next
        End Sub

        Public Function GetMsgText(key As String) As String
            If msgTextCache.ContainsKey(key) Then
                Return msgTextCache(key)
            End If

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\Msgbox.json"

            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            Dim dic As Dictionary(Of String, String) = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()

            msgTextCache.Add(key, dic(key))

            Return dic(key)
        End Function
        Public Function GetText(filename As String, key As String) As String

            If textCache.ContainsKey(filename) Then
                Return textCache(filename)(key)
            End If

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & filename & ".json"

            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            Dim dic As Dictionary(Of String, String) = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()

            textCache.Add(filename, dic)

            Return dic(key)
        End Function

        Public Function GetArray(filename As String, key As String) As String()
            If arrayCache.ContainsKey(filename) Then
                Return arrayCache(filename)(key).Split("\")
            End If

            Dim Languagepath As String = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & filename & ".json"

            Dim _filestream As New FileStream(Languagepath, FileMode.Open)
            Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)

            Dim jsonString As String = _streamreader.ReadToEnd


            Dim dic As Dictionary(Of String, String) = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)

            _streamreader.Close()
            _filestream.Close()

            arrayCache.Add(filename, dic)

            Return dic(key).Split("\")
        End Function
    End Module
End Namespace
