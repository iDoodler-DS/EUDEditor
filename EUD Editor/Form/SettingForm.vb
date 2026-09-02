Imports System.IO

''' <summary>
''' Program settings: StarCraft version and paths, language, auto-compile and the
''' colour theme. Project settings live in ProjectSettingForm, hosted as a tab.
''' </summary>
Public Class SettingForm
    Dim FormStatus As Integer = 0

    Enum Satus
        programset = 1
        projectset = 2
    End Enum

    Public Sub PreSizeSet()
        If ProjectSet.isload = False Then
            Button6.DialogResult = DialogResult.None
        Else
            Button6.DialogResult = DialogResult.Cancel
        End If
    End Sub

    'OK needs both paths; until then Cancel (which quits the editor) is offered.
    Private Sub CheckButton5()
        If ProgramSet.StarDirec = "" Or ProgramSet.euddraftDirec = "" Then
            Button5.Enabled = False
            Button5.Size = New Size(210, 34)
            Button6.Visible = True
        Else
            Button5.Enabled = True
            Button5.Size = New Size(420, 34)
            Button6.Visible = False
        End If
    End Sub

    Private Sub SettingForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Lan.GetLanguage(Me)
        fillingTheme = True
        Lan.SetLanguage(Me)
        fillingTheme = False
        ComboBox3.Items.Clear()
        Dim Folder As String = My.Application.Info.DirectoryPath & "\Data\Language"
        For Each _file As String In IO.Directory.GetDirectories(Folder)
            ComboBox3.Items.Add(_file.Split("\").Last)
        Next
        For i = 0 To ComboBox3.Items.Count - 1
            If My.Settings.Language = ComboBox3.Items(i) Then
                ComboBox3.SelectedIndex = i
            End If
        Next

        ShowThemeColors()

        ComboBox1.SelectedItem = ProgramSet.StarVersion
        TextBox1.Text = ProgramSet.StarDirec.Split("\").Last
        TextBox2.Text = ProgramSet.euddraftDirec.Split("\").Last
        CheckBox4.Checked = ProgramSet.isAutoCompile

        Button5.Visible = True
        If ProjectSet.isload = False Then
            FormStatus = Satus.programset
            CheckButton5()
        Else
            FormStatus = Satus.projectset
            Button5.Enabled = True
            Button5.Size = New Size(420, 34)
            Button6.Visible = False
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Me.Close()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        ProgramSet.StarVersion = ComboBox1.SelectedItem
        ProjectSet.saveStatus = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim dialog As DialogResult
        StarCraftOFD.InitialDirectory = ProgramSet.StarDirec.Replace("StarCraft.exe", "")

        dialog = StarCraftOFD.ShowDialog()

        If dialog = DialogResult.OK Then
            ProgramSet.StarDirec = StarCraftOFD.FileName
            TextBox1.Text = ProgramSet.StarDirec.Split("\").Last
        End If
        CheckButton5()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim dialog As DialogResult
        euddraftOFD.InitialDirectory = ProgramSet.euddraftDirec.Replace("euddraft.exe", "")

        dialog = euddraftOFD.ShowDialog()

        If dialog = DialogResult.OK Then
            ProgramSet.euddraftDirec = euddraftOFD.FileName
            TextBox2.Text = ProgramSet.euddraftDirec.Split("\").Last
        End If
        CheckButton5()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Select Case FormStatus
            Case Satus.programset
                Dim dialog As DialogResult
                dialog = MsgBox(Lan.GetMsgText("Exit"), MsgBoxStyle.OkCancel, ProgramSet.AlterFormMessage)

                If dialog = DialogResult.OK Then
                    Me.Close()
                End If
            Case Satus.projectset
                Close()
        End Select
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        My.Settings.Language = ComboBox3.SelectedItem
        fillingTheme = True
        Lan.SetLanguage(Me)
        fillingTheme = False
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        ProgramSet.isAutoCompile = CheckBox4.Checked
    End Sub

#Region "Theme"
    'True while the theme controls are being filled, so the preset combo does not re-apply.
    Private fillingTheme As Boolean

    'Presets in ThemePresetCombo order; 0 is "Custom".
    Private Sub ApplyThemePreset(index As Integer)
        Select Case index
            Case 1 'DatEdit
                ThemeSetForm.ApplyLightMode(Color.White, Color.Black, Color.DarkCyan, Color.DarkGray)
            Case 2 'EUD Editor
                ThemeSetForm.ApplyLightMode(Color.Black, Color.White, Color.PaleGreen, Color.LightGray)
            Case 3 'EUD Editor2
                ThemeSetForm.ApplyLightMode(Color.White, Color.FromArgb(&HFF193333), Color.DarkSlateBlue, Color.FromArgb(&HFF538585))
            Case 4 'Dark Mode
                ThemeSetForm.ApplyDarkMode()
        End Select
    End Sub

    'Which preset the current colours match, or 0 for a custom mix.
    Private Function CurrentThemePreset() As Integer
        Dim fieldText As Color = ProgramSet.colorFieldText
        Dim fieldBack As Color = ProgramSet.colorFieldBackground
        Dim changed As Color = ProgramSet.colorChangedBackground
        Dim checked As Color = ProgramSet.colorCheckedBackground
        If fieldText = Color.White AndAlso fieldBack = Color.Black AndAlso changed = Color.DarkCyan AndAlso checked = Color.DarkGray Then Return 1
        If fieldText = Color.Black AndAlso fieldBack = Color.White AndAlso changed = Color.PaleGreen AndAlso checked = Color.LightGray Then Return 2
        If fieldText = Color.White AndAlso fieldBack = Color.FromArgb(&HFF193333) AndAlso changed = Color.DarkSlateBlue AndAlso checked = Color.FromArgb(&HFF538585) Then Return 3
        If ProgramSet.colorBackground = ThemeSetForm.darkModeColorBackground AndAlso
           ProgramSet.colorLabelText = ThemeSetForm.darkModeColorLabelText AndAlso
           fieldBack = ThemeSetForm.darkModeColorFieldBackground AndAlso
           fieldText = ThemeSetForm.darkModeColorFieldText AndAlso
           ProgramSet.colorCodeBackground = ThemeSetForm.darkModeColorCodeBackground AndAlso
           ProgramSet.colorPanelBackground = ThemeSetForm.darkModeColorPanelBackground AndAlso
           changed = ThemeSetForm.darkModeColorChangedBackground AndAlso
           checked = ThemeSetForm.darkModeColorCheckedBackground Then Return 4
        Return 0
    End Function

    'Re-themes this dialog and shows the current colours in the swatches.
    Private Sub ShowThemeColors()
        ThemeSetForm.SetControlColor(Me)
        FieldTextColor.BackColor = ProgramSet.colorFieldText
        FieldBackColor.BackColor = ProgramSet.colorFieldBackground
        ChangedBackColor.BackColor = ProgramSet.colorChangedBackground
        CheckedBackColor.BackColor = ProgramSet.colorCheckedBackground
        BackgroundColor.BackColor = ProgramSet.colorBackground
        LabelTextColor.BackColor = ProgramSet.colorLabelText
        CodeBackColor.BackColor = ProgramSet.colorCodeBackground
        PanelBackColor.BackColor = ProgramSet.colorPanelBackground

        fillingTheme = True
        If ThemePresetCombo.Items.Count > 4 Then ThemePresetCombo.SelectedIndex = CurrentThemePreset()
        fillingTheme = False
    End Sub

    Private Sub ThemePresetCombo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThemePresetCombo.SelectedIndexChanged
        If fillingTheme Then Return
        ApplyThemePreset(ThemePresetCombo.SelectedIndex)
        ShowThemeColors()
    End Sub

    Private Sub PickThemeColor(sender As Object, e As EventArgs) Handles FieldTextButton.Click, FieldBackButton.Click,
        ChangedBackButton.Click, CheckedBackButton.Click, BackgroundButton.Click, LabelTextButton.Click,
        CodeBackButton.Click, PanelBackButton.Click

        If ThemeColorDialog.ShowDialog() <> DialogResult.OK Then Return
        Dim c As Color = ThemeColorDialog.Color
        If sender Is FieldTextButton Then
            ProgramSet.colorFieldText = c
        ElseIf sender Is FieldBackButton Then
            ProgramSet.colorFieldBackground = c
        ElseIf sender Is ChangedBackButton Then
            ProgramSet.colorChangedBackground = c
        ElseIf sender Is CheckedBackButton Then
            ProgramSet.colorCheckedBackground = c
        ElseIf sender Is BackgroundButton Then
            ProgramSet.colorBackground = c
        ElseIf sender Is LabelTextButton Then
            ProgramSet.colorLabelText = c
        ElseIf sender Is CodeBackButton Then
            ProgramSet.colorCodeBackground = c
        ElseIf sender Is PanelBackButton Then
            ProgramSet.colorPanelBackground = c
        End If
        ShowThemeColors()
    End Sub
#End Region
End Class
