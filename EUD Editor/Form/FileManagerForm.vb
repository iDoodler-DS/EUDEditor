Public Class FileManagerForm
    Dim TAB_INDEX As Byte

    Dim grpwire As New GRP
    Dim tranwire As New GRP
    Dim wirefram As New GRP

    Private Sub ColorReset()
        ThemeSetForm.SetControlColor(Me)
    End Sub

    Private loadcmp As Boolean = False
    Private Sub FileManagerForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        loadcmp = False
        Lan.SetLanguage(Me)
        Lan.SetMenu(Me, MenuStrip1)
        Lan.SetMenu(Me, ListMenu)


        'Data덤퍼에서 지정된게 있다면 4, 없다면 옵션에서 참조
        If dataDumper_stat_txt_f = 0 Then
            ComboBox3.SelectedIndex = statlang
        Else
            ComboBox3.SelectedIndex = 3
        End If



        DataGridView1.Columns(1).HeaderText = Lan.GetText(Me.Name, "OldText")
        DataGridView1.Columns(2).HeaderText = Lan.GetText(Me.Name, "NewText")
        DataGridView1.Columns(3).HeaderText = Lan.GetText(Me.Name, "Edit")


        Dim mpq As New SFMpq

        'The icons and strings are decoded at project load; only redo it if something changed.
        If Not FileImportableUpToDate() Then LoadFileimportable()


        'PasteRefresh()
        'ListView1.LargeImageList = DatEditForm.ICONILIST

        'For i = 0 To DatEditForm.ICONILIST.Images.Count - 1
        '    ListView1.Items.Add("")
        '    ListView1.Items(i).ImageIndex = i
        'Next

        'My.Computer.Clipboard.SetText(ImageToGRP("C:\Users\skslj\Desktop\LightBlock.bmp"))


        grpwire.Reset()
        tranwire.Reset()
        wirefram.Reset()

        grpwire.LoadPalette(14)
        tranwire.LoadPalette(14)
        wirefram.LoadPalette(14)
        If dataDumper_grpwire_f <> 0 Then
            grpwire.LoadGRP(dataDumper_grpwire)
        Else
            grpwire.LoadGRP(mpq.ReaddatFile("unit\wirefram\grpwire.grp"))
        End If

        If dataDumper_wirefram_f <> 0 Then
            wirefram.LoadGRP(dataDumper_wirefram)
        Else
            wirefram.LoadGRP(mpq.ReaddatFile("unit\wirefram\wirefram.grp"))
        End If

        If dataDumper_tranwire_f <> 0 Then
            tranwire.LoadGRP(dataDumper_tranwire)
        Else
            tranwire.LoadGRP(mpq.ReaddatFile("unit\wirefram\tranwire.grp"))
        End If

        LoadList()
        PaletDraw()
        ColorReset()
        loadcmp = True
    End Sub

