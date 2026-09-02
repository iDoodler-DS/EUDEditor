'맵에 삽입은 공통 사용.
'euddraft사용 체크하면 py넣고 아니면 맵에 직접 넣는 거지.

'Size1 423, 212
'Size2 489, 212
'Size3 423, 159

Imports System.IO

Public Class Main

    Dim RecentlyOpenedFiles As New ArrayList()

    Public Sub refreshSet()
        If ProjectSet.isload = True Then
            If ProgramSet.StarVersion = "Remastered" Then
                CheckCompatiblity()
            End If
        End If

        buttonResetting()
        menuResetting()
        nameResetting()
    End Sub

    'Re-applies the theme to the main window and to every editor loaded into a tab.
    'Only needed when the colours change; refreshSet runs after every open, save and
    'close, and walking every loaded editor there was wasted work.
    Public Sub refreshTheme()
        'Recolouring hundreds of controls one by one flickers; show the result in one repaint.
        SuspendDrawing(Me)
        Try
            ThemeSetForm.SetControlColor(Me)
        Finally
            ResumeDrawing(Me)
        End Try
    End Sub

    'Enables the toolbar and menu items that need a project, and decides which tools
    'get a tab (RefreshEditorTabs applies that).
    Public Sub buttonResetting()
        Dim loaded As Boolean = ProjectSet.isload
        Dim is116 As Boolean = (ProgramSet.StarVersion = "1.16.1")

        CheckBox1.Enabled = loaded
        If loaded Then CheckBox1.Checked = ProgramSet.isAutoCompile

        MpainjectWToolStripMenuItem.Enabled = loaded
        EDDOpenDToolStripMenuItem.Enabled = loaded
        ToolStripMenuItem1.Enabled = loaded
        Button14.Enabled = loaded
        Button18.Enabled = loaded

        'Retired for now: TileSet, GRP and BinEditor are unused. Set this to False to
        'bring them back; nothing else about them was removed.
        Const showRetiredTools As Boolean = False

        'BinEditor, GRP, MPQ and Debug only exist for 1.16.1.
        datEditTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(0)
        fireGraftTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(1)
        binEditorTool.Enabled = showRetiredTools AndAlso loaded AndAlso is116 AndAlso ProjectSet.UsedSetting(2)
        tileSetTool.Enabled = showRetiredTools AndAlso loaded AndAlso ProjectSet.UsedSetting(3)
        bgmPlayerTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(4)
        grpTool.Enabled = showRetiredTools AndAlso loaded AndAlso is116 AndAlso ProjectSet.UsedSetting(5)
        triggerEditorTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(6)
        pluginTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(7)
        fileManagerTool.Enabled = loaded AndAlso ProjectSet.UsedSetting(8)
        projectTool.Enabled = loaded
        fileSettingTool.Enabled = loaded
        mpqTool.Enabled = loaded AndAlso is116
        debugTool.Enabled = loaded AndAlso is116
    End Sub

    Public Sub menuResetting()
        SaveToolStripMenuItem.Enabled = ProjectSet.isload
        SaveasToolStripMenuItem.Enabled = ProjectSet.isload
        ProCloseToolStripMenuItem.Enabled = ProjectSet.isload

        PyViewVToolStripMenuItem.Enabled = ProjectSet.isload
        EdsViewEToolStripMenuItem.Enabled = ProjectSet.isload

        btn_Save.Enabled = ProjectSet.isload

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

        SetUpEditorTabs()
        refreshTheme()
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
        Dim themeBefore As String = ThemeSignature()
        SettingForm.PreSizeSet()
        SettingForm.ShowDialog()

        If ThemeSignature() <> themeBefore Then ApplyThemeChange()

        ProjectSet.LoadCHKdata()
        DatEditForm.ReloadCHK()
        refreshSet()
        If EditorLoaded(projectTool) Then ProjectSettingForm.ApplyStarVersion()
        CheckBox1.Checked = ProgramSet.isAutoCompile
        'My.Forms.SettingForm.Location = Me.Location + Button1.Location + New Point(0, 105)
    End Sub

    Private Sub 새로만들기NToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewNToolStripMenuItem.Click
        새로만들기()
    End Sub
    Private Sub 새로만들기()
        If ProjectSet.Close() = True Then
            'Drop the closed project's tabs now; that frees the project settings form for dialog use.
            refreshSet()
            ProjectSet.loading = True

            ProjectSet.Reset()
            ProjectSettingForm.ShowNewProjectDialog()

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

