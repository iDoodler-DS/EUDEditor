﻿Public Class NQCForm
    Public returnstring As String

    Private Sub CheckValidity()
        Dim truepoint As Byte = 0
        If EasyCompletionComboBox1.SelectedIndex <> -1 And RadioButton2.Checked = True Then
            truepoint += 1
        ElseIf RadioButton1.Checked = True Then
            truepoint += 1
        End If

        If EasyCompletionComboBox2.SelectedIndex <> -1 And RadioButton4.Checked = True Then
            truepoint += 1
        ElseIf RadioButton3.Checked = True Then
            truepoint += 1
        End If
        If truepoint > 1 Then
            Button5.Enabled = True
        Else
            Button5.Enabled = False
        End If

    End Sub

    Private Sub NQCForm_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        If RadioButton1.Checked = True Then '직접 입력일 경우
            returnstring = TextBox1.Text
        Else
            returnstring = EasyCompletionComboBox1.SelectedItem
        End If
        If RadioButton3.Checked = True Then '직접 입력일 경우
            returnstring = returnstring & "#" & TextBox2.Text
        Else
            returnstring = returnstring & "#" & EasyCompletionComboBox2.SelectedIndex & "#" & NumericUpDown1.Value
        End If
    End Sub

    Dim tload As Boolean = False
    Private Sub NQCForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Lan.SetLanguage(Me)

        TextBox1.Text = ""
        TextBox2.Text = ""
        EasyCompletionComboBox1.ResetText()
        EasyCompletionComboBox2.ResetText()

        tload = True
        EasyCompletionComboBox2.Items.Clear()

        For i = 0 To CODE(0).Count - 1
            If DatEditDATA(DTYPE.units).ReadValue("Unit Map String", i) = 0 Then
                EasyCompletionComboBox2.Items.Add(CODE(0)(i))
            Else
                Try
                    EasyCompletionComboBox2.Items.Add(ProjectSet.CHKSTRING(-1 + ProjectSet.CHKUNITNAME(i)) & "(" & CODE(0)(i) & ")")
                Catch ex As Exception
                    EasyCompletionComboBox2.Items.Add(CODE(0)(i))
                End Try
            End If
        Next

        If returnstring = "" Then
            RadioButton2.Checked = True
            EasyCompletionComboBox1.SelectedIndex = -1
            EasyCompletionComboBox1.ResetText()

            EasyCompletionComboBox2.SelectedIndex = -1
            EasyCompletionComboBox2.ResetText()
            Button5.Enabled = False
        Else
            Dim condstr As String = returnstring.Split("#")(0).Trim
            Dim deathunit As String = returnstring.Split("#")(1).Trim
            Dim deathval As String
            Try
                deathval = returnstring.Split("#")(2).Trim
            Catch ex As Exception
                deathval = "Null"
            End Try

            Try
                If EasyCompletionComboBox1.Items.Contains(condstr) Then
                    EasyCompletionComboBox1.SelectedIndex = EasyCompletionComboBox1.Items.IndexOf(condstr)
                    RadioButton2.Checked = True
                Else
                    TextBox1.Text = condstr
                    RadioButton1.Checked = True
                    EasyCompletionComboBox1.SelectedIndex = -1
                    EasyCompletionComboBox1.ResetText()
                End If
            Catch ex As Exception
                TextBox1.Text = condstr
                RadioButton1.Checked = True
                EasyCompletionComboBox1.SelectedIndex = -1
                EasyCompletionComboBox1.ResetText()
            End Try


            Dim checkstr As Boolean = True
            RadioButton4.Checked = True
            TextBox2.Enabled = False
            FlowLayoutPanel2.Enabled = True
            Try
                EasyCompletionComboBox2.SelectedIndex = deathunit
            Catch ex As Exception
                EasyCompletionComboBox2.SelectedIndex = 0
                checkstr = False
            End Try

            Try
                NumericUpDown1.Value = deathval
            Catch ex As Exception
                NumericUpDown1.Value = 0
                checkstr = False
            End Try

            If checkstr = False Then
                RadioButton3.Checked = True
                TextBox2.Enabled = True
                TextBox2.Text = deathunit
                FlowLayoutPanel2.Enabled = False
            End If
        End If
        tload = False
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked Then
            TextBox1.Enabled = True
            EasyCompletionComboBox1.Enabled = False
            CheckValidity()
        End If
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If RadioButton2.Checked Then
            TextBox1.Enabled = False
            EasyCompletionComboBox1.Enabled = True
            CheckValidity()
        End If
    End Sub

    Private Sub EasyCompletionComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EasyCompletionComboBox1.SelectedIndexChanged
        If tload = False Then
            CheckValidity()
        End If
    End Sub

    Private Sub EasyCompletionComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EasyCompletionComboBox2.SelectedIndexChanged
        If tload = False Then
            CheckValidity()
        End If
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        If RadioButton3.Checked Then
            TextBox2.Enabled = True
            FlowLayoutPanel2.Enabled = False
            CheckValidity()
        End If
    End Sub

    Private Sub RadioButton4_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton4.CheckedChanged
        If RadioButton4.Checked Then
            TextBox2.Enabled = False
            FlowLayoutPanel2.Enabled = True
            CheckValidity()
        End If
    End Sub

End Class