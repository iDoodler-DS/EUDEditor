﻿Public Class SoundLenForm
    Private WithEvents proc As New Process With {.EnableRaisingEvents = True}

    Dim tempfoluder As String = My.Application.Info.DirectoryPath & "\Data\temp\"
    Private interval As Double
    Private openfile As String
    Private workstatus As Integer


    Private Sub SeperateSound(wavFile As String)
        openfile = wavFile

        interval = Soundinterval * 1.05
        BackgroundWorker1.RunWorkerAsync()
    End Sub


    Private Sub SoundLenForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = Lan.GetMsgText("sonudsplittext")

        Label1.Text = ""
        Timer1.Enabled = True
        workstatus = 0
        proc.StartInfo.FileName = IO.Path.Combine(Application.StartupPath, "ffmpeg.exe")
        ThemeSetForm.SetControlColor(Me)
    End Sub
    Private Sub SoundLenForm_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        Timer1.Enabled = False
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim purefilename As String = openfile.Split("\").Last

        Dim output As String = tempfoluder & "M" & workstatus & "_"


        openfile = Chr(34) & openfile & Chr(34)


        Label1.Text = Lan.GetMsgText("sonudsplit").Replace("$S0$", openfile.Split("\").Last)


        '    Label1.Text = """" & openfile.Split("\").Last & "을 분리하는 중 ...(" & i & "/" & maxlen & ")"

        If ProjectSet.filename.EndsWith(".e2p") Then
            With proc.StartInfo
                .Arguments = "-i " & openfile & " -f segment -segment_time " & interval & " -y -c copy " & Chr(34) & output & "%d" & ".ogg" & Chr(34)
                .WindowStyle = ProcessWindowStyle.Hidden

            End With
            proc.Start()
            proc.WaitForExit()
        Else
            With proc.StartInfo
                .Arguments = "-i " & openfile & " -y " & Chr(34) & output & ".wav" & Chr(34)
                .WindowStyle = ProcessWindowStyle.Hidden

            End With
            proc.Start()
            proc.WaitForExit()

            With proc.StartInfo
                .Arguments = "-i " & Chr(34) & output & ".wav" & Chr(34) & " -y " & Chr(34) & output & ".ogg" & Chr(34)
                .WindowStyle = ProcessWindowStyle.Hidden

            End With
            proc.Start()
            proc.WaitForExit()

            With proc.StartInfo
                .Arguments = "-i " & Chr(34) & output & ".ogg" & Chr(34) & " -f segment -segment_time " & interval & " -y -c copy " & Chr(34) & output & "%d" & ".ogg" & Chr(34)
                .WindowStyle = ProcessWindowStyle.Hidden

            End With
            proc.Start()
            proc.WaitForExit()
        End If


        workstatus += 1
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If workstatus < Soundlist.Count Then
            If BackgroundWorker1.IsBusy = False Then
                SeperateSound(Soundlist(workstatus))
            End If
        Else
            Me.Close()
        End If
    End Sub
End Class