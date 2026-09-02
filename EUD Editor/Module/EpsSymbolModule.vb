Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

''' <summary>
''' What a call in epScript takes, so a line of it can be drawn as fields.
'''
''' Two places say so, and both are needed.
'''
''' The tables in Data/TriggerEditor describe the classic trigger set, the nodes
''' the old editor draws, and they say which list fills each value.
'''
''' eudplib describes everything else, in its own type notes. It ships compiled
''' for a Python the machine may not have, so it cannot be read from outside;
''' euddraft carries the eudplib the user builds with and will run a plugin, so
''' the answer is read from there and kept in a file beside the tables. See
''' development/spike/eudplib_symbols.py, which writes that file.
''' </summary>
Namespace EpsSource

    ''' <summary>One value a call takes.</summary>
    Public Class EpsValue
        Public Property Name As String = ""
        ''' <summary>The list that fills it, in the editor's own words, or "".</summary>
        Public Property Kind As String = ""
        Public Property Note As String = ""

        Public ReadOnly Property HasList As Boolean
            Get
                Return Kind <> ""
            End Get
        End Property
    End Class

    ''' <summary>One call the editor knows how to draw.</summary>
    Public Class EpsCall
        Public Property Name As String = ""
        Public ReadOnly Values As New List(Of EpsValue)
        Public Property Note As String = ""
        ''' <summary>
        ''' What this call does, said as a sentence, with a mark where each value
        ''' goes: "Modify death counts for $Player$: $Modifier$ $Number$ for $Unit$."
        ''' The editor's own tables carry these; eudplib does not.
        ''' </summary>
        Public Property Sentence As String = ""
        ''' <summary>Where it was read from, for the user to see.</summary>
        Public Property Source As String = ""
    End Class

    Public Module EpsSymbols

        Private ReadOnly calls_ As New Dictionary(Of String, EpsCall)(StringComparer.OrdinalIgnoreCase)
        Private loaded As Boolean

        ''' <summary>What eudplib calls a value, against the list the editor fills.</summary>
        Private ReadOnly DrawnAs As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
            {"TrgUnit", "Unit"}, {"DefaultUnit", "Unit"}, {"Unit", "Unit"},
            {"TrgPlayer", "Player"}, {"_Player", "Player"}, {"Player", "Player"},
            {"Location", "Location"},
            {"TrgComparison", "Comparison"}, {"Comparison", "Comparison"},
            {"TrgModifier", "Modifier"}, {"Modifier", "Modifier"},
            {"TrgResource", "ResourceType"}, {"Resource", "ResourceType"},
            {"TrgScore", "ScoreType"}, {"_Score", "ScoreType"},
            {"TrgSwitchAction", "SwitchAction"}, {"SwitchAction", "SwitchAction"},
            {"TrgSwitchState", "SwitchState"}, {"SwitchState", "SwitchState"},
            {"_Switch", "Switch"}, {"Switch", "Switch"},
            {"TrgAllyStatus", "AllyStatus"}, {"AllyStatus", "AllyStatus"},
            {"TrgOrder", "Order"}, {"_Order", "Order"},
            {"UnitOrder", "UnitOrder"}, {"DefaultUnitOrder", "UnitOrder"}, {"_UnitOrder", "UnitOrder"},
            {"UnitProperty", "Property"},
            {"String", "Text"}, {"StringIdMap", "Text"},
            {"AIScriptWithoutLocation", "AIScript"}, {"DefaultAIScriptWithoutLocation", "AIScript"}
        }

        'Names that say how a value is held, not what it means.
        Private ReadOnly Plumbing As New HashSet(Of String)(StringComparer.Ordinal) From {
            "ExprProxy", "EUDVariable", "ConstExpr", "ConstType", "Byte", "Word", "Dword",
            "Literal", "Optional", "Union", "Iterable", "Sequence", "Any", "Self", "T_co"
        }

        Public ReadOnly Property Count As Integer
            Get
                Return calls_.Count
            End Get
        End Property

        Public Function Find(name As String) As EpsCall
            EnsureLoaded()
            Dim found As EpsCall = Nothing
            If name IsNot Nothing AndAlso calls_.TryGetValue(name, found) Then Return found
            Return Nothing
        End Function

        Public Function Names() As List(Of String)
            EnsureLoaded()
            Dim out As New List(Of String)(calls_.Keys)
            out.Sort(StringComparer.OrdinalIgnoreCase)
            Return out
        End Function

        Public Sub Reload()
            loaded = False
            calls_.Clear()
            EnsureLoaded()
        End Sub

        Private Sub EnsureLoaded()
            If loaded Then Return
            loaded = True
            Try
                Dim folder As String = Path.Combine(My.Application.Info.DirectoryPath, "Data", "TriggerEditor")
                ReadEditorTable(Path.Combine(folder, "action.json"))
                ReadEditorTable(Path.Combine(folder, "condition.json"))
                ReadEudplib(Path.Combine(folder, "eudplib_signatures.json"))
            Catch ex As Exception
                LogException(ex, "reading the epScript symbols")
            End Try
        End Sub

        ''' <summary>
        ''' The classic trigger set. Each entry carries the epScript it stands for,
        ''' with a mark where each value goes, and the list that fills it.
        ''' </summary>
        Private Sub ReadEditorTable(path As String)
            If Not File.Exists(path) Then Return

            For Each entry As Dictionary(Of String, Object) In ReadJsonArray(path)
                Dim template As String = TextOf(entry, "CodeText")
                If template = "" Then Continue For

                Dim head As Match = Regex.Match(template.Trim(), "^([A-Za-z_]\w*)\s*\(")
                If Not head.Success Then Continue For        'a piece of epScript, not a call

                Dim made As New EpsCall With {.Name = head.Groups(1).Value, .Source = "editor table",
                                              .Sentence = SentenceOf(entry)}
                Dim kinds As List(Of String) = ListOf(entry, "ValuesDef")
                Dim i As Integer = 0
                For Each mark As Match In Regex.Matches(template, "\$(\w+)\$")
                    Dim kind As String = If(i < kinds.Count, kinds(i), mark.Groups(1).Value)
                    If kind = "None" Then kind = ""
                    made.Values.Add(New EpsValue With {.Name = mark.Groups(1).Value, .Kind = kind})
                    i += 1
                Next
                calls_(made.Name) = made
            Next
        End Sub

        'The description in the language the editor is set to, or the English one.
        Private Function SentenceOf(entry As Dictionary(Of String, Object)) As String
            Dim texts As List(Of String) = ListOf(entry, "Texts")
            Dim english As String = ""
            For i = 0 To texts.Count - 2 Step 2
                If String.Equals(texts(i), My.Settings.Language, StringComparison.OrdinalIgnoreCase) Then
                    Return texts(i + 1)
                End If
                If String.Equals(texts(i), "English", StringComparison.OrdinalIgnoreCase) Then
                    english = texts(i + 1)
                End If
            Next
            Return english
        End Function

        ''' <summary>
        ''' What euddraft said its eudplib offers. Only calls the tables do not
        ''' already describe are taken, because a table entry knows the lists this
        ''' editor fills and a type note only knows the language.
        ''' </summary>
        Private Sub ReadEudplib(path As String)
            If Not File.Exists(path) Then Return

            For Each pair As KeyValuePair(Of String, JToken) In ReadJsonObject(path)
                If calls_.ContainsKey(pair.Key) Then Continue For

                Dim entry As JObject = TryCast(pair.Value, JObject)
                If entry Is Nothing Then Continue For

                Dim values As JArray = TryCast(entry("params"), JArray)
                If values Is Nothing Then Continue For

                Dim made As New EpsCall With {.Name = pair.Key, .Source = "eudplib",
                                              .Note = If(entry("doc") Is Nothing, "", entry("doc").ToString())}
                For Each item As JToken In values
                    Dim value As JObject = TryCast(item, JObject)
                    If value Is Nothing Then Continue For
                    Dim name As String = If(value("name") Is Nothing, "", value("name").ToString())
                    If name = "self" OrElse name.StartsWith("*") Then Continue For
                    made.Values.Add(New EpsValue With {
                        .Name = name,
                        .Kind = KindOfAnnotation(If(value("annotation") Is Nothing, "", value("annotation").ToString())),
                        .Note = If(value("default") Is Nothing, "", value("default").ToString())})
                Next
                calls_(made.Name) = made
            Next
        End Sub

        ''' <summary>The list that fills a value, from what eudplib calls its type.</summary>
        Public Function KindOfAnnotation(annotation As String) As String
            If String.IsNullOrEmpty(annotation) Then Return ""

            'What is in quotes is a value, not a type.
            Dim clean As String = Regex.Replace(annotation, "'[^']*'", " ")
            clean = Regex.Replace(clean, """[^""]*""", " ")

            For Each piece As Match In Regex.Matches(clean, "[A-Za-z_][\w.]*")
                Dim leaf As String = piece.Value.Split("."c).Last()
                If Plumbing.Contains(leaf) Then Continue For
                Dim drawn As String = Nothing
                If DrawnAs.TryGetValue(leaf, drawn) Then Return drawn
            Next
            Return ""
        End Function

