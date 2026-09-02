Imports System.IO

''' <summary>
''' Project settings: the maps, the tools in use and the CHK and trigger options.
''' Hosted as a tab while a project is open, where every change applies at once,
''' and shown as the New Project dialog, where OK creates the project and Cancel
''' abandons it.
''' </summary>
Public Class ProjectSettingForm

    'True while the form sits in the main window's tab strip.
    Private ReadOnly Property Hosted As Boolean
        Get
            Return Not TopLevel
        End Get
    End Property

    'True while the controls are being filled from ProjectSet, so their change
    'handlers do not write the same values back and mark the project changed.
    Private fillingControls As Boolean

    ''' <summary>
    ''' Shows the form as the New Project dialog. The same instance may have been
    ''' hosted in a tab before, so restore what the tab host took away.
    ''' </summary>
    Public Function ShowNewProjectDialog() As DialogResult
        Hide()
        TopLevel = True
        Dock = DockStyle.None
        AutoSize = True
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        'Load only fires on a fresh show; a form that was merely hidden keeps its old state.
        SetUpForCurrentMode()
        Return ShowDialog()
    End Function

    Private Sub ProjectSettingForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetUpForCurrentMode()
    End Sub

    'Being put into a tab does not raise Load when the form was never closed
    '(the previous project's tab was just detached), so refresh here as well.
    Protected Overrides Sub OnParentChanged(e As EventArgs)
        MyBase.OnParentChanged(e)
        If Parent IsNot Nothing AndAlso Hosted Then SetUpForCurrentMode()
    End Sub

    'Fills the controls from ProjectSet and arranges the form for tab or dialog use.
    Private Sub SetUpForCurrentMode()
        Lan.SetLanguage(Me)
        ThemeSetForm.SetControlColor(Me)

        fillingControls = True
        TextBox3.Text = MapName(ProjectSet.InputMap)
        TextBox4.Text = MapName(ProjectSet.OutputMap)
        CheckBox1.Checked = ProjectSet.LoadFromCHK
        CheckBox2.Checked = ProjectSet.TriggerSetTouse
        ComboBox2.SelectedIndex = ProjectSet.TriggerPlayer
        CheckBox3.Checked = ProjectSet.EUDEditorDebug
        CheckBox5.Checked = ProjectSet.epTraceDebug
        If ProjectSet.euddraftuse Then
            RadioButton1.Checked = True
        Else
            RadioButton2.Checked = True
        End If
        '0 DatEdit  1 FireGraft  2 BinEditor  3 TileSet  4 BGMPlayer
        For i = 0 To 4
            CheckedListBox2.SetItemChecked(i, ProjectSet.UsedSetting(i))
        Next
        '5 GRP  6 TriggerEditor  7 Plugin  8 FileManager
        For i = 5 To 8
            CheckedListBox1.SetItemChecked(i - 5, ProjectSet.UsedSetting(i))
        Next
        fillingControls = False
        ApplyStarVersion()

        'The OK/Cancel row only makes sense for the dialog; in the tab changes apply at once.
        FlowLayoutPanel2.Visible = Not Hosted
        If Hosted Then
            AutoSize = False
            AcceptButton = Nothing
            CancelButton = Nothing
        Else
            AcceptButton = Button5
            CancelButton = Button6
            Button5.Enabled = MapsSet()
        End If
    End Sub

    ''' <summary>The debug option only exists for 1.16.1; call again after the version changes.</summary>
    Public Sub ApplyStarVersion()
        GroupBox4.Visible = (ProgramSet.StarVersion = "1.16.1")
    End Sub

    Private Shared Function MapName(path As String) As String
        If path = "" Then Return ". . ."
        Return path.Split("\").Last
    End Function

    Private Function MapsSet() As Boolean
        Return ProjectSet.InputMap <> "" AndAlso ProjectSet.OutputMap <> ""
    End Function

    'Tells the main window about a change made in the tab. The New Project dialog
    'does not need this: its caller loads everything once the dialog closes.
    Private Sub Changed(mapsChanged As Boolean)
        If fillingControls OrElse Not Hosted Then Return
        Main.ProjectSettingsChanged(mapsChanged)
    End Sub

#Region "Maps"
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If SetInputMap() Then Changed(True)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If SetOutputMap() Then Changed(True)
    End Sub

    Public Function SetInputMap() As Boolean
        Dim returnval As Boolean = True
        Dim dialog As DialogResult
        OpenFileDialog1.FileName = ProjectSet.InputMap.Split("\").Last

        OpenFileDialog1.InitialDirectory = ProgramSet.StarDirec.Replace("StarCraft.exe", "") & "maps\"
retry:
        dialog = OpenFileDialog1.ShowDialog()

        If dialog = DialogResult.OK Then
            If ProjectSet.OutputMap = OpenFileDialog1.FileName Then
                MsgBox(Lan.GetMsgText("NotSamename") & vbCrLf & "LoadMap :" & OpenFileDialog1.FileName.Split("\").Last & vbCrLf & "SaveMap :" & ProjectSet.OutputMap.Split("\").Last, MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                GoTo retry
            Else
                ProjectSet.InputMap = OpenFileDialog1.FileName
                Dim fileinfo As New FileInfo(ProjectSet.InputMap)
                Main.LastData = fileinfo.LastWriteTime
                TextBox3.Text = MapName(ProjectSet.InputMap)
            End If
        Else
            returnval = False
        End If

        ProjectSet.saveStatus = False
        Button5.Enabled = MapsSet()

        Return returnval
    End Function

    Public Function SetOutputMap() As Boolean
        Dim returnval As Boolean = True
        Dim dialog As DialogResult

        SaveFileDialog1.InitialDirectory = ProgramSet.StarDirec.Replace("StarCraft.exe", "") & "maps\"
        SaveFileDialog1.FileName = ProjectSet.OutputMap.Split("\").Last
retry:
        dialog = SaveFileDialog1.ShowDialog()

        If dialog = DialogResult.OK Then
            If ProjectSet.InputMap = SaveFileDialog1.FileName Then
                MsgBox(Lan.GetMsgText("NotSamename") & vbCrLf & "LoadMap :" & ProjectSet.InputMap.Split("\").Last & vbCrLf & "SaveMap :" & SaveFileDialog1.FileName.Split("\").Last, MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                GoTo retry
            Else
                ProjectSet.OutputMap = SaveFileDialog1.FileName
                TextBox4.Text = MapName(ProjectSet.OutputMap)
            End If
        Else
            returnval = False
        End If

        ProjectSet.saveStatus = False
        Button5.Enabled = MapsSet()

        Return returnval
    End Function
#End Region

#Region "Options"
    Private Sub CheckedListBox1_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox1.ItemCheck
        If fillingControls Then Return
        ProjectSet.UsedSetting(e.Index + 5) = (e.NewValue = CheckState.Checked)
        ProjectSet.saveStatus = False
        Changed(False)
    End Sub

    Private Sub CheckedListBox2_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox2.ItemCheck
        If fillingControls Then Return
        ProjectSet.UsedSetting(e.Index) = (e.NewValue = CheckState.Checked)
        ProjectSet.saveStatus = False
        Changed(False)
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If fillingControls Then Return
        ProjectSet.LoadFromCHK = CheckBox1.Checked
        Changed(True)
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If fillingControls OrElse Not RadioButton1.Checked Then Return
        ProjectSet.euddraftuse = True
        CheckedListBox1.Visible = True
        ProjectSet.saveStatus = False
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If fillingControls OrElse Not RadioButton2.Checked Then Return
        ProjectSet.euddraftuse = False
        CheckedListBox1.Visible = False
        ProjectSet.saveStatus = False
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If fillingControls Then Return
        ProjectSet.TriggerSetTouse = CheckBox2.Checked
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        If fillingControls Then Return
        ProjectSet.TriggerPlayer = ComboBox2.SelectedIndex
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If fillingControls Then Return
        ProjectSet.EUDEditorDebug = CheckBox3.Checked
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If fillingControls Then Return
        ProjectSet.epTraceDebug = CheckBox5.Checked
    End Sub
#End Region

#Region "New Project dialog buttons"
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Close()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        ProjectSet.loading = False
        Close()
    End Sub
#End Region
End Class
