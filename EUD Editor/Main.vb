'맵에 삽입은 공통 사용.
'euddraft사용 체크하면 py넣고 아니면 맵에 직접 넣는 거지.

'Size1 423, 212
'Size2 489, 212
'Size3 423, 159


Imports System.IO

Public Class Main

    Dim RecentlyOpenedFiles As New ArrayList()

    Public Sub refreshSet()
        ThemeSetForm.SetControlColor(Me)

        If ProjectSet.isload = True Then
            If ProgramSet.StarVersion = "Remastered" Then
                CheckCompatiblity()
            End If
        End If


        buttonResetting()
        menuResetting()
        nameResetting()
    End Sub

    Public Sub buttonResetting()
        If ProgramSet.StarVersion = "1.16.1" Then
            Me.MinimumSize = New Size(423 + 66, 400)
        Else
            Me.MinimumSize = New Size(423 + 66 + 65 + 66, 400)
        End If


        If ProjectSet.isload = True Then '로드 되어 있을 경우
            FlowLayoutPanel2.Enabled = True
            Button2.Enabled = ProjectSet.UsedSetting(0)
            Button3.Enabled = ProjectSet.UsedSetting(1)
            CheckBox1.Enabled = True
            CheckBox1.Checked = ProgramSet.isAutoCompile


            If ProgramSet.StarVersion = "1.16.1" Then
                FlowLayoutPanel4.AutoSize = True

                FlowLayoutPanel2.Enabled = False

                MpainjectWToolStripMenuItem.Enabled = True
                EDDOpenDToolStripMenuItem.Enabled = True
                Button14.Enabled = True
                Button17.Enabled = True
                Button18.Enabled = True
                ToolStripMenuItem1.Enabled = True


                Button5.Enabled = ProjectSet.UsedSetting(2) 'binEdit
                Button6.Enabled = ProjectSet.UsedSetting(3) 'tileSet
                Button7.Enabled = ProjectSet.UsedSetting(4)
                Button8.Enabled = ProjectSet.UsedSetting(5) 'GRP
                Button9.Enabled = ProjectSet.UsedSetting(6)
                Button10.Enabled = ProjectSet.UsedSetting(7) 'Plugin
                Button15.Enabled = ProjectSet.UsedSetting(8) 'FileManger
                Button12.Enabled = True
                Button13.Enabled = True
                Button13.Visible = True

                Button11.Visible = True
                Button11.Enabled = True
                'Button11.Enabled = True
                Button12.Visible = True

                Button5.Visible = True
                ' Button6.Visible = True
                Button8.Visible = True
                'Button7.Enabled = ProjectSet.UsedSetting(4)

                Button13.Enabled = True
            Else
                FlowLayoutPanel4.AutoSize = True

                FlowLayoutPanel2.Enabled = False

                MpainjectWToolStripMenuItem.Enabled = True
                EDDOpenDToolStripMenuItem.Enabled = True
                Button14.Enabled = True
                Button17.Enabled = True
                Button18.Enabled = True
                ToolStripMenuItem1.Enabled = True

                Button5.Enabled = False 'binEdit
                Button5.Visible = False

                Button6.Enabled = ProjectSet.UsedSetting(3) 'tileSet
                'Button6.Visible = False

                Button7.Enabled = ProjectSet.UsedSetting(4)
                Button8.Enabled = False 'GRP
                Button8.Visible = False

                Button9.Enabled = ProjectSet.UsedSetting(6)
                Button10.Enabled = ProjectSet.UsedSetting(7) 'Plugin
                Button15.Enabled = ProjectSet.UsedSetting(8) 'FileManger
                Button12.Enabled = False
                Button13.Enabled = False
                Button13.Visible = False

                Button11.Visible = True
                Button11.Enabled = True
                'Button11.Enabled = True
                Button12.Visible = False

                'Button7.Enabled = ProjectSet.UsedSetting(4)

                Button13.Enabled = True
            End If

        Else '로드 안되어 있을 경우
            Button2.Enabled = False
            Button3.Enabled = False
            CheckBox1.Enabled = False


            If ProgramSet.StarVersion = "1.16.1" Then
                FlowLayoutPanel4.AutoSize = True

                FlowLayoutPanel2.Enabled = False

                MpainjectWToolStripMenuItem.Enabled = False
                EDDOpenDToolStripMenuItem.Enabled = False
                Button14.Enabled = False
                Button17.Enabled = False
                Button18.Enabled = False
                ToolStripMenuItem1.Enabled = False


                Button5.Enabled = False
                Button6.Enabled = False
                Button7.Enabled = False
                Button8.Enabled = False
                Button9.Enabled = False
                Button10.Enabled = False
                Button15.Enabled = False
                Button12.Enabled = False
                Button13.Enabled = False
                Button13.Visible = True

                Button11.Visible = True
                Button11.Enabled = False
                Button12.Visible = True

                Button5.Visible = True
                'Button6.Visible = True
                Button8.Visible = True
                'Button7.Enabled = ProjectSet.UsedSetting(4)

                Button13.Enabled = False
            Else
                FlowLayoutPanel4.AutoSize = True

                FlowLayoutPanel2.Enabled = False

                MpainjectWToolStripMenuItem.Enabled = False
                EDDOpenDToolStripMenuItem.Enabled = False
                Button14.Enabled = False
                Button17.Enabled = False
                Button18.Enabled = False
                ToolStripMenuItem1.Enabled = False

                Button5.Enabled = False 'binEdit
                Button5.Visible = False

                Button6.Enabled = False 'tileSet
                Button6.Visible = False

                Button7.Enabled = False
                Button8.Enabled = False 'GRP
                Button8.Visible = False

                Button9.Enabled = False
                Button10.Enabled = False
                Button15.Enabled = False
                Button12.Enabled = False
                Button13.Visible = False

                Button11.Visible = True
                Button11.Enabled = False
                Button12.Visible = False

                'Button7.Enabled = ProjectSet.UsedSetting(4)

                Button13.Enabled = False
            End If
        End If
    End Sub

    Public Sub menuResetting()
        SaveToolStripMenuItem.Enabled = ProjectSet.isload
        SaveasToolStripMenuItem.Enabled = ProjectSet.isload
        ProCloseToolStripMenuItem.Enabled = ProjectSet.isload

        PyViewVToolStripMenuItem.Enabled = ProjectSet.isload
        EdsViewEToolStripMenuItem.Enabled = ProjectSet.isload


        btn_Save.Enabled = ProjectSet.isload
        btn_close.Enabled = ProjectSet.isload

        RefreshEditorTabs()
    End Sub




    Public Sub nameResetting()
        Dim issaved As String

        If ProjectSet.saveStatus Then
            issaved = "  "
        Else
            issaved = " *"
        End If

        If ProjectSet.isload = True Then
            If ProjectSet.filename = "" Then
                Me.Text = Lan.GetMsgText("Defacultname") & " " & issaved & " -  EUD Editor SE " & ProgramSet.Version & "." & ProgramSet.StarVersion

                If DatEditForm IsNot Nothing Then DatEditForm.Text = ProgramSet.DatEditName & issaved & " " & ProgramSet.Version

                If FireGraftForm IsNot Nothing Then FireGraftForm.Text = ProgramSet.FireGraftName & issaved & " " & ProgramSet.Version
            Else
                Dim name As String = ProjectSet.filename.Split("\").Last

                Me.Text = name & issaved & " -  EUD Editor SE " & ProgramSet.Version & "." & ProgramSet.StarVersion
                If DatEditForm IsNot Nothing Then DatEditForm.Text = name & issaved & " - " & ProgramSet.DatEditName & " " & ProgramSet.Version

                If FireGraftForm IsNot Nothing Then FireGraftForm.Text = name & issaved & " - " & ProgramSet.FireGraftName & " " & ProgramSet.Version
            End If
        Else
            Me.Text = "EUD Editor SE " & ProgramSet.Version & "." & ProgramSet.StarVersion
        End If

    End Sub



    Private Sub Main_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        If ProgramSet.StarVersion = "1.16.1" Then
            If Size.Height > (313 - 52) Then '313
                Button12.Size = New Size(65, 50)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 67
            Else
                Button12.Size = New Size(65, 102)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 135
            End If
        Else
            If Size.Height > (313 - 52 - 52) Then '313
                Button12.Size = New Size(65, 50)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 67
            Else
                Button12.Size = New Size(65, 102)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 135
            End If
        End If
    End Sub

    Dim ShutDown As Boolean = False
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load



        'My.Settings.Reset()
        If init() = False Then
            ShutDown = True
            Me.Close()
        End If
        Lan.SetMenu(Me, MenuStrip1)
        Lan.SetLanguage(Me)

        SaveFileDialog1.Filter = Lan.GetText(Me.Name, "SaveFilter")
        OpenFileDialog1.Filter = Lan.GetText(Me.Name, "OpenFilter")

        RefreshRecentlyOpenedList()

        refreshSet()
    End Sub

    Private Sub Main_Closed(sender As Object, e As FormClosingEventArgs) Handles MyBase.Closing
        If ShutDown = False Then
            'Dim filename As String = My.Application.Info.DirectoryPath & "\Data\temp"
            'System.IO.Directory.Delete(path, False)

            'Try
            '    Dim path As String = My.Application.Info.DirectoryPath & "\Data\temp"
            '    Dim di As New IO.DirectoryInfo(path)
            '    di.Delete(True)
            'Catch ex As Exception

            'End Try



            If ProjectSet.Close() Then
                'StarCraftVisibleForm.Close()

                My.Settings.StarDirec = ProgramSet.StarDirec
                My.Settings.euddraftDirec = ProgramSet.euddraftDirec
                My.Settings.StarVersion = ProgramSet.StarVersion
                My.Settings.AutoCompile = ProgramSet.isAutoCompile

                SaveTheme()

                My.Settings.mpqDirec = String.Join(",", ProgramSet.DatMPQDirec)

                My.Settings.Save()
            Else
                e.Cancel = True
            End If
        End If
    End Sub

    Sub SaveTheme()
        My.Settings.DatEditColor1 = ProgramSet.colorFieldText
        My.Settings.DatEditColor2 = ProgramSet.colorFieldBackground
        My.Settings.DatEditColor3 = ProgramSet.colorChangedBackground
        My.Settings.DatEditColor4 = ProgramSet.colorCheckedBackground
        My.Settings.DatEditColor5 = ProgramSet.colorBackground
        My.Settings.DatEditColor6 = ProgramSet.colorLabelText
        My.Settings.DatEditColor7 = ProgramSet.colorCodeBackground
        My.Settings.DatEditColor8 = ProgramSet.colorPanelBackground
        My.Settings.Save()
    End Sub


    Private Sub Setting_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SettingForm.PreSizeSet()
        SettingForm.ShowDialog()


        ProjectSet.LoadCHKdata()
        DatEditForm.ReloadCHK()
        refreshSet()
        CheckBox1.Checked = ProgramSet.isAutoCompile
        'My.Forms.SettingForm.Location = Me.Location + Button1.Location + New Point(0, 105)
    End Sub

    Private Sub 새로만들기NToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewNToolStripMenuItem.Click
        새로만들기()
    End Sub
    Private Sub 새로만들기()
        If ProjectSet.Close() = True Then
            ProjectSet.loading = True

            ProjectSet.Reset()
            SettingForm.PreSizeSet()
            SettingForm.ShowDialog()


            If ProjectSet.loading = True Then
                ProjectSet.isload = True
                ProjectSet.saveStatus = True
            Else '취소 할 경우
                ProjectSet.Close()
            End If

            ProjectSet.LoadCHKdata()
            LoadFileimportable()
            ProjectSet.loading = False
            refreshSet()
        End If
    End Sub

    Private Sub 프로젝트닫기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ProCloseToolStripMenuItem.Click
        프로젝트닫기()
    End Sub
    Private Sub 프로젝트닫기()
        ProjectSet.Close()

        refreshSet()
    End Sub

    Private Sub 다른이름으로저장ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveasToolStripMenuItem.Click
        다른이름으로저장()
    End Sub
    Private Sub 다른이름으로저장()
        Dim Dialog As DialogResult

        If ProjectSet.filename = "" Then
            SaveFileDialog1.FileName = Lan.GetMsgText("Defacultname")
        Else
            Dim name As String = ProjectSet.filename.Split("\").Last

            SaveFileDialog1.FileName = Mid(name, 1, name.Length - 4)
            SaveFileDialog1.InitialDirectory = ProjectSet.filename.Replace(ProjectSet.filename.Split("\").Last, "")
        End If


        Dialog = SaveFileDialog1.ShowDialog()

        If Dialog = DialogResult.Cancel Then
        Else
            ProjectSet.Save(SaveFileDialog1.FileName)
        End If

        refreshSet()
    End Sub

    Private Sub 열기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        열기()
    End Sub
    Private Sub 열기()
        Dim Dialog As DialogResult



        Dialog = OpenFileDialog1.ShowDialog()
        If Dialog = DialogResult.Cancel Then
        Else
            If ProjectSet.Close() = True Then
                ProjectSet.Load(OpenFileDialog1.FileName)
                CheckMapFile()
                'Load bails out (with a message) on an invalid or incompatible file,
                'leaving no input map; only stamp the map time when we really have one.
                If ProjectSet.isload AndAlso File.Exists(ProjectSet.InputMap) Then
                    Dim fileinfo As New FileInfo(ProjectSet.InputMap)
                    LastData = fileinfo.LastWriteTime
                End If
            End If
        End If


        refreshSet()
    End Sub

    Private Sub 저장ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click
        저장()
    End Sub
    Public Sub 저장()
        Dim extension As String = ProjectSet.filename.Split(".").Last
        'Dim ise2s As Boolean = False
        'Try
        '    If Mid(ProjectSet.filename, ProjectSet.filename.Length - 3) <> ".e2s" Then
        '        ise2s = True
        '    End If
        'Catch ex As Exception

        'End Try


        If ProjectSet.filename = "" Or extension = "ees" Or extension = "mem" Then 'Or ise2s Then
            Dim Dialog As DialogResult


            If ProjectSet.filename = "" Then
                SaveFileDialog1.FileName = Lan.GetMsgText("Defacultname")
            Else
                SaveFileDialog1.FileName = Mid(ProjectSet.filename, 1, ProjectSet.filename.Length - 4)
            End If


            Dialog = SaveFileDialog1.ShowDialog()
            If Dialog = DialogResult.Cancel Then
            Else
                ProjectSet.Save(SaveFileDialog1.FileName)
            End If
        Else
            ProjectSet.Save(ProjectSet.filename)
        End If
        If ProgramSet.isAutoCompile = True Then
            eudplib.Toflie()
        End If
        refreshSet()
    End Sub
    Private Sub 끝내기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        If ProjectSet.Close() Then
            Me.Close()
        End If
    End Sub

    Private Sub MPQFormOpen(sender As Object, e As EventArgs) Handles Button13.Click
        ShowEditorTab(MPQForm, Button13)
    End Sub

    Private Sub GRPFormOpen(sender As Object, e As EventArgs) Handles Button8.Click
        'GRPForm.Location = Me.Location
        'Size = New Size(221, 438)
        ShowEditorTab(GRPForm, Button8)
    End Sub

    Private Sub DatEditFormOpen(sender As Object, e As EventArgs) Handles Button2.Click
        DatEditForm.Timer1.Enabled = True
        ShowEditorTab(DatEditForm, Button2)
    End Sub

    Private Sub FireGraftFormOpen(sender As Object, e As EventArgs) Handles Button3.Click
        ShowEditorTab(FireGraftForm, Button3)
        FireGraftForm.RefreshForm()
    End Sub

#Region "Editor tabs"
    ' Every tool button has a blank tab while a project is open. The editor form is
    ' loaded into its tab the first time the tab is selected (or its button clicked),
    ' re-parented as a non-top-level window so it keeps all of its own code. When an
    ' editor closes or hides itself, the tab goes blank again and reloads on demand.

    Private ReadOnly editorClosedActions As New Dictionary(Of Form, System.Action)
    Private suppressTabLoad As Boolean

    'Tool buttons in tab order; each tab page's Tag is its button.
    Private Function ToolButtons() As Button()
        Return {Button2, Button3, Button9, Button10, Button15, Button7, Button11, Button13, Button8, Button5, Button6, Button12}
    End Function

    'Creates blank tabs for the enabled tools and removes the others. Called after buttonResetting.
    Public Sub RefreshEditorTabs()
        suppressTabLoad = True
        Try
            Dim wanted As New List(Of Button)
            For Each b As Button In ToolButtons()
                If b.Visible AndAlso b.Enabled Then wanted.Add(b)
            Next

            For Each page As TabPage In EditorTabControl.TabPages.Cast(Of TabPage)().ToArray()
                If Not wanted.Contains(TryCast(page.Tag, Button)) Then
                    DetachEditor(page)
                    EditorTabControl.TabPages.Remove(page)
                    page.Dispose()
                End If
            Next

            For i = 0 To wanted.Count - 1
                Dim page As TabPage = FindEditorTab(wanted(i))
                If page Is Nothing Then
                    page = New TabPage(wanted(i).Text) With {.Tag = wanted(i), .Padding = New Padding(0)}
                    EditorTabControl.TabPages.Insert(Math.Min(i, EditorTabControl.TabPages.Count), page)
                Else
                    page.Text = wanted(i).Text
                End If
            Next
        Finally
            suppressTabLoad = False
        End Try
    End Sub

    Private Function FindEditorTab(tool As Button) As TabPage
        For Each page As TabPage In EditorTabControl.TabPages
            If page.Tag Is tool Then Return page
        Next
        Return Nothing
    End Function

    Private Function FindEditorTab(editor As Form) As TabPage
        For Each page As TabPage In EditorTabControl.TabPages
            If page.Controls.Contains(editor) Then Return page
        Next
        Return Nothing
    End Function

    'Loads an editor into its tool's tab (creating the tab if needed) and selects it.
    Public Sub ShowEditorTab(editor As Form, tool As Button, Optional onClosed As System.Action = Nothing)
        If onClosed IsNot Nothing Then editorClosedActions(editor) = onClosed

        Dim page As TabPage = FindEditorTab(tool)
        If page Is Nothing Then
            page = New TabPage(tool.Text) With {.Tag = tool, .Padding = New Padding(0)}
            EditorTabControl.TabPages.Add(page)
        End If

        If Not page.Controls.Contains(editor) Then
            DetachEditor(page)
            editor.TopLevel = False
            editor.FormBorderStyle = FormBorderStyle.None
            editor.Dock = DockStyle.Fill
            page.Controls.Add(editor)
            AddHandler editor.FormClosing, AddressOf EditorTab_FormClosing
            AddHandler editor.VisibleChanged, AddressOf EditorTab_VisibleChanged
        End If

        editor.Show()
        'An editor may close itself during Load (e.g. Debug when StarCraft is not running).
        If editor.IsDisposed OrElse page.IsDisposed OrElse Not page.Controls.Contains(editor) Then Return

        suppressTabLoad = True
        EditorTabControl.SelectedTab = page
        suppressTabLoad = False
        editor.Select()
    End Sub

    'Takes the editor out of its tab, leaving the tab blank, and runs the work that
    'used to follow the modal ShowDialog call.
    Private Sub DetachEditor(page As TabPage)
        If page Is Nothing Then Return
        Dim editor As Form = Nothing
        For Each c As Control In page.Controls
            editor = TryCast(c, Form)
            If editor IsNot Nothing Then Exit For
        Next
        If editor Is Nothing Then Return

        RemoveHandler editor.FormClosing, AddressOf EditorTab_FormClosing
        RemoveHandler editor.VisibleChanged, AddressOf EditorTab_VisibleChanged
        page.Controls.Remove(editor)
        nameResetting()

        Dim onClosed As System.Action = Nothing
        If editorClosedActions.TryGetValue(editor, onClosed) Then
            editorClosedActions.Remove(editor)
            onClosed()
        End If
    End Sub

    'Runs after the editor's own Closing handler (which may cancel and just hide).
    Private Sub EditorTab_FormClosing(sender As Object, e As FormClosingEventArgs)
        DetachEditor(FindEditorTab(DirectCast(sender, Form)))
    End Sub

    'Editors that cancel their close just hide themselves; blank their tab then.
    'Control.Visible is also False while the tab page is merely not selected, so
    'only react when the page itself is visible.
    Private Sub EditorTab_VisibleChanged(sender As Object, e As EventArgs)
        Dim editor As Form = sender
        If editor.IsDisposed OrElse editor.Visible Then Return
        If editor.Parent IsNot Nothing AndAlso editor.Parent.Visible Then DetachEditor(FindEditorTab(editor))
    End Sub

    'Lazy load: selecting a blank tab clicks its tool button, which loads the editor.
    Private Sub LoadSelectedTab()
        Dim page As TabPage = EditorTabControl.SelectedTab
        If page Is Nothing OrElse page.Controls.Count > 0 Then Return
        Dim tool As Button = TryCast(page.Tag, Button)
        If tool IsNot Nothing AndAlso tool.Enabled Then tool.PerformClick()
    End Sub

    Private Sub EditorTabControl_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EditorTabControl.SelectedIndexChanged
        If suppressTabLoad Then Return
        LoadSelectedTab()
    End Sub

    'Clicking the already selected (blank) tab does not change the selection; load it anyway.
    Private Sub EditorTabControl_MouseUp(sender As Object, e As MouseEventArgs) Handles EditorTabControl.MouseUp
        If e.Button <> MouseButtons.Left Then Return
        Dim idx As Integer = EditorTabControl.SelectedIndex
        If idx >= 0 AndAlso EditorTabControl.GetTabRect(idx).Contains(e.Location) Then LoadSelectedTab()
    End Sub
#End Region

    Private Sub TriggerViewFormOpen(sender As Object, e As EventArgs) Handles Button4.Click
        My.Forms.Main.Visible = False
        TriggerViewerForm.ShowDialog()
        My.Forms.Main.Visible = True
        nameResetting()
    End Sub

    Private Sub plugin_Click(sender As Object, e As EventArgs) Handles Button10.Click
        ProjectSet.saveStatus = False

        ShowEditorTab(PluginForm, Button10,
                      Sub()
                          LoadFileimportable()
                          ProjectSet.LoadCHKdata()
                      End Sub)
    End Sub

    Private Sub FileManager_Click(sender As Object, e As EventArgs) Handles Button15.Click
        ProjectSet.saveStatus = False

        ShowEditorTab(FileManagerForm, Button15,
                      Sub()
                          LoadFileimportable()
                          DatEditForm.Loadstattxt()
                      End Sub)
    End Sub


    Private Sub binEditor_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ProjectSet.saveStatus = False

        ShowEditorTab(binEditorForm, Button5)
    End Sub


    Private Sub TileSet_Click(sender As Object, e As EventArgs) Handles Button6.Click
        ProjectSet.saveStatus = False

        ShowEditorTab(TileSetForm, Button6)
    End Sub


    Private Sub DebugFormOpen(sender As Object, e As EventArgs) Handles Button12.Click

        ShowEditorTab(DebugForm, Button12)
    End Sub



    Private Sub btn_close_Click(sender As Object, e As EventArgs) Handles btn_close.Click
        프로젝트닫기()
    End Sub
    Private Sub Btn_Save_Click(sender As Object, e As EventArgs) Handles btn_Save.Click
        저장()
    End Sub
    Private Sub Btn_OpenFile_Click(sender As Object, e As EventArgs) Handles btn_OpenFile.Click
        열기()
    End Sub
    Private Sub Btn_NewFile_Click(sender As Object, e As EventArgs) Handles btn_NewFile.Click
        새로만들기()
    End Sub


    Private Sub EDD켜기DToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EDDOpenDToolStripMenuItem.Click
        eudplib.Toflie(True)
    End Sub

    Private Sub EDD켜기_Click(sender As Object, e As EventArgs) Handles Button17.Click
        eudplib.Toflie(True)
    End Sub
    Private Sub 맵에삽입_Click(sender As Object, e As EventArgs) Handles Button14.Click
        eudplib.Toflie()
    End Sub

    Private Sub 맵에삽입WToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MpainjectWToolStripMenuItem.Click
        eudplib.Toflie()
    End Sub
    Private Sub TriggerCopy_Click(sender As Object, e As EventArgs) Handles Button16.Click
        My.Computer.Clipboard.SetText(TriggerViewerForm.RedrawText())
        MsgBox(Lan.GetMsgText("CopyDone"), MsgBoxStyle.OkOnly, "EUD Editor")
    End Sub


    Private Sub 트리거보기TToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TriggerviewTToolStripMenuItem.Click
        TriggerViewerForm.ShowDialog()
        nameResetting()
    End Sub

    Private Sub 클립보드로EToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToclipEToolStripMenuItem.Click
        My.Computer.Clipboard.SetText(TriggerViewerForm.RedrawText())
        MsgBox(Lan.GetMsgText("CopyDone"), MsgBoxStyle.OkOnly, "EUD Editor")
    End Sub

    Private Sub Py파일보기VToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PyViewVToolStripMenuItem.Click
        Previewer.ispyfile = True
        Previewer.FCTB.Text = eudplib.GetPYtext
        Previewer.ShowDialog()
    End Sub

    Private Sub Eds파일보기EToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EdsViewEToolStripMenuItem.Click
        Previewer.ispyfile = True
        Previewer.FCTB.Text = eudplib.Getedstext

        Previewer.ShowDialog()
    End Sub

    Private Sub 도구TToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolTToolStripMenuItem.DropDownOpened
        If ProjectSet.euddraftuse = True And ProjectSet.isload = True Then
            TriggerviewTToolStripMenuItem.Enabled = False
            MpainjectWToolStripMenuItem.Enabled = True
            EDDOpenDToolStripMenuItem.Enabled = True
            ToclipEToolStripMenuItem.Enabled = False
        Else
            TriggerviewTToolStripMenuItem.Enabled = False 'True
            MpainjectWToolStripMenuItem.Enabled = False
            EDDOpenDToolStripMenuItem.Enabled = False
            ToclipEToolStripMenuItem.Enabled = False 'True
        End If
    End Sub

    Private Sub 블로그설명서ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BlogHelpToolStripMenuItem.Click
        HelpForm.Show()
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        ProjectSet.saveStatus = False

        If ProjectSet.LoadFromCHK = False Then
            MsgBox(Lan.GetText(Me.Name, "CHKMsg"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
        Else
            BulidForm.Close()
            ShowEditorTab(TrigEditorForm, Button9)
        End If
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Try
            Dim process As New Process
            Dim startInfo As New ProcessStartInfo


            startInfo.FileName = ProjectSet.InputMap
            process.StartInfo = startInfo
            process.Start()
        Catch ex As Exception
            MsgBox(Lan.GetText(Me.Name, "MapisNotExit"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
        End Try

    End Sub

    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        Try
            Dim process As New Process
            Dim startInfo As New ProcessStartInfo


            startInfo.FileName = ProjectSet.InputMap
            process.StartInfo = startInfo
            process.Start()
        Catch ex As Exception
            MsgBox(Lan.GetText(Me.Name, "MapisNotExit"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
        End Try
    End Sub

    Public LastData As Date
    Private Sub CheckMapWrite_Tick(sender As Object, e As EventArgs) Handles CheckMapWrite.Tick
        If ProjectSet.isload = True Then
            If CheckFileExist(ProjectSet.InputMap) = False Then
                Dim fileinfo As New FileInfo(ProjectSet.InputMap)
                If LastData < fileinfo.LastWriteTime Then
                    LastData = fileinfo.LastWriteTime

                    ProjectSet.LoadCHKdata()
                    If ProgramSet.isAutoCompile = True Then
                        eudplib.Toflie()
                    End If
                End If
            End If
        End If


    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        ProjectSet.saveStatus = False

        ShowEditorTab(SoundPlayerForm, Button7)
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        ShowEditorTab(FileSettingForm, Button11)
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        ProgramSet.isAutoCompile = CheckBox1.Checked
    End Sub

    Private Sub UpdateViewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UpdateViewToolStripMenuItem.Click
        CheckUpdateForm.ShowDialog()
    End Sub

    Private Sub RecentFileMenuItem_Click(sender As ToolStripMenuItem, e As EventArgs)
        Dim fileName = sender.Text
        If ProjectSet.Close() = True Then
            ProjectSet.Load(fileName)
            CheckMapFile()
            Dim fileinfo As New FileInfo(ProjectSet.InputMap)
            LastData = fileinfo.LastWriteTime
        End If
        refreshSet()
    End Sub
    Public Sub SaveRecentFile(strPath As String)
        OpenRecentToolStripMenuItem.DropDownItems.Clear()
        LoadRecentList()
        If (RecentlyOpenedFiles.Contains(strPath)) Then
            RecentlyOpenedFiles.Remove(strPath)
        End If
        RecentlyOpenedFiles.Add(strPath)
        While RecentlyOpenedFiles.Count > 15
            RecentlyOpenedFiles.RemoveAt(0)
        End While
        Dim stringToWrite As New StreamWriter(System.Environment.CurrentDirectory + "\Recent.txt")
        For Each item As String In RecentlyOpenedFiles
            stringToWrite.WriteLine(item)
        Next
        stringToWrite.Flush()
        stringToWrite.Close()
    End Sub

    Private Sub LoadRecentList()
        RecentlyOpenedFiles.Clear()
        Try
            Dim srStream As New StreamReader(System.Environment.CurrentDirectory + "\Recent.txt")
            Dim strLine As String = ""
            While (InlineAssignHelper(strLine, srStream.ReadLine())) IsNot Nothing
                RecentlyOpenedFiles.Add(strLine)
            End While
            srStream.Close()
        Catch ex As Exception
        End Try
        RecentlyOpenedFiles.Reverse()
        OpenRecentToolStripMenuItem.Visible = RecentlyOpenedFiles.Count > 0
    End Sub

    Public Sub RefreshRecentlyOpenedList()
        LoadRecentList()
        For Each item As String In RecentlyOpenedFiles
            Dim fileRecent As New ToolStripMenuItem(item, Nothing, New EventHandler(AddressOf RecentFileMenuItem_Click))
            OpenRecentToolStripMenuItem.DropDownItems.Add(fileRecent)
        Next
    End Sub

    Private Shared Function InlineAssignHelper(Of T) _
          (ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Private Sub ThemeSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ThemeSettingsToolStripMenuItem.Click
        ThemeSetForm.ShowDialog()
        SaveTheme()
        refreshSet()
    End Sub
End Class
