<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SettingForm
    Inherits System.Windows.Forms.Form

    'Form은 Dispose를 재정의하여 구성 요소 목록을 정리합니다.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows Form 디자이너에 필요합니다.
    Private components As System.ComponentModel.IContainer

    '참고: 다음 프로시저는 Windows Form 디자이너에 필요합니다.
    '수정하려면 Windows Form 디자이너를 사용하십시오.  
    '코드 편집기에서는 수정하지 마세요.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.ThemePresetCombo = New System.Windows.Forms.ComboBox()
        Me.ThemePresetLabel = New System.Windows.Forms.Label()
        Me.ThemeColorGroup = New System.Windows.Forms.GroupBox()
        Me.ThemeColorDialog = New System.Windows.Forms.ColorDialog()
        Me.FieldTextLabel = New System.Windows.Forms.Label()
        Me.FieldTextColor = New System.Windows.Forms.PictureBox()
        Me.FieldTextButton = New System.Windows.Forms.Button()
        Me.FieldBackLabel = New System.Windows.Forms.Label()
        Me.FieldBackColor = New System.Windows.Forms.PictureBox()
        Me.FieldBackButton = New System.Windows.Forms.Button()
        Me.ChangedBackLabel = New System.Windows.Forms.Label()
        Me.ChangedBackColor = New System.Windows.Forms.PictureBox()
        Me.ChangedBackButton = New System.Windows.Forms.Button()
        Me.CheckedBackLabel = New System.Windows.Forms.Label()
        Me.CheckedBackColor = New System.Windows.Forms.PictureBox()
        Me.CheckedBackButton = New System.Windows.Forms.Button()
        Me.BackgroundLabel = New System.Windows.Forms.Label()
        Me.BackgroundColor = New System.Windows.Forms.PictureBox()
        Me.BackgroundButton = New System.Windows.Forms.Button()
        Me.LabelTextLabel = New System.Windows.Forms.Label()
        Me.LabelTextColor = New System.Windows.Forms.PictureBox()
        Me.LabelTextButton = New System.Windows.Forms.Button()
        Me.CodeBackLabel = New System.Windows.Forms.Label()
        Me.CodeBackColor = New System.Windows.Forms.PictureBox()
        Me.CodeBackButton = New System.Windows.Forms.Button()
        Me.PanelBackLabel = New System.Windows.Forms.Label()
        Me.PanelBackColor = New System.Windows.Forms.PictureBox()
        Me.PanelBackButton = New System.Windows.Forms.Button()
        Me.ThemeGroup = New System.Windows.Forms.GroupBox()
        Me.CheckBox4 = New System.Windows.Forms.CheckBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.ComboBox3 = New System.Windows.Forms.ComboBox()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.euddraftOFD = New System.Windows.Forms.OpenFileDialog()
        Me.StarCraftOFD = New System.Windows.Forms.OpenFileDialog()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        Me.GroupBox2.SuspendLayout()
        Me.ThemeGroup.SuspendLayout()
        Me.ThemeColorGroup.SuspendLayout()
        CType(Me.FieldTextColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.FieldBackColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ChangedBackColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CheckedBackColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BackgroundColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LabelTextColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CodeBackColor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PanelBackColor, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.FlowLayoutPanel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 92)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(103, 15)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "스타크래프트버전"
        '
        'ComboBox1
        '
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Items.AddRange(New Object() {"1.16.1", "Remastered"})
        Me.ComboBox1.Location = New System.Drawing.Point(140, 89)
        Me.ComboBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(264, 23)
        Me.ComboBox1.TabIndex = 2
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 126)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(79, 15)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "스타실행파일"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(346, 122)
        Me.Button1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(58, 25)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "설정"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(140, 123)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ReadOnly = True
        Me.TextBox1.Size = New System.Drawing.Size(200, 23)
        Me.TextBox1.TabIndex = 5
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(140, 157)
        Me.TextBox2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.ReadOnly = True
        Me.TextBox2.Size = New System.Drawing.Size(200, 23)
        Me.TextBox2.TabIndex = 8
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(346, 156)
        Me.Button2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(58, 25)
        Me.Button2.TabIndex = 7
        Me.Button2.Text = "설정"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 160)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(73, 15)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "euddraft.exe"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.CheckBox4)
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.ComboBox3)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.ComboBox1)
        Me.GroupBox2.Controls.Add(Me.TextBox2)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Button2)
        Me.GroupBox2.Controls.Add(Me.Button1)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.TextBox1)
        Me.GroupBox2.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(0)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(420, 196)
        Me.GroupBox2.TabIndex = 10
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "프로그램 세팅"
        '
        'CheckBox4
        '
        Me.CheckBox4.AutoSize = True
        Me.CheckBox4.Location = New System.Drawing.Point(12, 24)
        Me.CheckBox4.Name = "CheckBox4"
        Me.CheckBox4.Size = New System.Drawing.Size(86, 19)
        Me.CheckBox4.TabIndex = 14
        Me.CheckBox4.Text = "자동컴파일"
        Me.CheckBox4.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(12, 58)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(84, 15)
        Me.Label8.TabIndex = 9
        Me.Label8.Text = "언어(Language)"
        '
        'ComboBox3
        '
        Me.ComboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ComboBox3.FormattingEnabled = True
        Me.ComboBox3.Location = New System.Drawing.Point(140, 55)
        Me.ComboBox3.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ComboBox3.Name = "ComboBox3"
        Me.ComboBox3.Size = New System.Drawing.Size(264, 23)
        Me.ComboBox3.TabIndex = 10
        '
        'ThemePresetCombo
        '
        Me.ThemePresetCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ThemePresetCombo.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ThemePresetCombo.FormattingEnabled = True
        Me.ThemePresetCombo.Items.AddRange(New Object() {"사용자 정의", "DatEdit 테마", "EUD Editor 테마", "EUD Editor2 테마"})
        Me.ThemePresetCombo.Location = New System.Drawing.Point(140, 27)
        Me.ThemePresetCombo.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ThemePresetCombo.Name = "ThemePresetCombo"
        Me.ThemePresetCombo.Size = New System.Drawing.Size(264, 23)
        Me.ThemePresetCombo.TabIndex = 0
        '
        'ThemePresetLabel
        '
        Me.ThemePresetLabel.Location = New System.Drawing.Point(12, 30)
        Me.ThemePresetLabel.Name = "ThemePresetLabel"
        Me.ThemePresetLabel.Size = New System.Drawing.Size(125, 13)
        Me.ThemePresetLabel.TabIndex = 3
        Me.ThemePresetLabel.Text = "테마"
        Me.ThemePresetLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'ThemeColorGroup
        '
        Me.ThemeColorGroup.Controls.Add(Me.PanelBackColor)
        Me.ThemeColorGroup.Controls.Add(Me.CheckedBackColor)
        Me.ThemeColorGroup.Controls.Add(Me.CodeBackColor)
        Me.ThemeColorGroup.Controls.Add(Me.FieldTextColor)
        Me.ThemeColorGroup.Controls.Add(Me.ChangedBackColor)
        Me.ThemeColorGroup.Controls.Add(Me.FieldTextLabel)
        Me.ThemeColorGroup.Controls.Add(Me.LabelTextColor)
        Me.ThemeColorGroup.Controls.Add(Me.FieldTextButton)
        Me.ThemeColorGroup.Controls.Add(Me.FieldBackColor)
        Me.ThemeColorGroup.Controls.Add(Me.BackgroundColor)
        Me.ThemeColorGroup.Controls.Add(Me.PanelBackButton)
        Me.ThemeColorGroup.Controls.Add(Me.PanelBackLabel)
        Me.ThemeColorGroup.Controls.Add(Me.CheckedBackButton)
        Me.ThemeColorGroup.Controls.Add(Me.CodeBackButton)
        Me.ThemeColorGroup.Controls.Add(Me.CheckedBackLabel)
        Me.ThemeColorGroup.Controls.Add(Me.CodeBackLabel)
        Me.ThemeColorGroup.Controls.Add(Me.ChangedBackButton)
        Me.ThemeColorGroup.Controls.Add(Me.LabelTextButton)
        Me.ThemeColorGroup.Controls.Add(Me.ChangedBackLabel)
        Me.ThemeColorGroup.Controls.Add(Me.FieldBackButton)
        Me.ThemeColorGroup.Controls.Add(Me.BackgroundButton)
        Me.ThemeColorGroup.Controls.Add(Me.FieldBackLabel)
        Me.ThemeColorGroup.Controls.Add(Me.BackgroundLabel)
        Me.ThemeColorGroup.Controls.Add(Me.LabelTextLabel)
        Me.ThemeColorGroup.Location = New System.Drawing.Point(8, 62)
        Me.ThemeColorGroup.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ThemeColorGroup.Name = "ThemeColorGroup"
        Me.ThemeColorGroup.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ThemeColorGroup.Size = New System.Drawing.Size(404, 152)
        Me.ThemeColorGroup.TabIndex = 5
        Me.ThemeColorGroup.TabStop = False
        '
        'FieldTextLabel
        '
        Me.FieldTextLabel.Location = New System.Drawing.Point(208, 53)
        Me.FieldTextLabel.Name = "FieldTextLabel"
        Me.FieldTextLabel.Size = New System.Drawing.Size(100, 13)
        Me.FieldTextLabel.TabIndex = 4
        Me.FieldTextLabel.Text = "글씨색"
        Me.FieldTextLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'FieldTextColor
        '
        Me.FieldTextColor.Location = New System.Drawing.Point(314, 50)
        Me.FieldTextColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.FieldTextColor.Name = "FieldTextColor"
        Me.FieldTextColor.Size = New System.Drawing.Size(20, 20)
        Me.FieldTextColor.TabIndex = 14
        Me.FieldTextColor.TabStop = False
        '
        'FieldTextButton
        '
        Me.FieldTextButton.Location = New System.Drawing.Point(338, 48)
        Me.FieldTextButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.FieldTextButton.Name = "FieldTextButton"
        Me.FieldTextButton.Size = New System.Drawing.Size(50, 23)
        Me.FieldTextButton.TabIndex = 2
        Me.FieldTextButton.Text = "수정"
        Me.FieldTextButton.UseVisualStyleBackColor = True
        '
        'FieldBackLabel
        '
        Me.FieldBackLabel.Location = New System.Drawing.Point(8, 53)
        Me.FieldBackLabel.Name = "FieldBackLabel"
        Me.FieldBackLabel.Size = New System.Drawing.Size(100, 13)
        Me.FieldBackLabel.TabIndex = 7
        Me.FieldBackLabel.Text = "배경색"
        Me.FieldBackLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'FieldBackColor
        '
        Me.FieldBackColor.Location = New System.Drawing.Point(112, 50)
        Me.FieldBackColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.FieldBackColor.Name = "FieldBackColor"
        Me.FieldBackColor.Size = New System.Drawing.Size(20, 20)
        Me.FieldBackColor.TabIndex = 15
        Me.FieldBackColor.TabStop = False
        '
        'FieldBackButton
        '
        Me.FieldBackButton.Location = New System.Drawing.Point(136, 48)
        Me.FieldBackButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.FieldBackButton.Name = "FieldBackButton"
        Me.FieldBackButton.Size = New System.Drawing.Size(50, 23)
        Me.FieldBackButton.TabIndex = 6
        Me.FieldBackButton.Text = "수정"
        Me.FieldBackButton.UseVisualStyleBackColor = True
        '
        'ChangedBackLabel
        '
        Me.ChangedBackLabel.Location = New System.Drawing.Point(8, 117)
        Me.ChangedBackLabel.Name = "ChangedBackLabel"
        Me.ChangedBackLabel.Size = New System.Drawing.Size(100, 13)
        Me.ChangedBackLabel.TabIndex = 10
        Me.ChangedBackLabel.Text = "수정색"
        Me.ChangedBackLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'ChangedBackColor
        '
        Me.ChangedBackColor.Location = New System.Drawing.Point(112, 114)
        Me.ChangedBackColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ChangedBackColor.Name = "ChangedBackColor"
        Me.ChangedBackColor.Size = New System.Drawing.Size(20, 20)
        Me.ChangedBackColor.TabIndex = 16
        Me.ChangedBackColor.TabStop = False
        '
        'ChangedBackButton
        '
        Me.ChangedBackButton.Location = New System.Drawing.Point(136, 112)
        Me.ChangedBackButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ChangedBackButton.Name = "ChangedBackButton"
        Me.ChangedBackButton.Size = New System.Drawing.Size(50, 23)
        Me.ChangedBackButton.TabIndex = 9
        Me.ChangedBackButton.Text = "수정"
        Me.ChangedBackButton.UseVisualStyleBackColor = True
        '
        'CheckedBackLabel
        '
        Me.CheckedBackLabel.Location = New System.Drawing.Point(208, 117)
        Me.CheckedBackLabel.Name = "CheckedBackLabel"
        Me.CheckedBackLabel.Size = New System.Drawing.Size(100, 13)
        Me.CheckedBackLabel.TabIndex = 13
        Me.CheckedBackLabel.Text = "체크색"
        Me.CheckedBackLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'CheckedBackColor
        '
        Me.CheckedBackColor.Location = New System.Drawing.Point(314, 114)
        Me.CheckedBackColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.CheckedBackColor.Name = "CheckedBackColor"
        Me.CheckedBackColor.Size = New System.Drawing.Size(20, 20)
        Me.CheckedBackColor.TabIndex = 17
        Me.CheckedBackColor.TabStop = False
        '
        'CheckedBackButton
        '
        Me.CheckedBackButton.Location = New System.Drawing.Point(338, 112)
        Me.CheckedBackButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.CheckedBackButton.Name = "CheckedBackButton"
        Me.CheckedBackButton.Size = New System.Drawing.Size(50, 23)
        Me.CheckedBackButton.TabIndex = 12
        Me.CheckedBackButton.Text = "수정"
        Me.CheckedBackButton.UseVisualStyleBackColor = True
        '
        'BackgroundLabel
        '
        Me.BackgroundLabel.Location = New System.Drawing.Point(8, 21)
        Me.BackgroundLabel.Name = "BackgroundLabel"
        Me.BackgroundLabel.Size = New System.Drawing.Size(100, 13)
        Me.BackgroundLabel.TabIndex = 4
        Me.BackgroundLabel.Text = "BG"
        Me.BackgroundLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'BackgroundColor
        '
        Me.BackgroundColor.Location = New System.Drawing.Point(112, 18)
        Me.BackgroundColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.BackgroundColor.Name = "BackgroundColor"
        Me.BackgroundColor.Size = New System.Drawing.Size(20, 20)
        Me.BackgroundColor.TabIndex = 14
        Me.BackgroundColor.TabStop = False
        '
        'BackgroundButton
        '
        Me.BackgroundButton.Location = New System.Drawing.Point(136, 16)
        Me.BackgroundButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.BackgroundButton.Name = "BackgroundButton"
        Me.BackgroundButton.Size = New System.Drawing.Size(50, 23)
        Me.BackgroundButton.TabIndex = 2
        Me.BackgroundButton.Text = "수정"
        Me.BackgroundButton.UseVisualStyleBackColor = True
        '
        'LabelTextLabel
        '
        Me.LabelTextLabel.Location = New System.Drawing.Point(208, 21)
        Me.LabelTextLabel.Name = "LabelTextLabel"
        Me.LabelTextLabel.Size = New System.Drawing.Size(100, 13)
        Me.LabelTextLabel.TabIndex = 7
        Me.LabelTextLabel.Text = "LabelText"
        Me.LabelTextLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'LabelTextColor
        '
        Me.LabelTextColor.Location = New System.Drawing.Point(314, 18)
        Me.LabelTextColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.LabelTextColor.Name = "LabelTextColor"
        Me.LabelTextColor.Size = New System.Drawing.Size(20, 20)
        Me.LabelTextColor.TabIndex = 15
        Me.LabelTextColor.TabStop = False
        '
        'LabelTextButton
        '
        Me.LabelTextButton.Location = New System.Drawing.Point(338, 16)
        Me.LabelTextButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.LabelTextButton.Name = "LabelTextButton"
        Me.LabelTextButton.Size = New System.Drawing.Size(50, 23)
        Me.LabelTextButton.TabIndex = 6
        Me.LabelTextButton.Text = "수정"
        Me.LabelTextButton.UseVisualStyleBackColor = True
        '
        'CodeBackLabel
        '
        Me.CodeBackLabel.Location = New System.Drawing.Point(8, 85)
        Me.CodeBackLabel.Name = "CodeBackLabel"
        Me.CodeBackLabel.Size = New System.Drawing.Size(100, 13)
        Me.CodeBackLabel.TabIndex = 10
        Me.CodeBackLabel.Text = "CodeBG"
        Me.CodeBackLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'CodeBackColor
        '
        Me.CodeBackColor.Location = New System.Drawing.Point(112, 82)
        Me.CodeBackColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.CodeBackColor.Name = "CodeBackColor"
        Me.CodeBackColor.Size = New System.Drawing.Size(20, 20)
        Me.CodeBackColor.TabIndex = 16
        Me.CodeBackColor.TabStop = False
        '
        'CodeBackButton
        '
        Me.CodeBackButton.Location = New System.Drawing.Point(136, 80)
        Me.CodeBackButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.CodeBackButton.Name = "CodeBackButton"
        Me.CodeBackButton.Size = New System.Drawing.Size(50, 23)
        Me.CodeBackButton.TabIndex = 9
        Me.CodeBackButton.Text = "수정"
        Me.CodeBackButton.UseVisualStyleBackColor = True
        '
        'PanelBackLabel
        '
        Me.PanelBackLabel.Location = New System.Drawing.Point(208, 85)
        Me.PanelBackLabel.Name = "PanelBackLabel"
        Me.PanelBackLabel.Size = New System.Drawing.Size(100, 13)
        Me.PanelBackLabel.TabIndex = 13
        Me.PanelBackLabel.Text = "PanelBG"
        Me.PanelBackLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'PanelBackColor
        '
        Me.PanelBackColor.Location = New System.Drawing.Point(314, 82)
        Me.PanelBackColor.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.PanelBackColor.Name = "PanelBackColor"
        Me.PanelBackColor.Size = New System.Drawing.Size(20, 20)
        Me.PanelBackColor.TabIndex = 17
        Me.PanelBackColor.TabStop = False
        '
        'PanelBackButton
        '
        Me.PanelBackButton.Location = New System.Drawing.Point(338, 80)
        Me.PanelBackButton.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.PanelBackButton.Name = "PanelBackButton"
        Me.PanelBackButton.Size = New System.Drawing.Size(50, 23)
        Me.PanelBackButton.TabIndex = 12
        Me.PanelBackButton.Text = "수정"
        Me.PanelBackButton.UseVisualStyleBackColor = True
        '
        'ThemeGroup
        '
        Me.ThemeGroup.Controls.Add(Me.ThemePresetLabel)
        Me.ThemeGroup.Controls.Add(Me.ThemePresetCombo)
        Me.ThemeGroup.Controls.Add(Me.ThemeColorGroup)
        Me.ThemeGroup.Location = New System.Drawing.Point(0, 208)
        Me.ThemeGroup.Margin = New System.Windows.Forms.Padding(0, 12, 0, 0)
        Me.ThemeGroup.Name = "ThemeGroup"
        Me.ThemeGroup.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ThemeGroup.Size = New System.Drawing.Size(420, 226)
        Me.ThemeGroup.TabIndex = 11
        Me.ThemeGroup.TabStop = False
        Me.ThemeGroup.Text = "Theme"
        '
        'Button5
        '
        Me.Button5.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Button5.Enabled = False
        Me.Button5.Image = Global.EUD_Editor.My.Resources.Resources.Okay
        Me.Button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button5.Location = New System.Drawing.Point(0, 0)
        Me.Button5.Margin = New System.Windows.Forms.Padding(0)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(210, 34)
        Me.Button5.TabIndex = 11
        Me.Button5.Text = "확인"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'euddraftOFD
        '
        Me.euddraftOFD.FileName = "eudddraft.exe"
        Me.euddraftOFD.Filter = "eudddraft.exe|euddraft.exe"
        Me.euddraftOFD.Title = "euddraft.exe 선택"
        '
        'StarCraftOFD
        '
        Me.StarCraftOFD.FileName = "StarCraft.exe"
        Me.StarCraftOFD.Filter = "StarCraft.exe|StarCraft.exe"
        Me.StarCraftOFD.Title = "StarCraft.exe 선택"
        '
        'Button6
        '
        Me.Button6.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button6.Image = Global.EUD_Editor.My.Resources.Resources.Cancle
        Me.Button6.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button6.Location = New System.Drawing.Point(210, 0)
        Me.Button6.Margin = New System.Windows.Forms.Padding(0)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(210, 34)
        Me.Button6.TabIndex = 12
        Me.Button6.Text = "취소"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.AutoSize = True
        Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.FlowLayoutPanel1.Controls.Add(Me.GroupBox2)
        Me.FlowLayoutPanel1.Controls.Add(Me.ThemeGroup)
        Me.FlowLayoutPanel1.Controls.Add(Me.FlowLayoutPanel2)
        Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.FlowLayoutPanel1.MaximumSize = New System.Drawing.Size(444, 900)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Padding = New System.Windows.Forms.Padding(12)
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(444, 504)
        Me.FlowLayoutPanel1.TabIndex = 13
        '
        'FlowLayoutPanel2
        '
        Me.FlowLayoutPanel2.Controls.Add(Me.Button5)
        Me.FlowLayoutPanel2.Controls.Add(Me.Button6)
        Me.FlowLayoutPanel2.Location = New System.Drawing.Point(0, 446)
        Me.FlowLayoutPanel2.Margin = New System.Windows.Forms.Padding(0, 12, 0, 0)
        Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        Me.FlowLayoutPanel2.Size = New System.Drawing.Size(420, 34)
        Me.FlowLayoutPanel2.TabIndex = 14
        '
        'SettingForm
        '
        Me.AcceptButton = Me.Button5
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.CancelButton = Me.Button6
        Me.ClientSize = New System.Drawing.Size(444, 504)
        Me.ControlBox = False
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Font = New System.Drawing.Font("Malgun Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "SettingForm"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Setting"
        Me.GroupBox2.ResumeLayout(False)
        Me.ThemeGroup.ResumeLayout(False)
        Me.ThemeGroup.PerformLayout()
        Me.ThemeColorGroup.ResumeLayout(False)
        CType(Me.FieldTextColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.FieldBackColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ChangedBackColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CheckedBackColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BackgroundColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LabelTextColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CodeBackColor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PanelBackColor, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.PerformLayout()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents ThemePresetCombo As ComboBox
    Friend WithEvents ThemePresetLabel As Label
    Friend WithEvents ThemeColorGroup As GroupBox
    Friend WithEvents ThemeColorDialog As ColorDialog
    Friend WithEvents FieldTextLabel As Label
    Friend WithEvents FieldTextColor As PictureBox
    Friend WithEvents FieldTextButton As Button
    Friend WithEvents FieldBackLabel As Label
    Friend WithEvents FieldBackColor As PictureBox
    Friend WithEvents FieldBackButton As Button
    Friend WithEvents ChangedBackLabel As Label
    Friend WithEvents ChangedBackColor As PictureBox
    Friend WithEvents ChangedBackButton As Button
    Friend WithEvents CheckedBackLabel As Label
    Friend WithEvents CheckedBackColor As PictureBox
    Friend WithEvents CheckedBackButton As Button
    Friend WithEvents BackgroundLabel As Label
    Friend WithEvents BackgroundColor As PictureBox
    Friend WithEvents BackgroundButton As Button
    Friend WithEvents LabelTextLabel As Label
    Friend WithEvents LabelTextColor As PictureBox
    Friend WithEvents LabelTextButton As Button
    Friend WithEvents CodeBackLabel As Label
    Friend WithEvents CodeBackColor As PictureBox
    Friend WithEvents CodeBackButton As Button
    Friend WithEvents PanelBackLabel As Label
    Friend WithEvents PanelBackColor As PictureBox
    Friend WithEvents PanelBackButton As Button
    Friend WithEvents ThemeGroup As GroupBox
    Friend WithEvents Button5 As Button
    Friend WithEvents euddraftOFD As OpenFileDialog
    Friend WithEvents StarCraftOFD As OpenFileDialog
    Friend WithEvents Button6 As Button
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents FlowLayoutPanel2 As FlowLayoutPanel
    Friend WithEvents Label8 As Label
    Friend WithEvents ComboBox3 As ComboBox
    Friend WithEvents CheckBox4 As CheckBox
End Class
