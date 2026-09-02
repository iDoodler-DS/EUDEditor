Imports System.IO
Imports System.Text

''' <summary>
''' Builds the map from the epScript the new trigger editor keeps.
'''
''' The build of today gathers a great deal: the data editors, the button sets,
''' the plugins, and epScript written out of the tree of nodes. This one takes the
''' source as it stands and gives it to euddraft, so what is built is what is on
''' the screen and nothing else.
'''
''' That makes it a build for trying the source out, not a replacement for the
''' other one. What it leaves out is written down in
''' development/eps-trigger-editor.md.
''' </summary>
Namespace EpsSource

    Public Module EpsBuild

        Public Const SourceName As String = "EpsTriggers.eps"
        Public Const SettingsName As String = "EpsTriggers.eds"

        ''' <summary>Writes the source and the settings beside it, and runs euddraft.</summary>
        Public Sub Run(source As String)
            If Not ProjectSet.isload Then
                MsgBox("Open a project first.", MsgBoxStyle.Information, "EUD Editor")
                Return
            End If
            If String.IsNullOrWhiteSpace(ProjectSet.InputMap) OrElse
               String.IsNullOrWhiteSpace(ProjectSet.OutputMap) Then
                MsgBox("This project has no map to read or to write. Set them on the Project tab.",
                       MsgBoxStyle.Exclamation, "EUD Editor")
                Return
            End If

            Try
                Dim folder As String = BuildFolder()
                Directory.CreateDirectory(folder)

                File.WriteAllText(Path.Combine(folder, SourceName), source)
                File.WriteAllText(Path.Combine(folder, SettingsName), Settings())

                BuildRunner.Start(BaseFolder(), SettingsName)
            Catch ex As Exception
                LogException(ex, "building from the epScript source")
                MsgBox("The build did not start: " & ex.Message, MsgBoxStyle.Critical, "EUD Editor")
            End Try
        End Sub

        Private Function BaseFolder() As String
            If ProjectSet.filename.EndsWith(".e2p") Then
                Return ProjectSet.filename.Replace("\" & GetSafeName(ProjectSet.filename), "")
            End If
            Return My.Application.Info.DirectoryPath & "\Data"
        End Function

        Private Function BuildFolder() As String
            Return Path.Combine(BaseFolder(), "eudplibdata")
        End Function

        'What euddraft is told to do: read one map, write another, and put this
        'source in between. Nothing else of the project takes part.
        Private Function Settings() As String
            Dim out As New StringBuilder()
            out.AppendLine("[main]")
            out.AppendLine()
            out.AppendLine("input: " & ProjectSet.InputMap)
            out.AppendLine("output: " & ProjectSet.OutputMap)
            If ProjectSet.epTraceDebug Then out.AppendLine("debug: 1")
            out.AppendLine()
            out.AppendLine("[" & SourceName & "]")
            out.AppendLine()
            Return out.ToString()
        End Function
    End Module
End Namespace
