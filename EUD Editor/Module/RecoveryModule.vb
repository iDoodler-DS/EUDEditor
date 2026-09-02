Imports System.IO

''' <summary>
''' Keeps a recovery copy of the open project. The editor writes the copy every few
''' minutes while there are changes that are not saved, and deletes it after a save
''' or a close. A copy that is still there at the next start means the editor stopped
''' without a save, so the user is offered the copy.
'''
''' The copy is the same text as a project file, so the user can open it by hand.
''' </summary>
Module RecoveryModule
    Private ReadOnly writeLock As New Object

    Public ReadOnly Property RecoveryFolder As String
        Get
            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EUD_Editor", "Recovery")
        End Get
    End Property

    Public ReadOnly Property RecoveryFile As String
        Get
            Return Path.Combine(RecoveryFolder, "project.e2s")
        End Get
    End Property

    Private ReadOnly Property RecoveryInfoFile As String
        Get
            Return Path.Combine(RecoveryFolder, "project.info")
        End Get
    End Property

    ''' <summary>What a recovery copy says about itself.</summary>
    Public Class RecoveryInfo
        Public Property ProjectPath As String = ""
        Public Property Written As Date

        Public ReadOnly Property DisplayName As String
            Get
                If ProjectPath = "" Then Return "an unnamed project"
                Return Path.GetFileName(ProjectPath)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Writes the recovery copy, if the project has changes that are not saved.
    ''' Never throws: a failed recovery copy must not stop the user's work.
    ''' </summary>
    Public Function WriteCopy() As Boolean
        If Not ProjectSet.isload Then Return False
        If ProjectSet.saveStatus Then Return False   'no change since the last save

        Try
            SyncLock writeLock
                Directory.CreateDirectory(RecoveryFolder)

                'Write beside the copy first, then move, so a stop in the middle
                'leaves the copy of the last round.
                Dim staging As String = RecoveryFile & ".part"
                File.WriteAllText(staging, ProjectSet.ProjectText())
                If File.Exists(RecoveryFile) Then File.Delete(RecoveryFile)
                File.Move(staging, RecoveryFile)

                File.WriteAllText(RecoveryInfoFile,
                    "Path : " & ProjectSet.filename & Environment.NewLine &
                    "Written : " & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & Environment.NewLine &
                    "Version : " & ProgramSet.Version & Environment.NewLine)
            End SyncLock
            Return True
        Catch ex As Exception
            LogException(ex, "writing the recovery copy")
            Return False
        End Try
    End Function

    ''' <summary>Takes the recovery copy away. Call after a save, or after a close.</summary>
    Public Sub Clear()
        Try
            SyncLock writeLock
                For Each name As String In {RecoveryFile, RecoveryInfoFile, RecoveryFile & ".part"}
                    If File.Exists(name) Then File.Delete(name)
                Next
            End SyncLock
        Catch ex As Exception
            LogException(ex, "clearing the recovery copy")
        End Try
    End Sub

    ''' <summary>
    ''' Keeps a copy the user did not want to open, under a name of its own, so the
    ''' question comes only once and the work is still there to find.
    ''' </summary>
    Public Sub SetAside()
        Try
            SyncLock writeLock
                If Not File.Exists(RecoveryFile) Then Return
                Dim kept As String = Path.Combine(
                    RecoveryFolder,
                    "project-" & File.GetLastWriteTime(RecoveryFile).ToString("yyyyMMdd-HHmmss") & ".e2s")
                If File.Exists(kept) Then File.Delete(kept)
                File.Move(RecoveryFile, kept)
                If File.Exists(RecoveryInfoFile) Then File.Delete(RecoveryInfoFile)
            End SyncLock
        Catch ex As Exception
            LogException(ex, "keeping the recovery copy")
        End Try
    End Sub

    ''' <summary>
    ''' Copies the recovery file to a name of its own and gives back that name. The
    ''' editor closes the open project before it opens the copy, and a close takes the
    ''' recovery file away, so the copy must stand somewhere else first.
    ''' Gives back an empty string if it cannot.
    ''' </summary>
    Public Function HoldForOpening() As String
        Try
            SyncLock writeLock
                If Not File.Exists(RecoveryFile) Then Return ""
                Dim held As String = Path.Combine(RecoveryFolder, "opening.e2s")
                File.Copy(RecoveryFile, held, True)
                Return held
            End SyncLock
        Catch ex As Exception
            LogException(ex, "holding the recovery copy")
            Return ""
        End Try
    End Function

    ''' <summary>Takes away the copy that HoldForOpening made.</summary>
    Public Sub ReleaseHold()
        Try
            Dim held As String = Path.Combine(RecoveryFolder, "opening.e2s")
            If File.Exists(held) Then File.Delete(held)
        Catch ex As Exception
            LogException(ex, "releasing the recovery copy")
        End Try
    End Sub

    ''' <summary>The recovery copy that waits, or Nothing.</summary>
    Public Function Pending() As RecoveryInfo
        Try
            If Not File.Exists(RecoveryFile) Then Return Nothing

            Dim info As New RecoveryInfo With {.Written = File.GetLastWriteTime(RecoveryFile)}
            If File.Exists(RecoveryInfoFile) Then
                For Each line As String In File.ReadAllLines(RecoveryInfoFile)
                    Dim parts() As String = line.Split(New Char() {":"c}, 2)
                    If parts.Length < 2 Then Continue For
                    Select Case parts(0).Trim()
                        Case "Path" : info.ProjectPath = parts(1).Trim()
                        Case "Written"
                            Dim written As Date
                            If Date.TryParse(parts(1).Trim(), written) Then info.Written = written
                    End Select
                Next
            End If
            Return info
        Catch ex As Exception
            LogException(ex, "reading the recovery copy")
            Return Nothing
        End Try
    End Function
End Module
