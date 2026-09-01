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
        Catch
            'Undocumented export; ignore on builds that lack it.
        End Try
    End Sub

    ''' <summary>Applies the dark explorer theme (dark scroll bars, dark selection) to a control's window.</summary>
    Public Sub SetWindowDark(c As Control, dark As Boolean, Optional theme As String = "DarkMode_Explorer")
        Try
            If dark Then
                SetWindowTheme(c.Handle, theme, Nothing)
            Else
                SetWindowTheme(c.Handle, Nothing, Nothing)
            End If
        Catch
        End Try
    End Sub

    ''' <summary>Themes the pop-up list of a combo box.</summary>
    Public Sub SetComboBoxDark(cb As ComboBox, dark As Boolean)
        Try
            SetWindowDark(cb, dark, "DarkMode_CFD")
            Dim info As New COMBOBOXINFO
            info.cbSize = Marshal.SizeOf(info)
            If GetComboBoxInfo(cb.Handle, info) AndAlso info.hwndList <> IntPtr.Zero Then
                If dark Then
                    SetWindowTheme(info.hwndList, "DarkMode_Explorer", Nothing)
                Else
                    SetWindowTheme(info.hwndList, Nothing, Nothing)
                End If
            End If
        Catch
        End Try
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
        Catch
        End Try
    End Sub

    Private Function ToColorRef(c As Color) As Integer
        Return c.R Or (CInt(c.G) << 8) Or (CInt(c.B) << 16)
    End Function

    ''' <summary>Themes a list view's header window so its background follows the theme.</summary>
    Public Sub SetListViewHeaderDark(lv As ListView, dark As Boolean)
        Try
            Dim header As IntPtr = SendMessage(lv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero)
            If header <> IntPtr.Zero Then
                If dark Then
                    SetWindowTheme(header, "DarkMode_ItemsView", Nothing)
                Else
                    SetWindowTheme(header, Nothing, Nothing)
                End If
            End If
        Catch
        End Try
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
                painters.Remove(tc)
                tc.Invalidate()
            End If
        End Sub

        Private Sub New(tc As TabControl)
            tab = tc
        End Sub

        Private Sub OnHandleCreated(sender As Object, e As EventArgs)
            AssignHandle(tab.Handle)
        End Sub

        Private Sub OnHandleDestroyed(sender As Object, e As EventArgs)
            ReleaseHandle()
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            MyBase.WndProc(m)
            If m.Msg = WM_PAINT Then
                Try
                    Using g As Graphics = Graphics.FromHwnd(tab.Handle)
                        PaintChrome(g)
                    End Using
                Catch
                End Try
            End If
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
                    region.Exclude(display)
                    g.FillRegion(bgBrush, region)
                End Using
            End Using

            'Frame around the page area.
            Using pen As New Pen(border)
                Dim frame As Rectangle = display
                frame.Inflate(1, 1)
                frame.Width -= 1
                frame.Height -= 1
                g.DrawRectangle(pen, frame)
            End Using

            'Tab headers.
            For i = 0 To tab.TabCount - 1
                Dim r As Rectangle = tab.GetTabRect(i)
                Dim selected As Boolean = (i = tab.SelectedIndex)
                Using brush As New SolidBrush(If(selected, selectedBg, bg))
                    g.FillRectangle(brush, r)
                End Using
                Using pen As New Pen(border)
                    g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1)
                End Using
                TextRenderer.DrawText(g, tab.TabPages(i).Text, tab.Font, r, fg,
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine)
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
