Imports FastColoredTextBoxNS

Public Class ThemeSetForm
    Dim loadstatus As Boolean = False

    'Dark Mode preset
    Public darkModeColorBackground As Color = Color.FromArgb(&HFF1F1F1F)
    Public darkModeColorLabelText As Color = Color.FromArgb(&HFFE5E5E5)
    Public darkModeColorFieldBackground As Color = Color.FromArgb(&HFF3D3D3D)
    Public darkModeColorFieldText As Color = Color.White
    Public darkModeColorCodeBackground As Color = Color.FromArgb(&HFF1E1E1E)
    Public darkModeColorPanelBackground As Color = Color.FromArgb(&HFF3D3D3D)
    Public darkModeColorChangedBackground As Color = Color.DarkSlateBlue
    Public darkModeColorCheckedBackground As Color = Color.FromArgb(&HFF538585)

    'Form colours shared by the light presets (DatEdit / EUD Editor / EUD Editor2)
    Public lightModeColorBackground As Color = SystemColors.Control
    Public lightModeColorLabelText As Color = SystemColors.ControlText
    Public lightModeColorCodeBackground As Color = Color.White
    Public lightModeColorPanelBackground As Color = Color.FromArgb(&HFFE1E1E1)

    Public Sub ApplyDarkMode()
        ProgramSet.colorBackground = darkModeColorBackground
        ProgramSet.colorLabelText = darkModeColorLabelText
        ProgramSet.colorFieldBackground = darkModeColorFieldBackground
        ProgramSet.colorFieldText = darkModeColorFieldText
        ProgramSet.colorCodeBackground = darkModeColorCodeBackground
        ProgramSet.colorPanelBackground = darkModeColorPanelBackground
        ProgramSet.colorChangedBackground = darkModeColorChangedBackground
        ProgramSet.colorCheckedBackground = darkModeColorCheckedBackground
    End Sub

    Private Sub ApplyLightMode(fieldText As Color, fieldBackground As Color, changedBackground As Color, checkedBackground As Color)
        ProgramSet.colorBackground = lightModeColorBackground
        ProgramSet.colorLabelText = lightModeColorLabelText
        ProgramSet.colorFieldBackground = fieldBackground
        ProgramSet.colorFieldText = fieldText
        ProgramSet.colorCodeBackground = lightModeColorCodeBackground
        ProgramSet.colorPanelBackground = lightModeColorPanelBackground
        ProgramSet.colorChangedBackground = changedBackground
        ProgramSet.colorCheckedBackground = checkedBackground
    End Sub

    Public ReadOnly Property IsDarkTheme As Boolean
        Get
            Return ProgramSet.colorBackground.GetBrightness() < 0.5
        End Get
    End Property

    Private Sub ColorSet()
        SetControlColor(Me)

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
                Case 1 'DatEdit
                    ApplyLightMode(Color.White, Color.Black, Color.DarkCyan, Color.DarkGray)
                Case 2 'EUD Editor
                    ApplyLightMode(Color.Black, Color.White, Color.PaleGreen, Color.LightGray)
                Case 3 'EUD Editor2
                    ApplyLightMode(Color.White, Color.FromArgb(&HFF193333), Color.DarkSlateBlue, Color.FromArgb(&HFF538585))
                Case 4 'Dark Mode
                    ApplyDarkMode()
            End Select



            ColorSet()
        End If
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    ''' <summary>
    ''' Recursively applies the current theme colours to a control tree,
    ''' including menu strips, context menus and their drop-down items.
    ''' Safe to call repeatedly on the same control.
    ''' </summary>
    Sub SetControlColor(c As Control)
        Dim bg As Color = ProgramSet.colorBackground
        Dim fg As Color = ProgramSet.colorLabelText
        Dim fieldBg As Color = ProgramSet.colorFieldBackground
        Dim fieldFg As Color = ProgramSet.colorFieldText
        Dim panelBg As Color = ProgramSet.colorPanelBackground
        Dim dark As Boolean = IsDarkTheme

        If TypeOf c Is Button Then
            Dim b As Button = c
            b.ForeColor = fg
            If dark Then
                b.BackColor = panelBg
                b.FlatStyle = FlatStyle.Flat
                b.FlatAppearance.BorderSize = 1
                b.FlatAppearance.BorderColor = panelBg
                b.FlatAppearance.MouseOverBackColor = ProgramSet.colorCheckedBackground
                b.FlatAppearance.MouseDownBackColor = ProgramSet.colorChangedBackground
                b.UseVisualStyleBackColor = False
            Else
                b.BackColor = bg
                b.FlatStyle = FlatStyle.Standard
                b.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf c Is FastColoredTextBox Then
            Dim t As FastColoredTextBox = c
            t.BackColor = ProgramSet.colorCodeBackground
            t.ForeColor = fieldFg
            t.BookmarkColor = fieldFg
            t.TextAreaBorderColor = panelBg
            t.CurrentLineColor = panelBg
            t.ChangedLineColor = panelBg
            t.LineNumberColor = fieldFg
            t.IndentBackColor = ProgramSet.colorCodeBackground
            t.PaddingBackColor = panelBg
            t.DisabledColor = panelBg
            t.CaretColor = fieldFg
            t.ServiceLinesColor = fieldFg
            t.FoldingIndicatorColor = fieldFg
            t.SelectionColor = Color.FromArgb(&H80, ProgramSet.colorCheckedBackground)

        ElseIf TypeOf c Is DataGridView Then
            Dim g As DataGridView = c
            g.EnableHeadersVisualStyles = False
            g.BackgroundColor = fieldBg
            g.GridColor = panelBg
            g.ForeColor = fieldFg
            g.ColumnHeadersDefaultCellStyle.BackColor = panelBg
            g.ColumnHeadersDefaultCellStyle.ForeColor = fg
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = panelBg
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = fg
            g.RowHeadersDefaultCellStyle.BackColor = panelBg
            g.RowHeadersDefaultCellStyle.ForeColor = fg
            g.RowHeadersDefaultCellStyle.SelectionBackColor = panelBg
            g.RowHeadersDefaultCellStyle.SelectionForeColor = fg
            g.DefaultCellStyle.BackColor = fieldBg
            g.DefaultCellStyle.ForeColor = fieldFg
            g.DefaultCellStyle.SelectionBackColor = ProgramSet.colorCheckedBackground
            g.DefaultCellStyle.SelectionForeColor = fieldFg
            g.RowsDefaultCellStyle.BackColor = fieldBg
            g.RowsDefaultCellStyle.ForeColor = fieldFg
            g.AlternatingRowsDefaultCellStyle.BackColor = fieldBg
            g.AlternatingRowsDefaultCellStyle.ForeColor = fieldFg

        ElseIf TypeOf c Is ComboBox Then
            Dim cb As ComboBox = c
            cb.BackColor = fieldBg
            cb.ForeColor = fieldFg
            'DropDownList combo boxes ignore BackColor unless they are drawn flat
            If dark Then
                cb.FlatStyle = FlatStyle.Flat
            Else
                cb.FlatStyle = FlatStyle.Standard
            End If

        ElseIf TypeOf c Is TextBox OrElse TypeOf c Is NumericUpDown OrElse TypeOf c Is DomainUpDown OrElse
               TypeOf c Is ListBox OrElse TypeOf c Is CheckedListBox OrElse TypeOf c Is RichTextBox OrElse
               TypeOf c Is ListView OrElse TypeOf c Is TreeView Then
            c.BackColor = fieldBg
            c.ForeColor = fieldFg
            If TypeOf c Is TreeView Then
                Dim tv As TreeView = c
                tv.LineColor = fg
            End If

        ElseIf TypeOf c Is TabControl Then
            Dim tc As TabControl = c
            c.BackColor = bg
            c.ForeColor = fg
            RemoveHandler tc.DrawItem, AddressOf TabControl_DrawItem
            If dark Then
                tc.DrawMode = TabDrawMode.OwnerDrawFixed
                AddHandler tc.DrawItem, AddressOf TabControl_DrawItem
            Else
                tc.DrawMode = TabDrawMode.Normal
            End If

        ElseIf TypeOf c Is TabPage Then
            Dim tp As TabPage = c
            tp.UseVisualStyleBackColor = Not dark
            tp.BackColor = bg
            tp.ForeColor = fg

        ElseIf TypeOf c Is ScrollBar OrElse TypeOf c Is ProgressBar OrElse TypeOf c Is TrackBar OrElse TypeOf c Is WebBrowser Then
            'Natively drawn; leave alone.

        Else
            'Form, Panel, GroupBox, Label, CheckBox, RadioButton, PictureBox, SplitContainer,
            'ToolStrip/MenuStrip/ContextMenuStrip, UserControl, ...
            c.BackColor = bg
            c.ForeColor = fg
        End If

        If TypeOf c Is ToolStrip Then
            Dim ts As ToolStrip = c
            ts.RenderMode = ToolStripRenderMode.Professional
            ts.Renderer = New ThemeToolStripRenderer(dark)
            SetToolStripItemColor(ts.Items, dark)
        End If

        For Each control As Control In c.Controls
            SetControlColor(control)
        Next

        If c.ContextMenuStrip IsNot Nothing Then
            SetControlColor(c.ContextMenuStrip)
        End If
    End Sub

    Private Sub SetToolStripItemColor(items As ToolStripItemCollection, dark As Boolean)
        For Each item As ToolStripItem In items
            item.BackColor = ProgramSet.colorBackground
            item.ForeColor = ProgramSet.colorLabelText

            If TypeOf item Is ToolStripControlHost Then
                Dim host As ToolStripControlHost = item
                If host.Control IsNot Nothing Then SetControlColor(host.Control)
            End If

            If TypeOf item Is ToolStripDropDownItem Then
                Dim dd As ToolStripDropDownItem = item
                If dd.HasDropDown Then
                    dd.DropDown.BackColor = ProgramSet.colorBackground
                    dd.DropDown.ForeColor = ProgramSet.colorLabelText
                    dd.DropDown.RenderMode = ToolStripRenderMode.Professional
                    dd.DropDown.Renderer = New ThemeToolStripRenderer(dark)
                    SetToolStripItemColor(dd.DropDownItems, dark)
                End If
            End If
        Next
    End Sub

    Private Sub TabControl_DrawItem(sender As Object, e As DrawItemEventArgs)
        Dim tc As TabControl = sender
        Dim page As TabPage = tc.TabPages(e.Index)
        Dim selected As Boolean = (e.Index = tc.SelectedIndex)
        Dim back As Color = If(selected, ProgramSet.colorFieldBackground, ProgramSet.colorBackground)

        Using brush As New SolidBrush(back)
            e.Graphics.FillRectangle(brush, e.Bounds)
        End Using
        TextRenderer.DrawText(e.Graphics, page.Text, tc.Font, e.Bounds, ProgramSet.colorLabelText,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine)
    End Sub

    ''' <summary>
    ''' ProfessionalColorTable built from the current theme colours so menus,
    ''' tool strips and their drop-downs follow the theme.
    ''' </summary>
    Private Class ThemeColorTable
        Inherits ProfessionalColorTable

        Private ReadOnly bg As Color = ProgramSet.colorBackground
        Private ReadOnly panel As Color = ProgramSet.colorPanelBackground
        Private ReadOnly highlight As Color = ProgramSet.colorCheckedBackground
        Private ReadOnly pressed As Color = ProgramSet.colorChangedBackground

        Public Overrides ReadOnly Property MenuStripGradientBegin As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property MenuStripGradientEnd As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripGradientBegin As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripGradientMiddle As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripGradientEnd As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripBorder As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientMiddle As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonSelectedHighlight As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonSelectedBorder As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonSelectedGradientBegin As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonSelectedGradientMiddle As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonSelectedGradientEnd As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonPressedHighlight As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonPressedBorder As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonPressedGradientBegin As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonPressedGradientMiddle As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonPressedGradientEnd As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonCheckedHighlight As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonCheckedGradientBegin As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonCheckedGradientMiddle As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property ButtonCheckedGradientEnd As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property CheckBackground As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property CheckSelectedBackground As Color
            Get
                Return highlight
            End Get
        End Property
        Public Overrides ReadOnly Property CheckPressedBackground As Color
            Get
                Return pressed
            End Get
        End Property
        Public Overrides ReadOnly Property GripDark As Color
            Get
                Return panel
            End Get
        End Property
        Public Overrides ReadOnly Property GripLight As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property OverflowButtonGradientBegin As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property OverflowButtonGradientMiddle As Color
            Get
                Return bg
            End Get
        End Property
        Public Overrides ReadOnly Property OverflowButtonGradientEnd As Color
            Get
                Return bg
            End Get
        End Property
    End Class

    Private Class ThemeToolStripRenderer
        Inherits ToolStripProfessionalRenderer

        Private ReadOnly dark As Boolean

        Public Sub New(dark As Boolean)
            MyBase.New(If(dark, CType(New ThemeColorTable(), ProfessionalColorTable), New ProfessionalColorTable()))
            Me.dark = dark
            Me.RoundedEdges = Not dark
        End Sub

        Protected Overrides Sub OnRenderArrow(e As ToolStripArrowRenderEventArgs)
            If dark Then e.ArrowColor = ProgramSet.colorLabelText
            MyBase.OnRenderArrow(e)
        End Sub

        Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)
            If dark Then e.TextColor = ProgramSet.colorLabelText
            MyBase.OnRenderItemText(e)
        End Sub

        Protected Overrides Sub OnRenderItemCheck(e As ToolStripItemImageRenderEventArgs)
            If dark Then
                Dim r As Rectangle = e.ImageRectangle
                Using b As New SolidBrush(ProgramSet.colorCheckedBackground)
                    e.Graphics.FillRectangle(b, r)
                End Using
                Using p As New Pen(ProgramSet.colorLabelText, 2)
                    e.Graphics.DrawLines(p, New Point() {
                        New Point(r.Left + 3, r.Top + r.Height \ 2),
                        New Point(r.Left + r.Width \ 2 - 1, r.Bottom - 4),
                        New Point(r.Right - 3, r.Top + 3)})
                End Using
            Else
                MyBase.OnRenderItemCheck(e)
            End If
        End Sub
    End Class
End Class