#Region "Wireframe shown inside DatEdit"
    ' The wireframe of a unit belongs to the unit, and DatEdit lists the units, so the
    ' wireframe sits in a sub tab of the unit page there. The controls stay owned by
    ' this form, which holds the code behind them; only their parent changed.
    '
    ' What is left in this window is the string table, which is not per unit.

    Private wireframeMoved As Boolean

    Public ReadOnly Property WireframeReleased As Boolean
        Get
            Return wireframeMoved
        End Get
    End Property

    ''' <summary>
    ''' The wireframe fields, for DatEdit to place. The page also held a list of the
    ''' units, which DatEdit already shows, so only the fields move.
    ''' </summary>
    Public ReadOnly Property WireframeBox As Control
        Get
            Return Panel2
        End Get
    End Property

    ''' <summary>
    ''' Hands the wireframe fields to DatEdit and keeps the string table here.
    ''' Called once, by DatEdit, before it places them.
    ''' </summary>
    Public Sub ReleaseWireframe()
        If wireframeMoved Then Return
        wireframeMoved = True

        'The strip is not on the form: the string table is, and the wireframe fields
        'go to DatEdit. TAB_INDEX names the string table for the rest of this form.
        TAB_INDEX = 0
    End Sub

    ''' <summary>
    ''' Draws the wireframe of one unit. DatEdit calls this when its list changes.
    ''' </summary>
    Public Sub ShowWireframeFor(entryIndex As Integer)
        If Not wireframeMoved Then Return

        _OBJECTNUM = entryIndex
        Try
            LoadWireframeData()
        Catch ex As Exception
            LogException(ex, "drawing the wireframe of unit " & entryIndex)
        End Try
    End Sub
#End Region

#Region "Undo support"
    ' Wireframe values are written here only, so one place records the change.

    Public Sub SetWireframe(kind As Integer, entryIndex As Integer, value As Byte)
        Dim before As Byte = ReadWireframe(kind, entryIndex)
        If before = value Then Return

        Select Case kind
            Case 0 : wireframData(entryIndex) = value
            Case 1 : grpwireData(entryIndex) = value
            Case 2 : tranwireData(entryIndex) = value
        End Select
        ProjectSet.saveStatusChange()

        If Not EditHistory.History.Suppressed Then
            EditHistory.History.Record(New EditHistory.WireframeEdit(kind, entryIndex, before, value))
        End If
    End Sub

    Private Shared Function ReadWireframe(kind As Integer, entryIndex As Integer) As Byte
        Select Case kind
            Case 0 : Return wireframData(entryIndex)
            Case 1 : Return grpwireData(entryIndex)
            Case Else : Return tranwireData(entryIndex)
        End Select
    End Function

    ''' <summary>
    ''' Marks the field an undo is about to change. DatEdit opens the unit; this form
    ''' owns the controls, so it points at the one that changed.
    ''' </summary>
    Public Sub RevealWireframe(kind As Integer, entryIndex As Integer)
        _OBJECTNUM = entryIndex
        LoadWireframeData()
        Dim target As Control = Nothing
        Select Case kind
            Case 0 : target = NumericUpDown1
            Case 1 : target = NumericUpDown2
            Case 2 : target = NumericUpDown3
        End Select
        If target IsNot Nothing AndAlso target.CanSelect Then target.Focus()
    End Sub

    ''' <summary>Opens the string tab and selects the row an undo is about to change.</summary>
    Public Sub RevealStatText(index As Integer)
        If TabControl1.SelectedIndex <> 0 Then TabControl1.SelectedIndex = 0
        For Each row As DataGridViewRow In DataGridView1.Rows
            If row.Tag IsNot Nothing AndAlso CInt(row.Tag) = index Then
                DataGridView1.CurrentCell = row.Cells(1)
                Return
            End If
        Next
    End Sub

    Private Sub SelectEntry(entryIndex As Integer)
        For i = 0 To ListBox1.Items.Count - 1
            If ListBox1.Items(i)(LITEM.index) = entryIndex Then
                ListBox1.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    ''' <summary>Reads the changed values back into the controls after an undo.</summary>
    Public Sub ReloadAfterUndo()
        LoadListData()
        LoadList()

        'An undo reveals the place before it changes the value, so the wireframe fields
        'are drawn again here, after the change.
        Try
            LoadWireframeData()
        Catch ex As Exception
            LogSuppressed(ex, "FileManagerForm.ReloadAfterUndo")
        End Try
    End Sub
#End Region

    Private Sub ListBox1_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles ListBox1.MouseUp
        If e.Button = MouseButtons.Right Then

            Dim n As Integer = ListBox1.IndexFromPoint(e.X, e.Y)
            If n <> ListBox.NoMatches Then
                ListBox1.SelectedIndex = n
            End If

            ListMenuShow()
        End If
    End Sub

    Private Sub ListMenuShow()
        Dim cliptext As String = My.Computer.Clipboard.GetText()


        Try
            붙여넣기ToolStripMenuItem.Enabled = False
            Select Case TAB_INDEX
                Case 1
                    Dim values() As String = cliptext.Split(",")

                    If values.Count = 3 Then
                        For i = 0 To values.Count - 2
                            Try
                                Dim temp As Integer = CInt(values(i))
                                붙여넣기ToolStripMenuItem.Enabled = True
                            Catch ex As Exception
                                붙여넣기ToolStripMenuItem.Enabled = False
                                Exit For
                            End Try
                        Next
                    Else
                        붙여넣기ToolStripMenuItem.Enabled = False
                    End If

            End Select
        Catch ex As Exception
            붙여넣기ToolStripMenuItem.Enabled = False
        End Try

        ListMenu.Show()
        ListMenu.Location = MousePosition
    End Sub


    Private Sub ListBox1_DrawItem(ByVal sender As Object,
ByVal e As System.Windows.Forms.DrawItemEventArgs) Handles ListBox1.DrawItem

        If (e.Index < 0) Then Exit Sub

        Dim myBrush = New SolidBrush(ProgramSet.colorFieldText)

        If ListBox1.Items(e.Index)(LITEM.ischange) = True Then
            myBrush = Brushes.IndianRed
        End If



        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            e = New DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State Xor DrawItemState.Selected, e.ForeColor,
        Color.DarkRed)
        End If


        e.DrawBackground()


        e.Graphics.DrawString(ListBox1.Items(e.Index)(LITEM.Name),
        e.Font, myBrush, e.Bounds, StringFormat.GenericDefault)


        e.DrawFocusRectangle()

    End Sub

    Private Sub PaletDraw()
        ListView1.BeginUpdate()
        ListView1.Items.Clear()
        Dim flingyNum, SpriteNum, ImageNum As Integer
        Dim size As Integer = ListBox1.Items.Count - 1
        For i = 0 To size
            Dim index As Integer = ListBox1.Items(i)(LITEM.index)

            ListView1.Items.Add("")
            Dim itemindex As Integer = ListView1.Items.Count - 1
            flingyNum = DatEditDATA(DTYPE.units).ReadValue("Graphics", index)
            SpriteNum = DatEditDATA(DTYPE.flingy).ReadValue("Sprite", flingyNum)
            ImageNum = DatEditDATA(DTYPE.sprites).ReadValue("Image File", SpriteNum)

            ListView1.LargeImageList = DatEditForm.IMAGELIST
            ListView1.Items(itemindex).ImageIndex = ImageNum
            ListView1.Items(itemindex).Tag = index
        Next
        ListView1.EndUpdate()


        'ListView1.Clear()
        'ListView1.Items.Add(New ListView.ListViewItemCollection())
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        LoadList()
        PaletDraw()
    End Sub

    Private LastSize As Integer
    Private Sub PaletteBtn(sender As Object, e As EventArgs) Handles Button5.Click
        If SplitContainer1.SplitterDistance = 24 Then
            SplitContainer1.Panel1MinSize = 93
            SplitContainer1.IsSplitterFixed = False
            SplitContainer1.SplitterDistance = LastSize '244
            Button5.Text = Lan.GetText(Me.Name, "Fold")
        Else
            LastSize = SplitContainer1.SplitterDistance
            SplitContainer1.Panel1MinSize = 24
            SplitContainer1.IsSplitterFixed = True
            SplitContainer1.SplitterDistance = 24
            Button5.Text = Lan.GetText(Me.Name, "UnFold")
        End If
    End Sub
    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.KeyUp
        LISTFILTER = TextBox2.Text

        LoadList()
        PaletDraw()
    End Sub


    Private Sub LoadListData()
        Dim editText As String = Lan.GetText(Me.Name, "Edit")
        Dim oldFilter As String = TextBox1.Text
        Dim newFilter As String = TextBox3.Text
        Dim rows As New List(Of DataGridViewRow)
        For i = 0 To stat_txt.Length - 1
            If oldFilter <> "" AndAlso Not stat_txt(i).Contains(oldFilter) Then Continue For
            Dim newText As String = Nothing
            If stattextdic.ContainsKey(i) Then newText = stattextdic(i)
            If newFilter <> "" AndAlso (newText Is Nothing OrElse Not newText.Contains(newFilter)) Then Continue For
            Dim row As New DataGridViewRow With {.Tag = i}
            row.CreateCells(DataGridView1, i + 1, stat_txt(i), If(newText, ""), editText)
            rows.Add(row)
        Next
        'One AddRange lays the grid out once; Rows.Add did that for every row.
        DataGridView1.SuspendLayout()
        DataGridView1.Rows.Clear()
        DataGridView1.Rows.AddRange(rows.ToArray())
        DataGridView1.ResumeLayout()
    End Sub


    Dim LoadStatus As Boolean
    ''' <summary>Draws the wireframe fields for the entry on show.</summary>
    Public Sub LoadWireframeData()
        LoadStatus = True
        Try
            NumericUpDown1.Maximum = wirefram.framecount - 1
            NumericUpDown2.Maximum = grpwire.framecount - 1
            NumericUpDown3.Maximum = tranwire.framecount - 1

            '12 > 11
            If wirefram.framecount > _OBJECTNUM Then
                wirefram.DrawToPictureBox(PictureBox1, wireframData(_OBJECTNUM))
                NumericUpDown1.Value = wireframData(_OBJECTNUM)

                If wireframData(_OBJECTNUM) = _OBJECTNUM Then
                    NumericUpDown1.BackColor = ProgramSet.colorFieldBackground
                Else
                    NumericUpDown1.BackColor = ProgramSet.colorChangedBackground
                End If
                NumericUpDown1.Visible = True
            Else
                wirefram.DrawToPictureBox(PictureBox1, 0)
                NumericUpDown1.Visible = False
            End If
            If grpwire.framecount > _OBJECTNUM Then
                grpwire.DrawToPictureBox(PictureBox2, grpwireData(_OBJECTNUM))
                NumericUpDown2.Value = grpwireData(_OBJECTNUM)

                If grpwireData(_OBJECTNUM) = _OBJECTNUM Then
                    NumericUpDown2.BackColor = ProgramSet.colorFieldBackground
                Else
                    NumericUpDown2.BackColor = ProgramSet.colorChangedBackground
                End If
                NumericUpDown2.Visible = True
            Else
                grpwire.DrawToPictureBox(PictureBox2, 0)
                NumericUpDown2.Visible = False
            End If

            If tranwire.framecount > _OBJECTNUM Then
                tranwire.DrawToPictureBox(PictureBox3, tranwireData(_OBJECTNUM))
                NumericUpDown3.Value = tranwireData(_OBJECTNUM)

                If tranwireData(_OBJECTNUM) = _OBJECTNUM Then
                    NumericUpDown3.BackColor = ProgramSet.colorFieldBackground
                Else
                    NumericUpDown3.BackColor = ProgramSet.colorChangedBackground
                End If
                NumericUpDown3.Visible = True
            Else
                tranwire.DrawToPictureBox(PictureBox3, 0)
                NumericUpDown3.Visible = False
            End If
        Finally
            LoadStatus = False
        End Try
    End Sub

    Public Sub LoadData()
        LoadStatus = True
        Select Case TAB_INDEX
            Case 0
                LoadListData()

            Case 1
                LoadWireframeData()
        End Select
        LoadStatus = False
    End Sub

    Private Sub FileManagerForm_Close(sender As Object, e As EventArgs) Handles MyBase.Closing
        DataGridView1.EndEdit()
    End Sub


    Private Sub SELECTLIST(index As Integer)
        ListBox1.SelectedIndex = -1

        For i = 0 To ListBox1.Items.Count - 1
            If ListBox1.Items(i)(LITEM.index) = index Then
                ListBox1.SelectedIndex = i
                _OBJECTNUM = index
                Exit Sub
            End If
        Next


        If ListBox1.SelectedIndex = -1 Then
            If ListBox1.Items.Count <> 0 Then
                ListBox1.SelectedIndex = 0
                _OBJECTNUM = ListBox1.Items(0)(LITEM.index)
                Exit Sub
            End If
        End If

        _OBJECTNUM = 0
    End Sub

    'True while LoadList picks the selection; it loads the data itself afterwards.
    Private selectingList As Boolean

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If selectingList Then Return
        If ListBox1.SelectedIndex <> -1 Then
            _OBJECTNUM = ListBox1.SelectedItem(LITEM.index)

            LoadData()
        End If
    End Sub
    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.Click, ListView1.ItemSelectionChanged
        Try
            SELECTLIST(ListView1.SelectedItems(0).Tag)
        Catch ex As Exception
            LogSuppressed(ex, "FileManagerForm.ListView1_SelectedIndexChanged")
        End Try
    End Sub

    Enum LITEM
        ischange = 0
        index = 1
        Name = 2
    End Enum

    Dim LISTFILTER As String
    Dim _OBJECTNUM As Integer
    Private Sub LoadList()
        Dim lastSELECT As Integer = _OBJECTNUM


        ListBox1.BeginUpdate()

        ListBox1.Items.Clear()



        For i = 0 To CODE(DTYPE.units).Count - 1
            Dim list(2) As String

            Dim temp As String = CODE(DTYPE.units)(i)
            If DatEditDATA(DTYPE.units).ReadValue("Unit Map String", i) = 0 Then
                list(LITEM.Name) = temp
            Else
                Try
                    list(LITEM.Name) = ProjectSet.CHKSTRING(-1 + DatEditDATA(DTYPE.units).ReadValue("Unit Map String", i)) & " (" & temp & ")" 'ProjectSet.UNITSTR(index)
                Catch ex As Exception
                    list(LITEM.Name) = temp
                End Try

            End If
            list(LITEM.index) = i
            list(LITEM.ischange) = False
            list(LITEM.Name) = "[" & Format(i, "000") & "]- " & list(LITEM.Name)




            If wireframData(i) <> i Or grpwireData(i) <> i Or tranwireData(i) <> i Then
                list(LITEM.ischange) = True
            End If




            Dim stra, strb As String
            stra = list(LITEM.Name).ToLower
            If LISTFILTER <> "" Then
                strb = LISTFILTER.ToLower
            Else
                strb = ""
            End If


            If CheckBox5.Checked = True Then
                If list(LITEM.ischange) = True And InStr(stra, strb) <> 0 Then
                    ListBox1.Items.Add(list)
                End If
            Else
                If InStr(stra, strb) <> 0 Then
                    ListBox1.Items.Add(list)
                End If
            End If
        Next


        selectingList = True
        SELECTLIST(lastSELECT)
        selectingList = False

        ListBox1.EndUpdate()
        LoadData()
    End Sub


    Private Sub DataGridView1_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit
        StatTextAdd(DataGridView1.Rows(e.RowIndex).Tag, DataGridView1.Item(e.ColumnIndex, e.RowIndex).Value)
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        If e.ColumnIndex = 3 Then
            Dim dialog As DialogResult
            StatTextForm.stringNum = DataGridView1.Rows(e.RowIndex).Tag
            dialog = StatTextForm.ShowDialog()
            If dialog = DialogResult.OK Then

                StatTextAdd(DataGridView1.Rows(e.RowIndex).Tag, StatTextForm.RawText)
                DataGridView1.Item(2, e.RowIndex).Value = StatTextForm.RawText

                'ComboBox32.Items(TextBox58.Text) = StatTextForm.RawText
            End If
        End If
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        TAB_INDEX = TabControl1.SelectedIndex
        LoadData()
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        If LoadStatus = False Then
            SetWireframe(0, _OBJECTNUM, CByte(NumericUpDown1.Value))
            LoadWireframeData()
            If ListBox1.SelectedIndex <> -1 Then
                If wireframData(_OBJECTNUM) <> _OBJECTNUM Then
                    ListBox1.SelectedItem(LITEM.ischange) = True
                Else
                    ListBox1.SelectedItem(LITEM.ischange) = False
                End If
            End If
        End If
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        If LoadStatus = False Then
            SetWireframe(1, _OBJECTNUM, CByte(NumericUpDown2.Value))
            LoadWireframeData()
            If ListBox1.SelectedIndex <> -1 Then
                If grpwireData(_OBJECTNUM) <> _OBJECTNUM Then
                    ListBox1.SelectedItem(LITEM.ischange) = True
                Else
                    ListBox1.SelectedItem(LITEM.ischange) = False
                End If
            End If
        End If
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown3.ValueChanged
        If LoadStatus = False Then
            SetWireframe(2, _OBJECTNUM, CByte(NumericUpDown3.Value))
            LoadWireframeData()
            If ListBox1.SelectedIndex <> -1 Then
                If tranwireData(_OBJECTNUM) <> _OBJECTNUM Then
                    ListBox1.SelectedItem(LITEM.ischange) = True
                Else
                    ListBox1.SelectedItem(LITEM.ischange) = False
                End If
            End If
        End If
    End Sub

    Private Sub 초기화ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 초기화ToolStripMenuItem.Click
        EditHistory.History.BeginGroup("Reset wireframe")

        SetWireframe(0, _OBJECTNUM, CByte(_OBJECTNUM))

        SetWireframe(1, _OBJECTNUM, CByte(_OBJECTNUM))

        SetWireframe(2, _OBJECTNUM, CByte(_OBJECTNUM))

        EditHistory.History.EndGroup()
        If ListBox1.SelectedIndex <> -1 Then
            ListBox1.SelectedItem(LITEM.ischange) = False
        End If
        LoadData()
    End Sub

    Private Sub 복사ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 복사ToolStripMenuItem.Click
        Dim str As String = ""
        Select Case TAB_INDEX
            Case 1
                str = wireframData(_OBJECTNUM) & "," & grpwireData(_OBJECTNUM) & "," & tranwireData(_OBJECTNUM)
        End Select

        PasteRefresh()
        Try
            My.Computer.Clipboard.SetText(str)
        Catch ex As Exception
            My.Computer.Clipboard.Clear()
        End Try
    End Sub

    Private Sub 붙여넣기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 붙여넣기ToolStripMenuItem.Click
        Dim cliptext As String = My.Computer.Clipboard.GetText

        Select Case TAB_INDEX
            Case 1
                Dim codes() As String = cliptext.Split(",")

                EditHistory.History.BeginGroup("Paste wireframe")


                SetWireframe(0, _OBJECTNUM, CByte(codes(0)))


                SetWireframe(1, _OBJECTNUM, CByte(codes(1)))


                SetWireframe(2, _OBJECTNUM, CByte(codes(2)))


                EditHistory.History.EndGroup()

                If ListBox1.SelectedIndex <> -1 Then
                    If wireframData(_OBJECTNUM) = _OBJECTNUM And grpwireData(_OBJECTNUM) = _OBJECTNUM And tranwireData(_OBJECTNUM) = _OBJECTNUM Then
                        ListBox1.SelectedItem(LITEM.ischange) = False
                    Else
                        ListBox1.SelectedItem(LITEM.ischange) = True
                    End If
                End If

                LoadData()
        End Select
    End Sub

    Private Sub PasteRefresh()
        Dim cliptext As String = My.Computer.Clipboard.GetText()


        Try
            오브젝트붙여넣기ToolStripMenuItem.Enabled = False
            Select Case TAB_INDEX
                Case 1
                    Dim values() As String = cliptext.Split(",")

                    If values.Count = 3 Then
                        For i = 0 To values.Count - 2
                            Try
                                Dim temp As Integer = CInt(values(i))
                                오브젝트붙여넣기ToolStripMenuItem.Enabled = True
                            Catch ex As Exception
                                오브젝트붙여넣기ToolStripMenuItem.Enabled = False
                                Exit For
                            End Try
                        Next
                    Else
                        오브젝트붙여넣기ToolStripMenuItem.Enabled = False
                    End If

            End Select
        Catch ex As Exception
            오브젝트붙여넣기ToolStripMenuItem.Enabled = False
        End Try
    End Sub
    Private Sub 편집ToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles 편집ToolStripMenuItem.DropDownOpening
        PasteRefresh()
    End Sub
    Private Sub 편집ToolStripMenuItem_DropDownClosed(sender As Object, e As EventArgs) Handles 편집ToolStripMenuItem.DropDownClosed
        오브젝트붙여넣기ToolStripMenuItem.Enabled = True
    End Sub

    Private Sub 오브젝트초기화ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 오브젝트초기화ToolStripMenuItem.Click
        EditHistory.History.BeginGroup("Reset wireframe")

        SetWireframe(0, _OBJECTNUM, CByte(_OBJECTNUM))

        SetWireframe(1, _OBJECTNUM, CByte(_OBJECTNUM))

        SetWireframe(2, _OBJECTNUM, CByte(_OBJECTNUM))

        EditHistory.History.EndGroup()
        If ListBox1.SelectedIndex <> -1 Then
            ListBox1.SelectedItem(LITEM.ischange) = False
        End If
        LoadData()
    End Sub

    Private Sub 오브젝트복사ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 오브젝트복사ToolStripMenuItem.Click
        Dim str As String = ""
        Select Case TAB_INDEX
            Case 1
                str = wireframData(_OBJECTNUM) & "," & grpwireData(_OBJECTNUM) & "," & tranwireData(_OBJECTNUM)
        End Select

        Try
            My.Computer.Clipboard.SetText(str)
        Catch ex As Exception
            My.Computer.Clipboard.Clear()
        End Try
    End Sub

    Private Sub 오브젝트붙여넣기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 오브젝트붙여넣기ToolStripMenuItem.Click
        Dim cliptext As String = My.Computer.Clipboard.GetText()


        Try
            Select Case TAB_INDEX
                Case 1
                    Dim values() As String = cliptext.Split(",")

                    If values.Count = 3 Then
                        For i = 0 To values.Count - 2
                            Try

                            Catch ex As Exception
                                Exit Sub
                            End Try
                        Next
                    Else
                        Exit Sub
                    End If

            End Select
        Catch ex As Exception
            Exit Sub
        End Try


        Select Case TAB_INDEX
            Case 1
                Dim codes() As String = cliptext.Split(",")

                EditHistory.History.BeginGroup("Paste wireframe")


                SetWireframe(0, _OBJECTNUM, CByte(codes(0)))


                SetWireframe(1, _OBJECTNUM, CByte(codes(1)))


                SetWireframe(2, _OBJECTNUM, CByte(codes(2)))


                EditHistory.History.EndGroup()

                If ListBox1.SelectedIndex <> -1 Then
                    If wireframData(_OBJECTNUM) = _OBJECTNUM And grpwireData(_OBJECTNUM) = _OBJECTNUM And tranwireData(_OBJECTNUM) = _OBJECTNUM Then
                        ListBox1.SelectedItem(LITEM.ischange) = False
                    Else
                        ListBox1.SelectedItem(LITEM.ischange) = True
                    End If
                End If

                LoadData()
        End Select
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        LoadListData()
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        If loadcmp Then
            statlang = ComboBox3.SelectedIndex
            If statlang <> 3 Then
                dataDumper_stat_txt_f = 0
                dataDumper_stat_txt = ""
            Else
                ComboBox3.SelectedIndex = 0
                Exit Sub
            End If
            stat_txt = Readstat_txtfile(True)
            LoadListData()
        End If
    End Sub
End Class