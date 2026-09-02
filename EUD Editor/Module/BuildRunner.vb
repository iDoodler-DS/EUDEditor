Imports System.ComponentModel

''' <summary>
''' Runs euddraft on the project's .eds file and streams its output into the main
''' window's log panel. Replaces the old BulidForm popup.
''' </summary>
Module BuildRunner
    Private process As Process
    Private baseFolder As String
    Private errorText As String = ""
    Private retries As Integer

    'VB default form instances are per thread, so the main window is captured on the
    'UI thread and used from the reader callbacks through this field.
    Private ui As Main

    'euddraft occasionally fails to start its zip import; the old code retried without limit.
    Private Const ZlibError As String = "zipimport.ZipImportError: can't decompress data; zlib not available"
    Private Const MaxRetries As Integer = 5

    Public Sub Start(folder As String)
        ui = Main
        baseFolder = folder
        errorText = ""
        retries = 0
        ui.ResetLog(Lan.GetMsgText("build") & vbCrLf)
        Launch()
    End Sub

    Private Sub Launch()
        Dim filename As String = baseFolder & "\eudplibdata\EUDEditor.eds"
        Dim info As New ProcessStartInfo(ProgramSet.euddraftDirec, """" & filename & """") With {
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .RedirectStandardInput = True,
            .WindowStyle = ProcessWindowStyle.Hidden,
            .CreateNoWindow = True,
            .UseShellExecute = False}

        process = New Process With {.StartInfo = info}
        AddHandler process.OutputDataReceived, AddressOf Process_OutputDataReceived
        AddHandler process.ErrorDataReceived, AddressOf Process_ErrorDataReceived

        Try
            process.Start()
        Catch ex As Win32Exception
            MsgBox(Lan.GetText("Msgbox", "neeuddraft"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
            SettingForm.ShowDialog()
            Return
        End Try

        'euddraft waits for Enter before it exits.
        process.StandardInput.Write(vbCrLf)
        process.BeginOutputReadLine()
        process.BeginErrorReadLine()

        'The parameterless WaitForExit also waits for the async readers to drain.
        Dim p As Process = process
        Dim owner As Main = ui
        Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                   p.WaitForExit()
                                                   owner.BeginInvoke(New System.Action(AddressOf Finish))
                                               End Sub)
    End Sub

    Private Sub Process_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then ui.AppendLog(e.Data & vbCrLf)
    End Sub

    Private Sub Process_ErrorDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then errorText &= e.Data & vbCrLf
    End Sub

    'Runs on the UI thread once euddraft has exited.
    Private Sub Finish()
        Dim output As String = ui.LogText
        process.Dispose()
        process = Nothing

        If errorText.Contains(ZlibError) AndAlso retries < MaxRetries Then
            retries += 1
            errorText = ""
            ui.AppendLog(vbCrLf)
            Launch()
            Return
        End If

        DeledtDebugpy()

        If errorText <> "" OrElse output.Contains("[Error]") Then
            TEErrorText = errorText
            TEErrorText2 = output
            ui.AppendLog(vbCrLf & vbCrLf & Lan.GetMsgText("buildError") & vbCrLf & vbCrLf & errorText, Color.LightSalmon)
            ui.Activate()
        Else
            My.Computer.Audio.Play(My.Resources.successBulid, AudioPlayMode.Background)
            If ProjectSet.SCDBUse Then
                StartCheckSum()
            End If
        End If
    End Sub
End Module
