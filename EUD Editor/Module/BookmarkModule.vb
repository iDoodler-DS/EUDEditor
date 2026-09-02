''' <summary>
''' Marks entries the user works on, so a return to them costs one click. A bookmark
''' belongs to the project, because the entries it names belong to the project, so it
''' is written in the project file and goes with it.
'''
''' A bookmark holds the kind of data and the number of the entry, not its name. A name
''' can change; the entry it names does not.
''' </summary>
Module BookmarkModule

    Public Class Bookmark
        Public Property DataType As Integer
        Public Property Index As Integer

        Public Sub New()
        End Sub

        Public Sub New(dataType As Integer, index As Integer)
            Me.DataType = dataType
            Me.Index = index
        End Sub

        Public Function SameAs(other As Bookmark) As Boolean
            Return other IsNot Nothing AndAlso other.DataType = DataType AndAlso other.Index = Index
        End Function
    End Class

    Public ReadOnly Bookmarks As New List(Of Bookmark)

    ''' <summary>Raised when the list changes, so the menu can be built again.</summary>
    Public Event Changed()

    Public Function IsBookmarked(dataType As Integer, index As Integer) As Boolean
        Return Find(dataType, index) IsNot Nothing
    End Function

    Public Function Find(dataType As Integer, index As Integer) As Bookmark
        For Each mark As Bookmark In Bookmarks
            If mark.DataType = dataType AndAlso mark.Index = index Then Return mark
        Next
        Return Nothing
    End Function

    ''' <summary>Adds the entry, or takes it away when it is there. Gives back the new state.</summary>
    Public Function Toggle(dataType As Integer, index As Integer) As Boolean
        Dim mark As Bookmark = Find(dataType, index)
        If mark Is Nothing Then
            Bookmarks.Add(New Bookmark(dataType, index))
            MarkProjectChanged()
            RaiseEvent Changed()
            Return True
        End If

        Bookmarks.Remove(mark)
        MarkProjectChanged()
        RaiseEvent Changed()
        Return False
    End Function

    Public Sub RemoveAll()
        If Bookmarks.Count = 0 Then Return
        Bookmarks.Clear()
        MarkProjectChanged()
        RaiseEvent Changed()
    End Sub

    ''' <summary>Empties the list without marking the project. For a load or a close.</summary>
    Public Sub Reset()
        Bookmarks.Clear()
        RaiseEvent Changed()
    End Sub

    Private Sub MarkProjectChanged()
        If ProjectSet.isload Then ProjectSet.saveStatus = False
    End Sub

    ''' <summary>The name of the entry a bookmark names, for a menu.</summary>
    Public Function LabelOf(mark As Bookmark) As String
        Dim listType As Integer = If(mark.DataType = DatEditForm.BUTTON_TAB, DTYPE.btnunit, mark.DataType)
        Dim name As String = ""
        Try
            If listType >= 0 AndAlso listType < CODE.Count AndAlso
               mark.Index >= 0 AndAlso mark.Index < CODE(listType).Count Then
                name = CODE(listType)(mark.Index)
            End If
        Catch ex As Exception
            LogSuppressed(ex, "BookmarkModule.LabelOf")
        End Try

        If name = "" Then name = "#" & mark.Index
        Return KindName(mark.DataType) & " " & name
    End Function

    Private Function KindName(dataType As Integer) As String
        If dataType = DatEditForm.BUTTON_TAB Then Return "Button"
        Select Case dataType
            Case DTYPE.units : Return "Unit"
            Case DTYPE.weapons : Return "Weapon"
            Case DTYPE.flingy : Return "Flingy"
            Case DTYPE.sprites : Return "Sprite"
            Case DTYPE.images : Return "Image"
            Case DTYPE.upgrades : Return "Upgrade"
            Case DTYPE.techdata : Return "Tech"
            Case DTYPE.orders : Return "Order"
            Case DTYPE.sfxdata : Return "Sound"
            Case Else : Return "Entry"
        End Select
    End Function

    ''' <summary>The lines of the project file. Empty when there is nothing to write.</summary>
    Public Function ToProjectText() As String
        If Bookmarks.Count = 0 Then Return ""

        Dim text As New Text.StringBuilder()
        text.Append("S_BookmarkSET" & vbCrLf)
        For Each mark As Bookmark In Bookmarks
            text.Append("Bookmark : " & mark.DataType & "," & mark.Index & vbCrLf)
        Next
        text.Append("E_BookmarkSET" & vbCrLf)
        Return text.ToString()
    End Function

    ''' <summary>Reads the section of the project file. An older file has none.</summary>
    Public Sub FromProjectText(section As String)
        Bookmarks.Clear()
        If String.IsNullOrWhiteSpace(section) Then
            RaiseEvent Changed()
            Return
        End If

        For Each line As String In section.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            Dim body As String = line.Trim()
            If Not body.StartsWith("Bookmark") Then Continue For

            Dim parts() As String = body.Substring(body.IndexOf(":"c) + 1).Split(","c)
            Dim dataType, index As Integer
            If parts.Length = 2 AndAlso Integer.TryParse(parts(0).Trim(), dataType) AndAlso
               Integer.TryParse(parts(1).Trim(), index) Then
                If Find(dataType, index) Is Nothing Then Bookmarks.Add(New Bookmark(dataType, index))
            End If
        Next
        RaiseEvent Changed()
    End Sub
End Module
