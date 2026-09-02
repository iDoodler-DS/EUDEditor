Namespace My
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            'Exceptions on threads other than the UI thread (BackgroundWorker, timers, ...)
            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainUnhandledException
        End Sub

        Private Sub MyApplication_UnhandledException(sender As Object, e As ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            LogException(e.Exception, "UI thread")

            'An error is the moment a recovery copy is worth most.
            Dim saved As Boolean = RecoveryModule.WriteCopy()

            Try
                Using dialog As New CrashForm(e.Exception, saved)
                    dialog.ShowDialog()
                End Using
            Catch ex As Exception
                LogException(ex, "showing the error window")
                MsgBox(e.Exception.GetType().Name & ": " & e.Exception.Message & vbCrLf & vbCrLf &
                       "Details were written to:" & vbCrLf & LogPath,
                       MsgBoxStyle.Critical, "EUD Editor")
            End Try

            'Keep the editor open so unsaved work is not lost.
            e.ExitApplication = False
        End Sub

        Private Sub OnDomainUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
            Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
            If ex IsNot Nothing Then
                LogException(ex, "background thread, terminating=" & e.IsTerminating)
            Else
                LogMessage("Unhandled non-exception object: " & e.ExceptionObject.ToString())
            End If
        End Sub
    End Class
End Namespace
