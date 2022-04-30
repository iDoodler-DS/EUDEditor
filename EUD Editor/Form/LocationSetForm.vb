Public Class LocationSetForm
    Public rawstring As String
    Public lists As New List(Of String)


    Private Sub LocationSetForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Lan.SetLanguage(Me)

        ListBox1.SelectedIndex = -1
        EasyCompletionComboBox1.Enabled = False

        Dim liststr() As String = rawstring.Split(",")



        EasyCompletionComboBox1.Items.Clear()
        EasyCompletionComboBox1.Items.Add("None")
        For i = 0 To 254
            If ProjectSet.CHKLOCATIONNAME.Count <> 0 Then
                If ProjectSet.CHKLOCATIONNAME(i) <> 0 Then
                    EasyCompletionComboBox1.Items.Add(ProjectSet.CHKSTRING(ProjectSet.CHKLOCATIONNAME(i) - 1))
                Else
                    EasyCompletionComboBox1.Items.Add("Location " & i)
                End If
            Else
                EasyCompletionComboBox1.Items.Add("Location " & i)
            End If

        Next
        EasyCompletionComboBox1.ResetText()

        lists.Clear()
        ListBox1.Items.Clear()
        lists.Add(0)
        ListBox1.Items.Add("First Human Player : None")

        If liststr(0) <> "" Then
            lists(0) = liststr(0)
            ListBox1.Items(0) = "First Human Player : " & EasyCompletionComboBox1.Items(liststr(0))
        End If
    End Sub


    Private loadstatus As Boolean = True
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If loadstatus Then
            loadstatus = False
            EasyCompletionComboBox1.Enabled = True
            If ListBox1.SelectedIndex <> -1 Then
                Try
                    EasyCompletionComboBox1.SelectedIndex = lists(ListBox1.SelectedIndex)
                Catch ex As Exception
                    EasyCompletionComboBox1.SelectedIndex = 0
                End Try
            End If
            loadstatus = True
        End If
    End Sub

    Private Sub EasyCompletionComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EasyCompletionComboBox1.SelectedIndexChanged
        If loadstatus Then
            loadstatus = False
            If ListBox1.SelectedIndex <> -1 Then
                ListBox1.Items(ListBox1.SelectedIndex) = "First Human Player : " & EasyCompletionComboBox1.SelectedItem
                lists(ListBox1.SelectedIndex) = EasyCompletionComboBox1.SelectedIndex
            End If
            loadstatus = True
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

    End Sub
End Class