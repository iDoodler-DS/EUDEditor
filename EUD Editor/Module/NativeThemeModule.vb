Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

''' <summary>
''' Win32 helpers that push the theme into the parts WinForms does not paint itself:
''' non-client scroll bars, window title bars, combo box drop-down lists, list view
''' headers, numeric up/down buttons and tab control chrome.
''' </summary>
Module NativeThemeModule

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    <DllImport("uxtheme.dll", EntryPoint:="#135")>
    Private Function SetPreferredAppMode(mode As Integer) As Integer
    End Function

    <DllImport("uxtheme.dll", EntryPoint:="#136")>
    Private Sub FlushMenuThemes()
    End Sub

    <DllImport("dwmapi.dll")>
    Private Function DwmSetWindowAttribute(hwnd As IntPtr, attr As Integer, ByRef attrValue As Integer, attrSize As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Function GetComboBoxInfo(hWnd As IntPtr, ByRef pcbi As COMBOBOXINFO) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function BeginPaint(hWnd As IntPtr, ByRef ps As PAINTSTRUCT) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function EndPaint(hWnd As IntPtr, ByRef ps As PAINTSTRUCT) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure PAINTSTRUCT
        Public hdc As IntPtr
        Public fErase As Boolean
        Public rcPaint As RECT
        Public fRestore As Boolean
        Public fIncUpdate As Boolean
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> Public rgbReserved As Byte()
    End Structure

    Private Const WM_SETREDRAW As Integer = &HB
    Private Const WM_ERASEBKGND As Integer = &H14

    ''' <summary>
    ''' Stops a window (and its children) from repainting until ResumeDrawing, so a
    ''' burst of changes shows up as one repaint instead of a flickering sequence.
    ''' </summary>
    Public Sub SuspendDrawing(c As Control)
        If c IsNot Nothing AndAlso c.IsHandleCreated Then SendMessage(c.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero)
    End Sub

    Public Sub ResumeDrawing(c As Control)
        If c IsNot Nothing AndAlso c.IsHandleCreated Then
            SendMessage(c.Handle, WM_SETREDRAW, New IntPtr(1), IntPtr.Zero)
            c.Refresh()
        End If
    End Sub

    <StructLayout(LayoutKind.Sequential)>
    Private Structure COMBOBOXINFO
        Public cbSize As Integer
        Public rcItem As RECT
        Public rcButton As RECT
        Public stateButton As Integer
        Public hwndCombo As IntPtr
        Public hwndItem As IntPtr
        Public hwndList As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left, Top, Right, Bottom As Integer
    End Structure

    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE_OLD As Integer = 19
    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20
    Private Const DWMWA_CAPTION_COLOR As Integer = 35
    Private Const DWMWA_TEXT_COLOR As Integer = 36
    Private Const DWMWA_COLOR_DEFAULT As Integer = &HFFFFFFFF

    Private Const LVM_GETHEADER As Integer = &H1000 + 31

    Private Const APPMODE_DEFAULT As Integer = 0
    Private Const APPMODE_ALLOWDARK As Integer = 1

    ''' <summary>Tells uxtheme that this process may use dark mode. Needed for dark scroll bars on some builds.</summary>
    Public Sub SetProcessDarkMode(dark As Boolean)
        Try
            SetPreferredAppMode(If(dark, APPMODE_ALLOWDARK, APPMODE_DEFAULT))
            FlushMenuThemes()
        Catch sup1 As Exception
            LogSuppressed(sup1, "NativeThemeModule.SetProcessDarkMode")
            'Undocumented export; ignore on builds that lack it.
        End Try
    End Sub

    ''' <summary>
    ''' Window themes can only be applied once a control's window exists. Forcing the
    ''' window into existence (Control.Handle) is what made theming a big form slow:
    ''' every control on every hidden tab page got a window it did not need yet.
    ''' So each theme action runs now if the window exists, and otherwise when it is
    ''' created; it also runs again after a recreate, which used to lose the theme.
    ''' A control can have several actions (its own window plus, say, its header).
    ''' </summary>
    Private ReadOnly themeActions As New ConditionalWeakTable(Of Control, Dictionary(Of String, System.Action))

    Private Sub WhenHandleReady(c As Control, slot As String, apply As System.Action)
        Dim actions As Dictionary(Of String, System.Action) = themeActions.GetOrCreateValue(c)
        actions(slot) = apply
        RemoveHandler c.HandleCreated, AddressOf ThemedControl_HandleCreated
        AddHandler c.HandleCreated, AddressOf ThemedControl_HandleCreated
        If c.IsHandleCreated Then apply()
    End Sub

    Private Sub ThemedControl_HandleCreated(sender As Object, e As EventArgs)
        Dim actions As Dictionary(Of String, System.Action) = Nothing
        If themeActions.TryGetValue(DirectCast(sender, Control), actions) Then
            For Each apply As System.Action In actions.Values.ToArray()
                apply()
            Next
        End If
    End Sub

    Private Sub ApplyWindowTheme(hWnd As IntPtr, theme As String)
        Try
            SetWindowTheme(hWnd, theme, Nothing)
        Catch sup2 As Exception
            LogSuppressed(sup2, "NativeThemeModule.ApplyWindowTheme")
        End Try
    End Sub

    ''' <summary>Applies the dark explorer theme (dark scroll bars, dark selection) to a control's window.</summary>
    Public Sub SetWindowDark(c As Control, dark As Boolean, Optional theme As String = "DarkMode_Explorer")
        WhenHandleReady(c, "window", Sub() ApplyWindowTheme(c.Handle, If(dark, theme, Nothing)))
    End Sub

    ''' <summary>Themes a combo box and its pop-up list.</summary>
    Public Sub SetComboBoxDark(cb As ComboBox, dark As Boolean)
        WhenHandleReady(cb, "window",
                        Sub()
                            ApplyWindowTheme(cb.Handle, If(dark, "DarkMode_CFD", Nothing))
                            Try
                                Dim info As New COMBOBOXINFO
                                info.cbSize = Marshal.SizeOf(info)
                                If GetComboBoxInfo(cb.Handle, info) AndAlso info.hwndList <> IntPtr.Zero Then
                                    ApplyWindowTheme(info.hwndList, If(dark, "DarkMode_Explorer", Nothing))
                                End If
                            Catch sup3 As Exception
                                LogSuppressed(sup3, "NativeThemeModule.SetComboBoxDark")
                            End Try
                        End Sub)
    End Sub

    ''' <summary>Colours the window title bar to match the theme.</summary>
    Public Sub SetTitleBarColor(f As Form, dark As Boolean, background As Color, text As Color)
        Try
            Dim useDark As Integer = If(dark, 1, 0)
            If DwmSetWindowAttribute(f.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, useDark, 4) <> 0 Then
                DwmSetWindowAttribute(f.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, useDark, 4)
            End If

            'Windows 11 lets us pick the exact caption colours.
            Dim captionColor As Integer = If(dark, ToColorRef(background), DWMWA_COLOR_DEFAULT)
            Dim textColor As Integer = If(dark, ToColorRef(text), DWMWA_COLOR_DEFAULT)
            DwmSetWindowAttribute(f.Handle, DWMWA_CAPTION_COLOR, captionColor, 4)
            DwmSetWindowAttribute(f.Handle, DWMWA_TEXT_COLOR, textColor, 4)
        Catch sup4 As Exception
            LogSuppressed(sup4, "NativeThemeModule.SetTitleBarColor")
        End Try
    End Sub

    Private Function ToColorRef(c As Color) As Integer
        Return c.R Or (CInt(c.G) << 8) Or (CInt(c.B) << 16)
    End Function

    ''' <summary>Themes a list view's header window so its background follows the theme.</summary>
    Public Sub SetListViewHeaderDark(lv As ListView, dark As Boolean)
        WhenHandleReady(lv, "header",
                        Sub()
                            Try
                                Dim hdr As IntPtr = SendMessage(lv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero)
                                If hdr <> IntPtr.Zero Then ApplyWindowTheme(hdr, If(dark, "DarkMode_ItemsView", Nothing))
                            Catch sup5 As Exception
                                LogSuppressed(sup5, "NativeThemeModule.SetListViewHeaderDark")
                            End Try
                        End Sub)
    End Sub

    ''' <summary>
    ''' Paints over a tab control's native chrome (tab strip, tab headers and page border)
    ''' after Windows has drawn it, so the whole control follows the theme.
    ''' </summary>
    Public Class TabControlPainter
        Inherits NativeWindow

        Private Const WM_PAINT As Integer = &HF

        Private Shared ReadOnly painters As New Dictionary(Of TabControl, TabControlPainter)
        Private ReadOnly tab As TabControl

        Public Shared Sub Attach(tc As TabControl)
            If painters.ContainsKey(tc) Then Return
            Dim p As New TabControlPainter(tc)
            painters(tc) = p
            p.AssignHandle(tc.Handle)
            AddHandler tc.HandleCreated, AddressOf p.OnHandleCreated
            AddHandler tc.HandleDestroyed, AddressOf p.OnHandleDestroyed
            tc.Invalidate()
        End Sub

        Public Shared Sub Detach(tc As TabControl)
            Dim p As TabControlPainter = Nothing
            If painters.TryGetValue(tc, p) Then
                RemoveHandler tc.HandleCreated, AddressOf p.OnHandleCreated
                RemoveHandler tc.HandleDestroyed, AddressOf p.OnHandleDestroyed
                p.ReleaseHandle()
                p.DisposeIcons()
                painters.Remove(tc)
                tc.Invalidate()
            End If
        End Sub

        'ImageList.Images(key) allocates a new bitmap on every access, so keep one per key.
        Private ReadOnly icons As New Dictionary(Of String, Image)

        Private Sub DisposeIcons()
            For Each img As Image In icons.Values
                img.Dispose()
            Next
            icons.Clear()
        End Sub

        Private Function TabIcon(page As TabPage) As Image
            Dim list As ImageList = tab.ImageList
            If list Is Nothing Then Return Nothing
            Dim key As String = page.ImageKey
            If String.IsNullOrEmpty(key) Then
                If page.ImageIndex < 0 OrElse page.ImageIndex >= list.Images.Count Then Return Nothing
                key = "#" & page.ImageIndex
            ElseIf Not list.Images.ContainsKey(key) Then
                Return Nothing
            End If
            Dim img As Image = Nothing
            If Not icons.TryGetValue(key, img) Then
                img = If(key.StartsWith("#"), list.Images(page.ImageIndex), list.Images(key))
                icons(key) = img
            End If
            Return img
        End Function

        Private Sub New(tc As TabControl)
            tab = tc
        End Sub

        Private Sub OnHandleCreated(sender As Object, e As EventArgs)
            AssignHandle(tab.Handle)
        End Sub

        Private Sub OnHandleDestroyed(sender As Object, e As EventArgs)
            ReleaseHandle()
        End Sub

        'The strip is painted here in full, in place of the native drawing. Letting the
        'control paint its light chrome first and covering it afterwards flashed on
        'every repaint.
        Protected Overrides Sub WndProc(ByRef m As Message)
            Select Case m.Msg
                Case WM_ERASEBKGND
                    m.Result = New IntPtr(1)
                    Return
                Case WM_PAINT
                    Dim ps As New PAINTSTRUCT
                    Dim hdc As IntPtr = BeginPaint(m.HWnd, ps)
                    Try
                        Using g As Graphics = Graphics.FromHdc(hdc)
                            PaintChrome(g)
                        End Using
                    Catch sup6 As Exception
                        LogSuppressed(sup6, "NativeThemeModule.WndProc")
                    Finally
                        EndPaint(m.HWnd, ps)
                    End Try
                    m.Result = IntPtr.Zero
                    Return
            End Select
            MyBase.WndProc(m)
        End Sub

        Private Sub PaintChrome(g As Graphics)
            Dim bg As Color = ProgramSet.colorBackground
            Dim fg As Color = ProgramSet.colorLabelText
            Dim selectedBg As Color = ProgramSet.colorFieldBackground
            Dim border As Color = ProgramSet.colorPanelBackground

            Dim client As Rectangle = tab.ClientRectangle
            Dim display As Rectangle = tab.DisplayRectangle

            'Everything that is not the page area: strip, borders and gaps.
            Using bgBrush As New SolidBrush(bg)
                Using region As New Region(client)
                    If tab.TabCount > 0 Then region.Exclude(display)
                    g.FillRegion(bgBrush, region)
                End Using
            End Using
            If tab.TabCount = 0 Then Return

            'Frame around the page area.
            Using pen As New Pen(border)
                Dim frame As Rectangle = display
                frame.Inflate(1, 1)
                frame.Width -= 1
                frame.Height -= 1
                g.DrawRectangle(pen, frame)
            End Using

            'Tab headers: icon and caption centred in the tab.
            Const gap As Integer = 6
            Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPadding
            For i = 0 To tab.TabCount - 1
                Dim r As Rectangle = tab.GetTabRect(i)
                Dim selected As Boolean = (i = tab.SelectedIndex)
                Using brush As New SolidBrush(If(selected, selectedBg, bg))
                    g.FillRectangle(brush, r)
                End Using
                Using pen As New Pen(border)
                    g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1)
                End Using

                Dim text As String = tab.TabPages(i).Text
                Dim icon As Image = TabIcon(tab.TabPages(i))
                If icon Is Nothing Then
                    TextRenderer.DrawText(g, text, tab.Font, r, fg, flags Or TextFormatFlags.HorizontalCenter)
                Else
                    Dim textWidth As Integer = TextRenderer.MeasureText(g, text, tab.Font, r.Size, flags).Width
                    Dim x As Integer = r.X + Math.Max(0, (r.Width - (icon.Width + gap + textWidth)) \ 2)
                    g.DrawImage(icon, New Rectangle(x, r.Y + (r.Height - icon.Height) \ 2, icon.Width, icon.Height))
                    Dim textLeft As Integer = x + icon.Width + gap
                    TextRenderer.DrawText(g, text, tab.Font, New Rectangle(textLeft, r.Y, r.Right - textLeft, r.Height), fg, flags Or TextFormatFlags.Left)
                End If
            Next
        End Sub
    End Class

    ''' <summary>Draws themed up/down arrows over the spinner buttons of a NumericUpDown or DomainUpDown.</summary>
    Public Sub UpDownButtons_Paint(sender As Object, e As PaintEventArgs)
        Dim buttons As Control = sender
        Dim r As Rectangle = buttons.ClientRectangle
        Dim bg As Color = ProgramSet.colorPanelBackground
        Dim fg As Color = ProgramSet.colorLabelText

        Using brush As New SolidBrush(bg)
            e.Graphics.FillRectangle(brush, r)
        End Using

        Dim half As Integer = r.Height \ 2
        Dim cx As Integer = r.X + r.Width \ 2
        Dim size As Integer = Math.Max(2, Math.Min(r.Width, half) \ 4)

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Using brush As New SolidBrush(fg)
            Dim upY As Integer = r.Y + half \ 2
            e.Graphics.FillPolygon(brush, {New Point(cx - size, upY + size \ 2 + 1),
                                           New Point(cx + size, upY + size \ 2 + 1),
                                           New Point(cx, upY - size \ 2)})
            Dim downY As Integer = r.Y + half + half \ 2
            e.Graphics.FillPolygon(brush, {New Point(cx - size, downY - size \ 2),
                                           New Point(cx + size, downY - size \ 2),
                                           New Point(cx, downY + size \ 2 + 1)})
        End Using
    End Sub

    ''' <summary>Draws a themed column header for an owner-drawn ListView.</summary>
    Public Sub ListView_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs)
        Dim bg As Color = ProgramSet.colorPanelBackground
        Dim fg As Color = ProgramSet.colorLabelText
        Using brush As New SolidBrush(bg)
            e.Graphics.FillRectangle(brush, e.Bounds)
        End Using
        Using pen As New Pen(ProgramSet.colorBackground)
            e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom)
            e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
        Dim textRect As Rectangle = e.Bounds
        textRect.Inflate(-4, 0)
        Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis
        Select Case e.Header.TextAlign
            Case HorizontalAlignment.Center : flags = flags Or TextFormatFlags.HorizontalCenter
            Case HorizontalAlignment.Right : flags = flags Or TextFormatFlags.Right
            Case Else : flags = flags Or TextFormatFlags.Left
        End Select
        TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, textRect, fg, flags)
    End Sub

    Public Sub ListView_DrawItem(sender As Object, e As DrawListViewItemEventArgs)
        e.DrawDefault = True
    End Sub

    Public Sub ListView_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs)
        e.DrawDefault = True
    End Sub
End Module
