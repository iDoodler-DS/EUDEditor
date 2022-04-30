Public Class ThemeSetForm
    Dim loadstatus As Boolean = False
    Public darkModeColorBackground = Color.FromArgb(&HFF1F1F1F)
    Public darkModeColorLabelText = Color.FromArgb(&HFFE5E5E5)
    Public darkModeColorFieldBackground = Color.FromArgb(&HFF3D3D3D)
    Public darkModeColorFieldText = Color.White
    Public darkModeColorCodeBackground = Color.FromArgb(&HFF1E1E1E)
    Public darkModeColorPanelBackground = Color.FromArgb(&HFF3D3D3D)
    Public darkModeColorChangedBackground = Color.DarkSlateBlue
    Public darkModeColorCheckedBackground = Color.FromArgb(&HFF538585)
    Private Sub ColorSet()
        PictureBox1.BackColor = ProgramSet.colorFieldText
        PictureBox2.BackColor = ProgramSet.colorFieldBackground
        PictureBox3.BackColor = ProgramSet.colorChangedBackground
        PictureBox4.BackColor = ProgramSet.colorCheckedBackground
        PictureBox5.BackColor = ProgramSet.colorBackground
        PictureBox6.BackColor = ProgramSet.colorLabelText
        PictureBox7.BackColor = ProgramSet.colorCodeBackground
        PictureBox8.BackColor = ProgramSet.colorPanelBackground


        loadstatus = True
        ComboBox1.SelectedIndex = 0
        If ProgramSet.colorFieldText = Color.White And
        ProgramSet.colorFieldBackground = Color.Black And
        ProgramSet.colorChangedBackground = Color.DarkCyan And
        ProgramSet.colorCheckedBackground = Color.DarkGray Then
            ComboBox1.SelectedIndex = 1
        End If

        If ProgramSet.colorFieldText = Color.Black And
        ProgramSet.colorFieldBackground = Color.White And
        ProgramSet.colorChangedBackground = Color.PaleGreen And
        ProgramSet.colorCheckedBackground = Color.LightGray Then
            ComboBox1.SelectedIndex = 2
        End If

        If ProgramSet.colorFieldText = Color.White And
        ProgramSet.colorFieldBackground = Color.FromArgb(&HFF193333) And
        ProgramSet.colorChangedBackground = Color.DarkSlateBlue And
        ProgramSet.colorCheckedBackground = Color.FromArgb(&HFF538585) Then
            ComboBox1.SelectedIndex = 3
        End If ''FromArgb(&HFF4D9999)

        If ProgramSet.colorBackground = Me.darkModeColorBackground And
            ProgramSet.colorLabelText = Me.darkModeColorLabelText And
            ProgramSet.colorFieldBackground = Me.darkModeColorFieldBackground And
            ProgramSet.colorFieldText = Me.darkModeColorFieldText And
            ProgramSet.colorCodeBackground = Me.darkModeColorCodeBackground And
            ProgramSet.colorPanelBackground = Me.darkModeColorPanelBackground And
            ProgramSet.colorChangedBackground = Me.darkModeColorChangedBackground And
            ProgramSet.colorCheckedBackground = Me.darkModeColorCheckedBackground Then
            ComboBox1.SelectedIndex = 4
        End If
        loadstatus = False
    End Sub
    Private Sub ThemeSetForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Lan.SetLanguage(Me)

        ComboBox1.Items.Clear()
        ComboBox1.Items.AddRange(Lan.GetArray(Me.Name, "Combobox1"))
        ColorSet()
    End Sub
    '    사용자 정의
    'DatEdit 테마
    'EUD Editor 테마
    'EUD Editor2 테마

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorFieldText = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorFieldBackground = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorChangedBackground = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorCheckedBackground = ColorDialog1.Color
        ColorSet()
    End Sub
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorBackground = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorLabelText = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorCodeBackground = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        ColorDialog1.ShowDialog()
        ProgramSet.colorPanelBackground = ColorDialog1.Color
        ColorSet()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If loadstatus = False Then
            Select Case ComboBox1.SelectedIndex
                Case 1
                    ProgramSet.colorFieldText = Color.White

                    ProgramSet.colorFieldBackground = Color.Black
                    ProgramSet.colorChangedBackground = Color.DarkCyan

                    ProgramSet.colorCheckedBackground = Color.DarkGray
                Case 2
                    ProgramSet.colorFieldText = Color.Black

                    ProgramSet.colorFieldBackground = Color.White
                    ProgramSet.colorChangedBackground = Color.PaleGreen

                    ProgramSet.colorCheckedBackground = Color.LightGray
                Case 3
                    ProgramSet.colorFieldText = Color.White

                    ProgramSet.colorFieldBackground = Color.FromArgb(&HFF193333)
                    ProgramSet.colorChangedBackground = Color.DarkSlateBlue

                    ProgramSet.colorCheckedBackground = Color.FromArgb(&HFF538585) ''FromArgb(&HFF4D9999)
                Case 4
                    ProgramSet.colorBackground = Me.darkModeColorBackground
                    ProgramSet.colorLabelText = Me.darkModeColorLabelText
                    ProgramSet.colorFieldBackground = Me.darkModeColorFieldBackground
                    ProgramSet.colorFieldText = Me.darkModeColorFieldText
                    ProgramSet.colorCodeBackground = Me.darkModeColorCodeBackground
                    ProgramSet.colorPanelBackground = Me.darkModeColorPanelBackground
                    ProgramSet.colorChangedBackground = Me.darkModeColorChangedBackground
                    ProgramSet.colorCheckedBackground = Me.darkModeColorCheckedBackground
            End Select



            ColorSet()
        End If
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Sub SetControlColor(c As Control)
        'Public colorBackground As Color
        'Public colorLabelText As Color
        'Public colorFieldBackground As Color
        'Public colorFieldText As Color
        'Public colorChangedBackground As Color
        'Public colorCheckedBackground As Color
        'Public colorCodeBackground As Color
        'Public colorPanelBackground As Color
        c.BackColor = ProgramSet.colorBackground
        c.ForeColor = ProgramSet.colorLabelText

        If (c.GetType() = GetType(Button)) Then
            Dim b As Button = c
            b.FlatStyle = FlatStyle.Flat
            b.FlatAppearance.BorderSize = 1
            b.FlatAppearance.BorderColor = ProgramSet.colorPanelBackground
            b.ForeColor = ProgramSet.colorLabelText
        ElseIf c.GetType() = GetType(TableLayoutPanel) Or c.GetType() = GetType(FlowLayoutPanel) Then
            AddHandler c.Paint, AddressOf Panel_Paint
        End If


        Select Case c.GetType()
            Case GetType(Form), GetType(MenuStrip), GetType(ToolStrip), GetType(ToolStripMenuItem), GetType(TableLayoutPanel), GetType(FlowLayoutPanel), GetType(TabPage)
                c.BackColor = ProgramSet.colorBackground
                c.ForeColor = ProgramSet.colorLabelText
            Case GetType(TextBox), GetType(NumericUpDown), GetType(ComboBox), GetType(CheckBox), GetType(ListBox)
                c.BackColor = ProgramSet.colorFieldBackground
                c.ForeColor = ProgramSet.colorFieldText
        End Select

        For Each control As Control In c.Controls
            SetControlColor(control)
        Next
    End Sub

    Private Sub Panel_Paint(panel As Panel, e As PaintEventArgs)
        ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Color.Red, ButtonBorderStyle.None)
    End Sub
End Class