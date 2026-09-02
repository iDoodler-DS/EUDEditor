Imports System.IO
Imports System.Text

''' <summary>
''' Appends exceptions and diagnostic messages to ErrorLog.txt next to the executable.
''' Never throws; logging must not make a bad situation worse.
''' </summary>
Module ErrorLogModule
    Private ReadOnly logLock As New Object
    Private ReadOnly reportedPlaces As New HashSet(Of String)

    Public ReadOnly Property LogPath As String
        Get
            Return Path.Combine(My.Application.Info.DirectoryPath, "ErrorLog.txt")
        End Get
    End Property

    Public Sub LogException(ex As Exception, Optional context As String = "")
        Try
            Dim sb As New StringBuilder()
            sb.Append("==== ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.Append("  v").Append(ProgramSet.Version)
            If context <> "" Then sb.Append("  [").Append(context).Append("]")
            sb.AppendLine()

            Dim current As Exception = ex
            Dim depth As Integer = 0
            While current IsNot Nothing
                If depth > 0 Then sb.Append("-- inner: ")
                sb.Append(current.GetType().FullName).Append(": ").AppendLine(current.Message)
                sb.AppendLine(current.StackTrace)
                current = current.InnerException
                depth += 1
            End While
            sb.AppendLine()

            Write(sb.ToString())
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Records an error that the code goes on from. A place reports one time in a run,
    ''' because some of these places run many times a second. The first report says
    ''' where to look; a count of the rest is of no use.
    ''' </summary>
    Public Sub LogSuppressed(ex As Exception, place As String)
        Try
            SyncLock reportedPlaces
                If reportedPlaces.Contains(place) Then Return
                reportedPlaces.Add(place)
            End SyncLock
            LogException(ex, "went on after: " & place)
        Catch
        End Try
    End Sub

    Public Sub LogMessage(message As String)
        Try
            Write("---- " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "  " & message & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Sub Write(text As String)
        SyncLock logLock
            File.AppendAllText(LogPath, text, Encoding.UTF8)
        End SyncLock
    End Sub
End Module
