<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CreateValForm
    Inherits System.Windows.Forms.Form

    'Form은 Dispose를 재정의하여 구성 요소 목록을 정리합니다.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.NumericUpDown1 = New System.Windows.Forms.NumericUpDown()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.VariableTypeRadioButton = New System.Windows.Forms.RadioButton()
        Me.PlayerVariableTypeRadioButton = New System.Windows.Forms.RadioButton()
        Me.ArrayVariableTypeRadioButton = New System.Windows.Forms.RadioButton()
        Me.EasyCompletionComboBox1 = New SergeUtils.EasyCompletionComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(14, 18)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(60, 15)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "변수 명"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(86, 15)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(131, 23)
        Me.TextBox1.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(14, 48)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(60, 15)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "초기값"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'NumericUpDown1
        '
        Me.NumericUpDown1.Location = New System.Drawing.Point(86, 46)
        Me.NumericUpDown1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.NumericUpDown1.Maximum = New Decimal(New Integer() {-1, 0, 0, 0})
        Me.NumericUpDown1.Minimum = New Decimal(New Integer() {-1, 0, 0, -2147483648})
        Me.NumericUpDown1.Name = "NumericUpDown1"
        Me.NumericUpDown1.Size = New System.Drawing.Size(131, 23)
        Me.NumericUpDown1.TabIndex = 3
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel3, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(374, 152)
        Me.TableLayoutPanel1.TabIndex = 4
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.ColumnCount = 2
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel3.Controls.Add(Me.Button5, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.Button6, 1, 0)
        Me.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel3.Location = New System.Drawing.Point(0, 114)
        Me.TableLayoutPanel3.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        Me.TableLayoutPanel3.RowCount = 1
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel3.Size = New System.Drawing.Size(374, 38)
        Me.TableLayoutPanel3.TabIndex = 17
        '
        'Button5
        '
        Me.Button5.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Button5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button5.Enabled = False
        Me.Button5.Image = Global.EUD_Editor.My.Resources.Resources.Okay
        Me.Button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button5.Location = New System.Drawing.Point(0, 0)
        Me.Button5.Margin = New System.Windows.Forms.Padding(0)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(187, 38)
        Me.Button5.TabIndex = 13
        Me.Button5.Text = "확인"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'Button6
        '
        Me.Button6.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button6.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button6.Image = Global.EUD_Editor.My.Resources.Resources.Cancle
        Me.Button6.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button6.Location = New System.Drawing.Point(187, 0)
        Me.Button6.Margin = New System.Windows.Forms.Padding(0)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(187, 38)
        Me.Button6.TabIndex = 14
        Me.Button6.Text = "취소"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.GroupBox1)
        Me.Panel1.Controls.Add(Me.EasyCompletionComboBox1)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.NumericUpDown1)
        Me.Panel1.Controls.Add(Me.TextBox1)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(374, 114)
        Me.Panel1.TabIndex = 0
        '
        'VariableTypeRadioButton
        '
        Me.VariableTypeRadioButton.AutoSize = True
        Me.VariableTypeRadioButton.Location = New System.Drawing.Point(3, 28)
        Me.VariableTypeRadioButton.Name = "VariableTypeRadioButton"
        Me.VariableTypeRadioButton.Size = New System.Drawing.Size(68, 19)
        Me.VariableTypeRadioButton.TabIndex = 8
        Me.VariableTypeRadioButton.TabStop = True
        Me.VariableTypeRadioButton.Text = "Variable"
        Me.VariableTypeRadioButton.UseVisualStyleBackColor = True
        '
        'PlayerVariableTypeRadioButton
        '
        Me.PlayerVariableTypeRadioButton.AutoSize = True
        Me.PlayerVariableTypeRadioButton.Location = New System.Drawing.Point(3, 3)
        Me.PlayerVariableTypeRadioButton.Name = "PlayerVariableTypeRadioButton"
        Me.PlayerVariableTypeRadioButton.Size = New System.Drawing.Size(100, 19)
        Me.PlayerVariableTypeRadioButton.TabIndex = 8
        Me.PlayerVariableTypeRadioButton.TabStop = True
        Me.PlayerVariableTypeRadioButton.Text = "PlayerVariable"
        Me.PlayerVariableTypeRadioButton.UseVisualStyleBackColor = True
        '
        'ArrayVariableTypeRadioButton
        '
        Me.ArrayVariableTypeRadioButton.AutoSize = True
        Me.ArrayVariableTypeRadioButton.Location = New System.Drawing.Point(3, 53)
        Me.ArrayVariableTypeRadioButton.Name = "ArrayVariableTypeRadioButton"
        Me.ArrayVariableTypeRadioButton.Size = New System.Drawing.Size(53, 19)
        Me.ArrayVariableTypeRadioButton.TabIndex = 8
        Me.ArrayVariableTypeRadioButton.TabStop = True
        Me.ArrayVariableTypeRadioButton.Text = "Array"
        Me.ArrayVariableTypeRadioButton.UseVisualStyleBackColor = True
        '
        'EasyCompletionComboBox1
        '
        Me.EasyCompletionComboBox1.FormattingEnabled = True
        Me.EasyCompletionComboBox1.IntegralHeight = False
        Me.EasyCompletionComboBox1.Location = New System.Drawing.Point(86, 46)
        Me.EasyCompletionComboBox1.MaxDropDownItems = 16
        Me.EasyCompletionComboBox1.Name = "EasyCompletionComboBox1"
        Me.EasyCompletionComboBox1.Size = New System.Drawing.Size(130, 23)
        Me.EasyCompletionComboBox1.TabIndex = 6
        Me.EasyCompletionComboBox1.Visible = False
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.FlowLayoutPanel1)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Right
        Me.GroupBox1.Location = New System.Drawing.Point(254, 0)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(120, 114)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Variable Type"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Controls.Add(Me.PlayerVariableTypeRadioButton)
        Me.FlowLayoutPanel1.Controls.Add(Me.VariableTypeRadioButton)
        Me.FlowLayoutPanel1.Controls.Add(Me.ArrayVariableTypeRadioButton)
        Me.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(3, 19)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(111, 92)
        Me.FlowLayoutPanel1.TabIndex = 9
        '
        'CreateValForm
        '
        Me.AcceptButton = Me.Button5
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Button6
        Me.ClientSize = New System.Drawing.Size(380, 158)
        Me.ControlBox = False
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Font = New System.Drawing.Font("Malgun Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CreateValForm"
        Me.Padding = New System.Windows.Forms.Padding(3)
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "CreateVariable"
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel3.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents Button5 As Button
    Friend WithEvents Button6 As Button
    Friend WithEvents Panel1 As Panel
    Friend WithEvents EasyCompletionComboBox1 As SergeUtils.EasyCompletionComboBox
    Friend WithEvents VariableTypeRadioButton As RadioButton
    Friend WithEvents ArrayVariableTypeRadioButton As RadioButton
    Friend WithEvents PlayerVariableTypeRadioButton As RadioButton
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
End Class
