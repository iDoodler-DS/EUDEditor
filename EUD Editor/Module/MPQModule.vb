Imports System.IO

Namespace MPQlib
    Module MPQModule
        Public Function ReadListfile() As String()
            Dim list As New List(Of String)

            Dim hmpq As UInteger
            Dim hfile As UInteger
            Dim buffer(0) As Byte
            Dim filesize As UInteger
            Dim temptext As String = ""

            Dim pdwread As IntPtr

            StormLib.SFileOpenArchive(ProjectSet.InputMap, 0, 0, hmpq)


            Dim openFilename As String = "(listfile)"

            StormLib.SFileOpenFileEx(hmpq, openFilename, 0, hfile)

            If hfile <> 0 Then
                filesize = StormLib.SFileGetFileSize(hfile, filesize)
                ReDim buffer(filesize)

                StormLib.SFileReadFile(hfile, buffer, filesize, pdwread, 0)

                Dim mem As MemoryStream = New MemoryStream(buffer)
                Dim stream As StreamReader = New StreamReader(mem, System.Text.Encoding.Default)


                temptext = stream.ReadToEnd

                StormLib.SFileCloseFile(hfile)

                stream.Close()
                mem.Close()
            End If

            StormLib.SFileCloseArchive(hmpq)


            For i = 0 To temptext.Split(vbCrLf).Count - 1
                If temptext.Split(vbCrLf)(i).Trim <> "staredit\scenario.chk" Then
                    list.Add(temptext.Split(vbCrLf)(i).Trim)
                End If
            Next


            Return list.ToArray
        End Function

        'The map's (listfile), parsed once per map version. DatEdit used to open the
        'archive and re-parse it on every unit selection.
        Private cachedListfileKey As String = ""
        Private cachedListfile As String() = Nothing
        Private ReadOnly listboxListfileKeys As New Dictionary(Of ListBox, String)

        Private Function ListfileKey() As String
            Try
                Dim info As New FileInfo(ProjectSet.InputMap)
                If info.Exists Then Return info.FullName & "|" & info.LastWriteTimeUtc.Ticks & "|" & info.Length
            Catch
            End Try
            Return ProjectSet.InputMap
        End Function

        'Returns Nothing when the map has no (listfile).
        Private Function ReadListfileText() As String
            Dim hmpq As UInteger
            Dim hfile As UInteger
            Dim buffer(0) As Byte
            Dim filesize As UInteger
            Dim temptext As String = Nothing
            Dim pdwread As IntPtr

            StormLib.SFileOpenArchive(ProjectSet.InputMap, 0, 0, hmpq)
            StormLib.SFileOpenFileEx(hmpq, "(listfile)", 0, hfile)
            If hfile <> 0 Then
                filesize = StormLib.SFileGetFileSize(hfile, filesize)
                ReDim buffer(filesize)
                StormLib.SFileReadFile(hfile, buffer, filesize, pdwread, 0)
                Using mem As New MemoryStream(buffer)
                    Using stream As New StreamReader(mem, System.Text.Encoding.Default)
                        temptext = stream.ReadToEnd
                    End Using
                End Using
                StormLib.SFileCloseFile(hfile)
            End If
            StormLib.SFileCloseArchive(hmpq)
            Return temptext
        End Function

        Private Function ListfileLines() As String()
            Dim key As String = ListfileKey()
            If key <> cachedListfileKey Then
                cachedListfileKey = key
                Dim temptext As String = ReadListfileText()
                If temptext Is Nothing Then
                    cachedListfile = Nothing
                Else
                    Dim lines As New List(Of String)
                    Dim parts() As String = temptext.Split(vbCrLf)
                    For i = 0 To parts.Count - 1
                        If parts(i).Trim <> "staredit\scenario.chk" Then lines.Add(parts(i).Trim)
                    Next
                    If lines.Count > 0 Then lines.RemoveAt(lines.Count - 1)
                    cachedListfile = lines.ToArray()
                End If
            End If
            Return cachedListfile
        End Function

        Public Function ReadListfile(ByRef VListbox As ListBox) As Boolean
            Dim lines() As String = ListfileLines()
            If lines Is Nothing Then
                VListbox.Items.Clear()
                listboxListfileKeys.Remove(VListbox)
                Return False
            End If
            'Already holds this version of the list (possibly filtered by the caller).
            Dim filledFrom As String = Nothing
            If listboxListfileKeys.TryGetValue(VListbox, filledFrom) AndAlso filledFrom = cachedListfileKey Then Return True

            VListbox.BeginUpdate()
            VListbox.Items.Clear()
            VListbox.Items.AddRange(lines)
            VListbox.EndUpdate()
            listboxListfileKeys(VListbox) = cachedListfileKey
            Return True
        End Function


        Public Function ReadFile(openFilename As String, Optional MapName As String = "D") As Byte()
            Dim buffer(0) As Byte
            Dim hfile As UInteger
            Dim filesize As UInteger
            Dim pdwread As IntPtr

            Dim hmpq As UInteger

            If MapName = "D" Then
                MapName = ProjectSet.InputMap
            End If
            StormLib.SFileOpenArchive(MapName, 0, 0, hmpq)


            StormLib.SFileOpenFileEx(hmpq, openFilename, 0, hfile)

            If hfile <> 0 Then
                filesize = StormLib.SFileGetFileSize(hfile, filesize)
                ReDim buffer(filesize)

                StormLib.SFileReadFile(hfile, buffer, filesize, pdwread, 0)



                StormLib.SFileCloseFile(hfile)
            Else
                Return {0}
            End If


            StormLib.SFileCloseArchive(hmpq)
            Return buffer
        End Function


        Public Sub AddFile(Filename As String, ArchivedFilename As String, Optional MapName As String = "D")
            Dim hmpq As UInteger
            Filename = Filename.Trim

            If MapName = "D" Then
                MapName = ProjectSet.InputMap
            End If
            StormLib.SFileOpenArchive(MapName, 0, 0, hmpq)

            StormLib.SFileAddFile(hmpq, Filename, ArchivedFilename, StormLib.MPQ_FILE_REPLACEEXISTING)
            StormLib.SFileCloseArchive(hmpq)
        End Sub


        Public Sub AddFileSound(Filename As String, ArchivedFilename As String, Optional MapName As String = "D")
            Dim hmpq As UInteger
            Filename = Filename.Trim

            If MapName = "D" Then
                MapName = ProjectSet.InputMap
            End If
            StormLib.SFileOpenArchive(MapName, 0, 0, hmpq)

            StormLib.SFileAddWave(hmpq, Filename, ArchivedFilename, StormLib.MPQ_FILE_REPLACEEXISTING, StormLib.MPQ_WAVE_QUALITY_MEDIUM)
            StormLib.SFileCloseArchive(hmpq)
        End Sub

        Public Sub Rename(oldFilename As String, newFilename As String, Optional MapName As String = "D")
            Dim hmpq As UInteger
            oldFilename = oldFilename.Trim

            If MapName = "D" Then
                MapName = ProjectSet.InputMap
            End If
            StormLib.SFileOpenArchive(MapName, 0, 0, hmpq)
            StormLib.SFileRenameFile(hmpq, oldFilename, newFilename)
            StormLib.SFileCloseArchive(hmpq)
        End Sub
        Public Sub RemoveFile(Filename As String, Optional MapName As String = "D")
            Dim hmpq As UInteger
            Filename = Filename.Trim

            If MapName = "D" Then
                MapName = ProjectSet.InputMap
            End If
            StormLib.SFileOpenArchive(MapName, 0, 0, hmpq)
            StormLib.SFileRemoveFile(hmpq, Filename, 0)
            StormLib.SFileCloseArchive(hmpq)
        End Sub
    End Module

End Namespace