#Region "Undo and redo"
    ' One history for the project, because a project is one document with one Save and
    ' the data editors change parts of it. The trigger editor keeps its own history, so
    ' Ctrl+Z there means "undo the last trigger change"; everywhere else it means
    ' "undo the last data change".

    ''' <summary>Opens the DatEdit tab and shows the field an undo is about to change.</summary>
    Public Sub RevealDatField(dataIndex As Integer, entryIndex As Long, fieldKey As String)
        If Not datEditTool.Enabled Then Return
        If Not EditorLoaded(datEditTool) Then OpenDatEdit()
        SelectTool(datEditTool)
        If EditorLoaded(datEditTool) Then DatEditForm.RevealField(dataIndex, entryIndex, fieldKey)
    End Sub

    ''' <summary>
    ''' Redraws the DatEdit fields after an undo changed a value, then marks the field.
    ''' The controls still hold the old text until this runs.
    ''' </summary>
    Public Sub RefreshDatField(fieldKey As String)
        If Not EditorLoaded(datEditTool) Then Return
        DatEditForm.ReloadAfterUndo(fieldKey)
    End Sub

    ''' <summary>Opens the FileManager tab and shows the wireframe an undo is about to change.</summary>
    Public Sub RevealWireframe(kind As Integer, entryIndex As Integer)
        If Not fileManagerTool.Enabled Then Return
        If Not EditorLoaded(fileManagerTool) Then OpenFileManager()
        SelectTool(fileManagerTool)
        If EditorLoaded(fileManagerTool) Then FileManagerForm.RevealWireframe(kind, entryIndex)
    End Sub

    ''' <summary>Opens the FileManager tab and shows the string an undo is about to change.</summary>
    Public Sub RevealStatText(index As Integer)
        If Not fileManagerTool.Enabled Then Return
        If Not EditorLoaded(fileManagerTool) Then OpenFileManager()
        SelectTool(fileManagerTool)
        If EditorLoaded(fileManagerTool) Then FileManagerForm.RevealStatText(index)
    End Sub

    Public Sub RefreshFileManager()
        If EditorLoaded(fileManagerTool) Then FileManagerForm.ReloadAfterUndo()
    End Sub

    ''' <summary>Opens the FireGraft tab and shows the entry an undo is about to change.</summary>
    Public Sub RevealFireGraft(tabIndex As Integer, entryIndex As Integer)
        If Not fireGraftTool.Enabled Then Return
        If Not EditorLoaded(fireGraftTool) Then OpenFireGraft()
        SelectTool(fireGraftTool)
        If EditorLoaded(fireGraftTool) Then FireGraftForm.RevealEntry(tabIndex, entryIndex)
    End Sub

    Public Sub RefreshFireGraft()
        If EditorLoaded(fireGraftTool) Then FireGraftForm.ReloadAfterUndo()
    End Sub

    'True when the trigger editor owns the open tab and handles undo itself.
    Private Function TriggerTabActive() As Boolean
        Dim page As TabPage = EditorTabControl.SelectedTab
        Return page IsNot Nothing AndAlso page.Tag Is triggerEditorTool AndAlso page.Controls.Count > 0
    End Function

    Private Sub UndoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UndoToolStripMenuItem.Click
        If TriggerTabActive() Then
            TrigEditorForm.UndoFromMenu()
        Else
            EditHistory.History.Undo()
        End If
    End Sub

    Private Sub RedoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RedoToolStripMenuItem.Click
        If TriggerTabActive() Then
            TrigEditorForm.RedoFromMenu()
        Else
            EditHistory.History.Redo()
        End If
    End Sub

    'The labels name the change, so the user knows what Ctrl+Z will do.
    Private Sub EditToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles EditToolStripMenuItem.DropDownOpened
        Dim undoName As String = Lan.GetText(Me.Name, "Undo")
        Dim redoName As String = Lan.GetText(Me.Name, "Redo")

        If TriggerTabActive() Then
            UndoToolStripMenuItem.Enabled = TaskManager.Isundoable
            RedoToolStripMenuItem.Enabled = TaskManager.Isredoable
            UndoToolStripMenuItem.Text = undoName
            RedoToolStripMenuItem.Text = redoName
            Return
        End If

        UndoToolStripMenuItem.Enabled = EditHistory.History.CanUndo
        RedoToolStripMenuItem.Enabled = EditHistory.History.CanRedo
        UndoToolStripMenuItem.Text = If(EditHistory.History.CanUndo, undoName & " " & EditHistory.History.UndoLabel, undoName)
        RedoToolStripMenuItem.Text = If(EditHistory.History.CanRedo, redoName & " " & EditHistory.History.RedoLabel, redoName)
    End Sub
#End Region