#Region "Reading the tables"
        ' Newtonsoft is already a part of this program, so it does the reading.

        Private Function ReadJsonArray(path As String) As List(Of Dictionary(Of String, Object))
            Dim out As New List(Of Dictionary(Of String, Object))
            Dim items As JArray = JArray.Parse(File.ReadAllText(path))
            For Each item As JToken In items
                Dim entry As JObject = TryCast(item, JObject)
                If entry Is Nothing Then Continue For
                out.Add(entry.ToObject(Of Dictionary(Of String, Object))())
            Next
            Return out
        End Function

        Private Function ReadJsonObject(path As String) As JObject
            Return JObject.Parse(File.ReadAllText(path))
        End Function

        Private Function TextOf(entry As Dictionary(Of String, Object), key As String) As String
            Dim got As Object = Nothing
            If entry IsNot Nothing AndAlso entry.TryGetValue(key, got) AndAlso got IsNot Nothing Then
                Return Convert.ToString(got)
            End If
            Return ""
        End Function

        Private Function ListOf(entry As Dictionary(Of String, Object), key As String) As List(Of String)
            Dim out As New List(Of String)
            Dim got As Object = Nothing
            If entry Is Nothing OrElse Not entry.TryGetValue(key, got) Then Return out
            Dim items As JArray = TryCast(got, JArray)
            If items Is Nothing Then Return out
            For Each item As JToken In items
                out.Add(item.ToString())
            Next
            Return out
        End Function
#End Region
    End Module
End Namespace
