Imports System.IO
Imports Newtonsoft.Json

Namespace Lan
    Module LanguageModule

        'Parsed language files keyed by language and file name, so localising a form
        'again (every editor does it on load) does not re-read and re-parse its json.
        Dim fileCache As New Dictionary(Of String, Dictionary(Of String, String))
        Dim reportedProblems As New HashSet(Of String)

        Private Function LanguageFile(name As String) As String
            Return My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language & "\" & name & ".json"
        End Function

        ''' <summary>Reports a missing file/key once per run, in the error log only.</summary>
        Private Sub ReportProblem(what As String)
            If reportedProblems.Add(what) Then
                LogMessage("Language: " & what)
            End If
        End Sub

        ''' <summary>
        ''' Reads a language json file. Returns an empty dictionary (and logs) when the
        ''' file is missing or invalid, so a missing translation never crashes the editor.
        ''' </summary>
        Private Function ReadLanguageFile(name As String) As Dictionary(Of String, String)
            Dim Languagepath As String = LanguageFile(name)
            Try
                If Not File.Exists(Languagepath) Then
                    ReportProblem("file not found: " & Languagepath)
                    Return New Dictionary(Of String, String)
                End If
                Dim _filestream As New FileStream(Languagepath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim _streamreader As New StreamReader(_filestream, System.Text.Encoding.Default)
                Dim jsonString As String = _streamreader.ReadToEnd
                _streamreader.Close()
                _filestream.Close()
                Dim dic = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonString)
                If dic Is Nothing Then dic = New Dictionary(Of String, String)
                Return dic
            Catch ex As Exception
                ReportProblem("cannot read " & Languagepath & ": " & ex.Message)
                LogException(ex, "language file " & name)
                Return New Dictionary(Of String, String)
            End Try
        End Function

        Private Function CachedLanguageFile(name As String) As Dictionary(Of String, String)
            Dim cacheKey As String = My.Settings.Language & "|" & name
            Dim dic As Dictionary(Of String, String) = Nothing
            If Not fileCache.TryGetValue(cacheKey, dic) Then
                dic = ReadLanguageFile(name)
                fileCache(cacheKey) = dic
            End If
            Return dic
        End Function

        Private Function Lookup(dic As Dictionary(Of String, String), filename As String, key As String) As String
            If dic.ContainsKey(key) Then
                Return dic(key)
            End If
            ReportProblem("missing key '" & key & "' in " & filename & ".json")
            Return key
        End Function


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
                    LogSuppressed(ex, "LangageModule.getmeunitem")

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
                    LogSuppressed(ex, "LangageModule.GetMenu")

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
                    LogSuppressed(ex, "LangageModule.GetTooltip")

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



        Private Function label(key As String) As String
            If key <> "" AndAlso labels.ContainsKey(key) Then
                Return labels(key)
            End If
            Return ""
        End Function

        Private Sub settoolstripitems(items As ToolStripItemCollection)
            For Each item As ToolStripItem In items
                If label(item.Name) <> "" Then
                    item.Text = label(item.Name)
                End If
                If TypeOf item Is ToolStripDropDownItem Then
                    settoolstripitems(DirectCast(item, ToolStripDropDownItem).DropDownItems)
                End If
            Next
        End Sub

        Private Sub setcontrols(controls As Control)

            If label(controls.Name) <> "" Then
                controls.Text = label(controls.Name)
            End If

            'List / drop-down items: "<Name>.Items": "first\second\third"
            If TypeOf controls Is ComboBox OrElse TypeOf controls Is ListBox Then
                Dim itemText As String = label(controls.Name & ".Items")
                If itemText <> "" Then
                    Dim items() As String = itemText.Split("\")
                    If TypeOf controls Is ComboBox Then
                        Dim cb As ComboBox = controls
                        Dim sel As Integer = cb.SelectedIndex
                        cb.Items.Clear()
                        cb.Items.AddRange(items)
                        If sel >= 0 AndAlso sel < items.Length Then cb.SelectedIndex = sel
                    Else
                        Dim lb As ListBox = controls
                        lb.Items.Clear()
                        lb.Items.AddRange(items)
                    End If
                End If
            End If

            If TypeOf controls Is DataGridView Then
                For Each col As DataGridViewColumn In DirectCast(controls, DataGridView).Columns
                    If label(col.Name) <> "" Then col.HeaderText = label(col.Name)
                Next
            ElseIf TypeOf controls Is ListView Then
                For Each col As ColumnHeader In DirectCast(controls, ListView).Columns
                    If label(col.Name) <> "" Then col.Text = label(col.Name)
                Next
            ElseIf TypeOf controls Is ToolStrip Then
                settoolstripitems(DirectCast(controls, ToolStrip).Items)
            End If

            controls.SuspendLayout()
            For i = 0 To controls.Controls.Count - 1
                setcontrols(controls.Controls(i))
            Next
            controls.ResumeLayout()

        End Sub

        Dim labels As Dictionary(Of String, String)
        Public Sub SetLanguage(ByRef forms As Form)
            labels = CachedLanguageFile(forms.Name)
            If labels.Count = 0 Then Return

            'Window title: "FormTitle"
            If label("FormTitle") <> "" Then
                forms.Text = label("FormTitle")
            End If

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

            For Each item As ToolStripItem In meunitem.DropDownItems
                Dim child As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
                If child IsNot Nothing Then setmeunitem(child)
            Next
        End Sub

        Public Sub SetMenu(ByRef forms As Form, meun As Object, Optional name As String = "")
            labels = CachedLanguageFile(forms.Name & meun.Name & name)
            If labels.Count = 0 Then Return

            For Each item As ToolStripItem In meun.Items
                Dim child As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
                If child IsNot Nothing Then setmeunitem(child)
            Next
        End Sub

        Public Sub SetTooltip(forms As Form, meun As ToolStrip)
            labels = CachedLanguageFile(forms.Name & meun.Name)
            If labels.Count = 0 Then Return

            For i = 0 To meun.Items.Count - 1
                Try
                    meun.Items(i).Text = labels(meun.Items(i).Name)
                Catch ex As Exception
                    LogSuppressed(ex, "LangageModule.SetTooltip")

                End Try
                '_str.Append(getcontrolname(baseform.Controls(i)))
            Next
        End Sub

        Public Function GetMsgText(key As String) As String
            Return Lookup(CachedLanguageFile("Msgbox"), "Msgbox", key)
        End Function
        Public Function GetText(filename As String, key As String) As String
            Return Lookup(CachedLanguageFile(filename), filename, key)
        End Function

        Public Function GetArray(filename As String, key As String) As String()
            Return Lookup(CachedLanguageFile(filename), filename, key).Split("\")
        End Function
    End Module
End Namespace