#Region "Editor layout throttling"
    ' Every hosted editor is docked to its page, so a main-window resize used to lay
    ' out and repaint every loaded editor on every step, visible or not. Hidden
    ' editors keep their layout suspended until their tab shows. While the window
    ' frame is being dragged, the visible editor stops laying out and drawing after
    ' the first size change and reflows once when the drag ends. (A 20-step resize
    ' with four editors loaded went from 4.0 s to 0.26 s.)

    Private ReadOnly layoutSuspendedEditors As New HashSet(Of Form)
    Private resizeDragging As Boolean
    Private frozenEditor As Form

    Private Shared Function HostedEditor(page As TabPage) As Form
        If page Is Nothing Then Return Nothing
        For Each c As Control In page.Controls
            Dim editor As Form = TryCast(c, Form)
            If editor IsNot Nothing Then Return editor
        Next
        Return Nothing
    End Function

    Private Sub SuspendEditorLayout(editor As Form)
        If editor Is Nothing OrElse editor.IsDisposed OrElse layoutSuspendedEditors.Contains(editor) Then Return
        editor.SuspendLayout()
        layoutSuspendedEditors.Add(editor)
    End Sub

    Private Sub ResumeEditorLayout(editor As Form, performLayout As Boolean)
        If editor Is Nothing OrElse Not layoutSuspendedEditors.Remove(editor) Then Return
        If Not editor.IsDisposed Then editor.ResumeLayout(performLayout)
    End Sub

    Private Sub EditorTabControl_Deselected(sender As Object, e As TabControlEventArgs) Handles EditorTabControl.Deselected
        SuspendEditorLayout(HostedEditor(e.TabPage))
    End Sub

    Private Sub EditorTabControl_Selected(sender As Object, e As TabControlEventArgs) Handles EditorTabControl.Selected
        ResumeEditorLayout(HostedEditor(e.TabPage), True)
    End Sub

    'ResizeBegin/End also bracket a window move; the editor is only frozen once the
    'size really changes, so a move keeps it visible and costs nothing.
    Private Sub Main_ResizeBegin(sender As Object, e As EventArgs) Handles MyBase.ResizeBegin
        resizeDragging = True
    End Sub

    Private Sub Main_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not resizeDragging OrElse frozenEditor IsNot Nothing Then Return
        frozenEditor = HostedEditor(EditorTabControl.SelectedTab)
        If frozenEditor Is Nothing Then Return
        SuspendEditorLayout(frozenEditor)
        SuspendDrawing(frozenEditor)
    End Sub

    Private Sub Main_ResizeEnd(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        resizeDragging = False
        If frozenEditor Is Nothing Then Return
        Dim editor As Form = frozenEditor
        frozenEditor = Nothing
        ResumeEditorLayout(editor, True)
        ResumeDrawing(editor)
    End Sub
#End Region

#Region "Log panel"
    ' Build output. Docked at the bottom of the main window, resized with a splitter,
    ' collapsible to its header bar, and can be popped out into its own window.
    ' Hidden until something is logged.

    Private logCollapsed As Boolean
    Private logExpandedHeight As Integer = 160
    Private logWindow As Form

    Private ReadOnly Property LogPoppedOut As Boolean
        Get
            Return logWindow IsNot Nothing AndAlso LogTextBox.Parent Is logWindow
        End Get
    End Property

    Public ReadOnly Property LogText As String
        Get
            Return LogTextBox.Text
        End Get
    End Property

    'Clears the log and shows it with the given first line.
    Public Sub ResetLog(text As String)
        If InvokeRequired Then
            BeginInvoke(Sub() ResetLog(text))
            Return
        End If
        LogTextBox.Clear()
        AppendLog(text)
    End Sub

    'Appends text, optionally coloured, and makes sure the log is visible. Safe from any thread.
    Public Sub AppendLog(text As String, Optional color As Color = Nothing)
        If InvokeRequired Then
            BeginInvoke(Sub() AppendLog(text, color))
            Return
        End If
        Dim start As Integer = LogTextBox.TextLength
        LogTextBox.AppendText(text)
        If Not color.IsEmpty Then
            LogTextBox.Select(start, text.Length)
            LogTextBox.SelectionColor = color
            LogTextBox.DeselectAll()
        End If
        LogTextBox.SelectionStart = LogTextBox.TextLength
        LogTextBox.ScrollToCaret()
        ShowLog()
    End Sub

    Private Sub ShowLog()
        If LogPoppedOut Then
            If Not logWindow.Visible Then logWindow.Show(Me)
            Return
        End If
        If logCollapsed Then SetLogCollapsed(False)
        LogPanel.Visible = True
        LogSplitter.Visible = True
    End Sub

    'Collapsed, only the header bar stays; the splitter goes with the text.
    Private Sub SetLogCollapsed(collapsed As Boolean)
        If collapsed AndAlso Not logCollapsed Then logExpandedHeight = LogPanel.Height
        logCollapsed = collapsed
        LogTextBox.Visible = Not collapsed
        LogPanel.Height = If(collapsed, LogHeaderPanel.Height, logExpandedHeight)
        LogSplitter.Visible = Not collapsed AndAlso LogPanel.Visible
        LogToggleButton.Text = If(collapsed, "▲", "▼")
    End Sub

    Private Sub LogToggleButton_Click(sender As Object, e As EventArgs) Handles LogToggleButton.Click
        SetLogCollapsed(Not logCollapsed)
    End Sub

    Private Sub LogHeader_DoubleClick(sender As Object, e As EventArgs) Handles LogHeaderPanel.DoubleClick, LogLabel.DoubleClick
        If Not LogPoppedOut Then SetLogCollapsed(Not logCollapsed)
    End Sub

    Private Sub LogPopOutButton_Click(sender As Object, e As EventArgs) Handles LogPopOutButton.Click
        If LogPoppedOut Then
            DockLog()
        Else
            PopOutLog()
        End If
    End Sub

    'Moves the header and text into a floating window owned by the main window.
    Private Sub PopOutLog()
        If logWindow Is Nothing Then
            logWindow = New Form With {
                .Text = LogLabel.Text,
                .ShowInTaskbar = False,
                .ShowIcon = False,
                .StartPosition = FormStartPosition.Manual,
                .MinimumSize = New Size(300, 120),
                .Size = New Size(Math.Max(500, LogPanel.Width), 320)}
            AddHandler logWindow.FormClosing, AddressOf LogWindow_FormClosing
        End If
        If logCollapsed Then SetLogCollapsed(False)
        LogPanel.Visible = False
        LogSplitter.Visible = False

        logWindow.Location = New Point(Left + (Width - logWindow.Width) \ 2, Math.Max(0, Bottom - logWindow.Height - 40))
        logWindow.Controls.Add(LogTextBox)
        logWindow.Controls.Add(LogHeaderPanel)
        LogToggleButton.Visible = False
        LogPopOutButton.Text = "↙"
        ThemeSetForm.SetControlColor(logWindow)
        logWindow.Show(Me)
    End Sub

    Private Sub DockLog()
        logWindow.Hide()
        LogPanel.Controls.Add(LogTextBox)
        LogPanel.Controls.Add(LogHeaderPanel)
        LogToggleButton.Visible = True
        LogPopOutButton.Text = "↗"
        LogPanel.Visible = True
        LogSplitter.Visible = True
    End Sub

    'Closing the floating window docks the log back instead of losing it.
    Private Sub LogWindow_FormClosing(sender As Object, e As FormClosingEventArgs)
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            DockLog()
        End If
    End Sub
#End Region

#Region "Editor tabs"
    ' Every tool has a blank tab while a project is open. The editor form is loaded
    ' into its tab the first time the tab is selected, re-parented as a non-top-level
    ' window so it keeps all of its own code. When an editor closes or hides itself,
    ' the tab goes blank again and reloads on demand.

    'A tool hosted as a tab: its caption key in Main.json, its icon, whether the
    'current project offers it, and how to load its editor.
    Private Class EditorTool
        Public ReadOnly Key As String
        Public ReadOnly Icon As Image
        Public ReadOnly Open As System.Action
        'The editor form, for tools whose Load is safe to run unattended (no dialogs,
        'no side effects); those are warmed up in the background after a project opens.
        Public ReadOnly WarmForm As Func(Of Form)
        Public Enabled As Boolean

        Public Sub New(key As String, icon As Image, open As System.Action, Optional warmForm As Func(Of Form) = Nothing)
            Me.Key = key
            Me.Icon = icon
            Me.Open = open
            Me.WarmForm = warmForm
        End Sub
    End Class

    Private ReadOnly projectTool As New EditorTool("Tool_Project", ProjectFileIcon(), AddressOf OpenProjectSettings)
    Private ReadOnly datEditTool As New EditorTool("Tool_DatEdit", My.Resources.ICON_DatEdit, AddressOf OpenDatEdit, Function() DatEditForm)
    Private ReadOnly fireGraftTool As New EditorTool("Tool_FireGraft", My.Resources.ICON_FireGraft, AddressOf OpenFireGraft, Function() FireGraftForm)
    Private ReadOnly triggerEditorTool As New EditorTool("Tool_TriggerEditor", My.Resources.ICON_TriggerEditor, AddressOf OpenTriggerEditor)
    Private ReadOnly pluginTool As New EditorTool("Tool_Plugin", My.Resources.ICON_plugin, AddressOf OpenPlugin, Function() PluginForm)
    Private ReadOnly fileManagerTool As New EditorTool("Tool_FileManager", My.Resources.ICON_FileManager, AddressOf OpenFileManager, Function() FileManagerForm)
    Private ReadOnly bgmPlayerTool As New EditorTool("Tool_BGMPlayer", My.Resources.ICON_SoundPlayer, AddressOf OpenBGMPlayer, Function() SoundPlayerForm)
    Private ReadOnly fileSettingTool As New EditorTool("Tool_FileSetting", My.Resources.FileSetting, AddressOf OpenFileSetting, Function() FileSettingForm)
    Private ReadOnly mpqTool As New EditorTool("Tool_MPQ", My.Resources.ICON_MPQEditor, AddressOf OpenMPQ)
    Private ReadOnly grpTool As New EditorTool("Tool_GRP", My.Resources.ICON_GRP, AddressOf OpenGRP)
    Private ReadOnly binEditorTool As New EditorTool("Tool_BinEditor", My.Resources.ICON_BinEditor, AddressOf OpenBinEditor)
    Private ReadOnly tileSetTool As New EditorTool("Tool_TileSet", My.Resources.ICON_TileSet, AddressOf OpenTileSet)
    Private ReadOnly debugTool As New EditorTool("Tool_Debug", My.Resources.Debug, AddressOf OpenDebug)

    Private ReadOnly editorClosedActions As New Dictionary(Of Form, System.Action)
    Private suppressTabLoad As Boolean

    'Tools in tab order; each tab page's Tag is its tool.
    Private Function EditorTools() As EditorTool()
        Return {projectTool, datEditTool, fireGraftTool, triggerEditorTool, pluginTool, fileManagerTool, bgmPlayerTool,
                fileSettingTool, mpqTool, grpTool, binEditorTool, tileSetTool, debugTool}
    End Function

    Private Sub SetUpEditorTabs()
        Dim icons As New ImageList With {.ImageSize = New Size(24, 24), .ColorDepth = ColorDepth.Depth32Bit}
        For Each tool As EditorTool In EditorTools()
            'The ImageList keeps the Image itself until its native handle exists, so do not dispose it.
            icons.Images.Add(tool.Key, ScaleIcon(tool.Icon, icons.ImageSize))
        Next
        EditorTabControl.ImageList = icons

        'Editors are created and loaded here, a visible panel parked far outside the
        'client area, so the first paint anyone sees is the finished, themed editor.
        editorStaging = New Panel With {.Location = New Point(-30000, -30000), .Size = New Size(800, 600), .Margin = New Padding(0)}
        Controls.Add(editorStaging)
    End Sub

    Private editorStaging As Panel
    Private ReadOnly parkedEditors As New Dictionary(Of EditorTool, Form)
    Private ReadOnly warmUpQueue As New Queue(Of EditorTool)
    Private warmUpActive As Boolean

    'Creates, themes and fills an editor off-screen. Returns False if it closed itself.
    Private Function PrepareEditor(editor As Form, tool As EditorTool, page As TabPage) As Boolean
        Dim parked As Form = Nothing
        If parkedEditors.TryGetValue(tool, parked) AndAlso parked Is editor AndAlso Not editor.IsDisposed AndAlso editor.Parent Is editorStaging Then
            Return True
        End If
        parkedEditors.Remove(tool)

        editorStaging.Size = EditorTabControl.DisplayRectangle.Size
        editorStaging.Padding = page.Padding
        editor.TopLevel = False
        editor.FormBorderStyle = FormBorderStyle.None
        editor.Dock = DockStyle.Fill
        editorStaging.Controls.Add(editor)
        editor.Show()

        If editor.IsDisposed OrElse editor.Parent IsNot editorStaging Then Return False
        parkedEditors(tool) = editor
        Return True
    End Function

    'Drops an editor that was loaded ahead of time but whose tab went away.
    Private Sub UnparkEditor(tool As EditorTool)
        Dim editor As Form = Nothing
        If Not parkedEditors.TryGetValue(tool, editor) Then Return
        parkedEditors.Remove(tool)
        If editor.IsDisposed Then Return
        editorStaging.Controls.Remove(editor)
        editor.Close()
    End Sub

    'Loads the safe editors one per idle tick, so their tabs open instantly later.
    Private Sub QueueWarmUp()
        warmUpQueue.Clear()
        For Each tool As EditorTool In EditorTools()
            If tool.Enabled AndAlso tool.WarmForm IsNot Nothing Then warmUpQueue.Enqueue(tool)
        Next
        If warmUpQueue.Count > 0 AndAlso Not warmUpActive Then
            warmUpActive = True
            AddHandler Application.Idle, AddressOf WarmUpNextEditor
        End If
    End Sub

    Private Sub WarmUpNextEditor(sender As Object, e As EventArgs)
        While warmUpQueue.Count > 0
            Dim tool As EditorTool = warmUpQueue.Dequeue()
            Dim page As TabPage = FindEditorTab(tool)
            If page Is Nothing OrElse page.Controls.Count > 0 OrElse Not tool.Enabled Then Continue While
            Dim editor As Form = tool.WarmForm()
            If editor.Parent IsNot Nothing Then Continue While
            Try
                PrepareEditor(editor, tool, page)
            Catch ex As Exception
                LogException(ex, "warm up " & tool.Key)
            End Try
            Exit While
        End While
        If warmUpQueue.Count = 0 Then
            RemoveHandler Application.Idle, AddressOf WarmUpNextEditor
            warmUpActive = False
        End If
    End Sub

    'ImageList shrinks with a plain stretch; pre-scale so the 32px tool icons stay crisp.
    Private Shared Function ScaleIcon(source As Image, size As Size) As Bitmap
        Dim bmp As New Bitmap(size.Width, size.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.DrawImage(source, New Rectangle(Point.Empty, size))
        End Using
        Return bmp
    End Function

    'The e2s project file icon shipped in Data\icons (the file association uses it too).
    Private Shared Function ProjectFileIcon() As Image
        Try
            Dim path As String = IO.Path.Combine(My.Application.Info.DirectoryPath, "Data\icons\e2s.ico")
            If IO.File.Exists(path) Then
                Using ico As New Icon(path, 32, 32)
                    Return ico.ToBitmap()
                End Using
            End If
        Catch
        End Try
        Return My.Resources.MapEditor
    End Function

    Private Function ToolText(tool As EditorTool) As String
        Return Lan.GetText(Me.Name, tool.Key)
    End Function

    'Creates the blank tab for a tool and inserts it at the given position.
    Private Function NewEditorTab(tool As EditorTool, index As Integer) As TabPage
        'Small settings-style forms get breathing room; the full-size editors fill the page.
        Dim margin As Integer = If(tool Is projectTool OrElse tool Is pluginTool, 12, 0)
        Dim page As New TabPage(ToolText(tool)) With {.Tag = tool, .Padding = New Padding(margin)}
        'Pages are created after the form was themed; theme the blank page itself.
        ThemeSetForm.SetControlColor(page)
        EditorTabControl.TabPages.Insert(Math.Min(index, EditorTabControl.TabPages.Count), page)
        'The native tab control only picks the icon up once the page has a parent.
        page.ImageKey = tool.Key
        Return page
    End Function

    'Creates blank tabs for the enabled tools and removes the others. Called after buttonResetting.
    Public Sub RefreshEditorTabs()
        Dim wanted As List(Of EditorTool) = EditorTools().Where(Function(t) t.Enabled).ToList()
        suppressTabLoad = True
        SuspendDrawing(EditorTabControl)
        Try

            For Each page As TabPage In EditorTabControl.TabPages.Cast(Of TabPage)().ToArray()
                If Not wanted.Contains(TryCast(page.Tag, EditorTool)) Then
                    DetachEditor(page)
                    UnparkEditor(TryCast(page.Tag, EditorTool))
                    EditorTabControl.TabPages.Remove(page)
                    page.Dispose()
                End If
            Next

            For i = 0 To wanted.Count - 1
                Dim page As TabPage = FindEditorTab(wanted(i))
                If page Is Nothing Then
                    NewEditorTab(wanted(i), i)
                ElseIf page.Text <> ToolText(wanted(i)) Then
                    page.Text = ToolText(wanted(i))
                End If
            Next
        Finally
            suppressTabLoad = False
            ResumeDrawing(EditorTabControl)
        End Try

        'The project tab is first and cheap; show it straight away instead of a blank page.
        Dim selected As TabPage = EditorTabControl.SelectedTab
        If selected IsNot Nothing AndAlso selected.Tag Is projectTool Then LoadSelectedTab()

        RefreshViewMenu(wanted)
        QueueWarmUp()
    End Sub

    'One View menu entry per tab, Ctrl+1 .. Ctrl+9 and Ctrl+0 for the first ten.
    Private Sub RefreshViewMenu(tools As List(Of EditorTool))
        Dim items As ToolStripItemCollection = ViewVToolStripMenuItem.DropDownItems
        For Each item As ToolStripItem In items.Cast(Of ToolStripItem)().ToArray()
            If TypeOf item.Tag Is EditorTool OrElse item.Name = "ViewTabsSeparator" Then items.Remove(item)
        Next
        If tools.Count = 0 Then Return

        items.Add(New ToolStripSeparator With {.Name = "ViewTabsSeparator"})
        For i = 0 To tools.Count - 1
            Dim item As New ToolStripMenuItem(ToolText(tools(i)), Nothing, AddressOf ViewTabMenuItem_Click) With {
                .Tag = tools(i),
                .BackColor = ProgramSet.colorBackground,
                .ForeColor = ProgramSet.colorLabelText}
            If i < 9 Then
                item.ShortcutKeys = Keys.Control Or CType(Keys.D1 + i, Keys)
            ElseIf i = 9 Then
                item.ShortcutKeys = Keys.Control Or Keys.D0
            End If
            items.Add(item)
        Next
    End Sub

    Private Sub ViewTabMenuItem_Click(sender As Object, e As EventArgs)
        SelectTool(DirectCast(DirectCast(sender, ToolStripMenuItem).Tag, EditorTool))
    End Sub

    'Brings a tool's tab to the front, loading its editor if the tab is still blank.
    Private Sub SelectTool(tool As EditorTool)
        Dim page As TabPage = FindEditorTab(tool)
        If page Is Nothing Then Return
        If EditorTabControl.SelectedTab Is page Then
            LoadSelectedTab()
        Else
            EditorTabControl.SelectedTab = page
        End If
    End Sub

    'True when the tool's editor is currently loaded into its tab.
    Private Function EditorLoaded(tool As EditorTool) As Boolean
        Dim page As TabPage = FindEditorTab(tool)
        Return page IsNot Nothing AndAlso page.Controls.Count > 0
    End Function

    'For editors that want to push data into FireGraft only if it exists.
    Public Function FireGraftEditorAlive() As Boolean
        Return EditorLoaded(fireGraftTool) OrElse parkedEditors.ContainsKey(fireGraftTool)
    End Function

    Private Function FindEditorTab(tool As EditorTool) As TabPage
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
    Private Sub ShowEditorTab(editor As Form, tool As EditorTool, Optional onClosed As System.Action = Nothing)
        If onClosed IsNot Nothing Then editorClosedActions(editor) = onClosed

        Dim page As TabPage = FindEditorTab(tool)
        If page Is Nothing Then page = NewEditorTab(tool, EditorTabControl.TabPages.Count)

        If Not page.Controls.Contains(editor) Then
            'Load, theme and fill off-screen (or reuse the warmed-up copy), then move the
            'finished editor onto the page. An editor may close itself during Load
            '(e.g. Debug when StarCraft is not running); its tab then stays blank.
            If Not PrepareEditor(editor, tool, page) Then Return
            parkedEditors.Remove(tool)

            SuspendDrawing(EditorTabControl)
            Try
                DetachEditor(page)
                page.Controls.Add(editor)
                AddHandler editor.FormClosing, AddressOf EditorTab_FormClosing
                AddHandler editor.VisibleChanged, AddressOf EditorTab_VisibleChanged
                suppressTabLoad = True
                EditorTabControl.SelectedTab = page
                suppressTabLoad = False
            Finally
                ResumeDrawing(EditorTabControl)
            End Try
        Else
            suppressTabLoad = True
            EditorTabControl.SelectedTab = page
            suppressTabLoad = False
        End If
        If Not editor.IsDisposed Then editor.Select()
    End Sub

    'Takes the editor out of its tab, leaving the tab blank, and runs the work that
    'used to follow the modal ShowDialog call.
    Private Sub DetachEditor(page As TabPage)
        Dim editor As Form = HostedEditor(page)
        If editor Is Nothing Then Return

        'A hidden tab's editor has its layout suspended; do not carry that out of the tab.
        ResumeEditorLayout(editor, False)
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

    'Lazy load: selecting a blank tab opens its tool, which loads the editor.
    Private Sub LoadSelectedTab()
        Dim page As TabPage = EditorTabControl.SelectedTab
        If page Is Nothing OrElse page.Controls.Count > 0 Then Return
        Dim tool As EditorTool = TryCast(page.Tag, EditorTool)
        If tool IsNot Nothing AndAlso tool.Enabled Then tool.Open()
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

    Private Sub OpenProjectSettings()
        ShowEditorTab(ProjectSettingForm, projectTool)
    End Sub

    'Called by the project settings tab after the user changes something there.
    Public Sub ProjectSettingsChanged(mapsChanged As Boolean)
        If mapsChanged Then
            ProjectSet.LoadCHKdata()
            DatEditForm.ReloadCHK()
        End If
        refreshSet()
    End Sub

    Private Sub OpenDatEdit()
        DatEditForm.Timer1.Enabled = True
        ShowEditorTab(DatEditForm, datEditTool)
    End Sub

    Private Sub OpenFireGraft()
        ShowEditorTab(FireGraftForm, fireGraftTool)
        FireGraftForm.RefreshForm()
    End Sub

    Private Sub OpenTriggerEditor()
        ProjectSet.saveStatus = False
        If ProjectSet.LoadFromCHK = False Then
            MsgBox(Lan.GetText(Me.Name, "CHKMsg"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
        Else
            ShowEditorTab(TrigEditorForm, triggerEditorTool)
        End If
    End Sub

    Private Sub OpenPlugin()
        ProjectSet.saveStatus = False
        ShowEditorTab(PluginForm, pluginTool,
                      Sub()
                          LoadFileimportable()
                          ProjectSet.LoadCHKdata()
                      End Sub)
    End Sub

    Private Sub OpenFileManager()
        ProjectSet.saveStatus = False
        ShowEditorTab(FileManagerForm, fileManagerTool,
                      Sub()
                          LoadFileimportable()
                          DatEditForm.Loadstattxt()
                      End Sub)
    End Sub

    Private Sub OpenBGMPlayer()
        ProjectSet.saveStatus = False
        ShowEditorTab(SoundPlayerForm, bgmPlayerTool)
    End Sub

    Private Sub OpenFileSetting()
        ShowEditorTab(FileSettingForm, fileSettingTool)
    End Sub

    Private Sub OpenMPQ()
        ShowEditorTab(MPQForm, mpqTool)
    End Sub

    Private Sub OpenGRP()
        ShowEditorTab(GRPForm, grpTool)
    End Sub

    Private Sub OpenBinEditor()
        ProjectSet.saveStatus = False
        ShowEditorTab(binEditorForm, binEditorTool)
    End Sub

    Private Sub OpenTileSet()
        ProjectSet.saveStatus = False
        ShowEditorTab(TileSetForm, tileSetTool)
    End Sub

    Private Sub OpenDebug()
        ShowEditorTab(DebugForm, debugTool)
    End Sub
#End Region

    Private Sub TriggerViewFormOpen(sender As Object, e As EventArgs) Handles Button4.Click
        My.Forms.Main.Visible = False
        TriggerViewerForm.ShowDialog()
        My.Forms.Main.Visible = True
        nameResetting()
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

    Private Sub SCRMapDocsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SCRMapDocsToolStripMenuItem.Click
        OpenLink("https://havonz.github.io/SCRMapDocs/")
    End Sub

    Private Sub EuddraftWikiToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EuddraftWikiToolStripMenuItem.Click
        OpenLink("https://github.com/armoha/euddraft/wiki/")
    End Sub

    Private Sub EudBookToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EudBookToolStripMenuItem.Click
        OpenLink("https://armoha.github.io/eud-book/")
    End Sub

    Private Sub OpenLink(url As String)
        Try
            Process.Start(url)
        Catch ex As Exception
            MsgBox(url, MsgBoxStyle.Information, "EUD Editor")
        End Try
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

    'The eight theme colours as one string, to tell whether the Settings dialog changed them.
    Private Function ThemeSignature() As String
        Dim colors As Color() = {ProgramSet.colorFieldText, ProgramSet.colorFieldBackground, ProgramSet.colorChangedBackground,
                                 ProgramSet.colorCheckedBackground, ProgramSet.colorBackground, ProgramSet.colorLabelText,
                                 ProgramSet.colorCodeBackground, ProgramSet.colorPanelBackground}
        Return String.Join("|", colors.Select(Function(c) c.ToArgb().ToString()))
    End Function

    'Saves the new colours and pushes them into every window and loaded editor.
    Private Sub ApplyThemeChange()
        SaveTheme()
        refreshTheme()
        'These editors draw their data with the theme colours; the old theme dialog redrew them too.
        If EditorLoaded(datEditTool) Then DatEditForm.LoadData()
        If EditorLoaded(fileManagerTool) Then FileManagerForm.LoadData()
        If EditorLoaded(fireGraftTool) Then FireGraftForm.LoadData()
    End Sub

End Class
