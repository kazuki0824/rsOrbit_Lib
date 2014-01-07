Imports System.Net
Imports Fiddler
Imports System.Reflection
Imports AsynchronousExtensions
Imports System.Reactive.Linq

Module myOrbit
    Public Async Function DumpAndSave(s As Session) As Task(Of Tuple(Of String, IO.MemoryStream))
        Dim t As String
        Dim sugfn As String = String.Empty
        Using d As New Dump
            d.Show()
            If s.fullUrl.Contains("@"c) Or s.fullUrl.Contains("?"c) Then
                t = s.fullUrl.Remove(s.fullUrl.IndexOfAny({"@"c, "?"c}))
                t = t.Substring(t.LastIndexOf("/"c) + 1)
            End If
            If [String].IsNullOrEmpty(t) Then
                sugfn = s.SuggestedFilename
            ElseIf t <> s.SuggestedFilename Then
                sugfn = t
            End If
            If sugfn Is Nothing OrElse sugfn.Contains(".txt") Then sugfn = InputBox("Filename?")
            d.Label1.Text = "Waiting for the end of stream..."
            Await Task.Run(Function()
                               Do Until s.state = SessionStates.Done
                                   If s.state = SessionStates.Aborted Then Return -1
                               Loop
                               Return 0
                           End Function)
            s.utilDecodeResponse()
        End Using
        Return tuple.Create(sugfn, New IO.MemoryStream(s.ResponseBody))
    End Function
    Function getFiddlerLog() As IEnumerable(Of Fiddler.Session)
        Return utilFiddlerCtrl.Logger.log.Select(Of Fiddler.Session)(Function(a, b) a(0))
    End Function
    ''' <summary>
    ''' Parser
    ''' </summary>
    Enum vServiceKind
        No
        Niconico
        Niconama
        Youtube
        Ustream
        Anitube
        Youku
        Pandoratv
        Dailymotion
    End Enum
    ''' <summary>
    ''' The reflection target
    ''' </summary>
    Dim reflectionTarget As New Hashtable From {{vServiceKind.Youtube, "GetYoutube"}, {vServiceKind.Niconico, "Getniconico"}, {vServiceKind.Dailymotion, "Dailymotion"}}
    Friend Function UsingParserCk(u As String) As vServiceKind
        Dim attribute As vServiceKind
        If (Evaluation(u, "http://(www\.)?youtube\.com/watch\?.*", evalStrategy.RegularExpression)) OrElse Evaluation(u, "http://youtu\.be/\w+") Then
            attribute = vServiceKind.Youtube
        ElseIf Evaluation(u, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+", evalStrategy.RegularExpression) Then
            attribute = vServiceKind.Niconico
        ElseIf Evaluation(u, "(?<=http://v.youku.com/v_show/id_)\w+(?=\.html)", evalStrategy.RegularExpression) Then
            attribute = vServiceKind.Youku
        ElseIf Evaluation(u, "http://www.dailymotion.com/*ideo/*", evalStrategy.WildCard) Then
            attribute = vServiceKind.Dailymotion
        Else
            attribute = vServiceKind.No
        End If
        Return attribute
    End Function

    ''' <summary>
    ''' Usings the parser main.
    ''' </summary>
    ''' <param name="u">動画のURL.</param>
    ''' <returns>
    ''' 0から始まるインデックス.(有効な場合は画質番号.) 順に[ファイル名],[URI],[Cookie].
    ''' </returns>
    Public Function UsingParserMain(u As String) As SortedDictionary(Of Integer, Tuple(Of String, Uri, String))
        Dim invoke As String = CStr(reflectionTarget(UsingParserCk(u)))
        If (invoke Is Nothing) Then Return Nothing
        Dim t As Type = GetType(OrbitV)
        Dim returnValue As Object = t.InvokeMember(invoke, _
            BindingFlags.InvokeMethod Or BindingFlags.OptionalParamBinding, _
            Nothing, _
            Nothing, _
            New Object() {u})
        Select Case True
            Case returnValue.GetType.Name.Contains("Dictionary")
                Return (New SortedDictionary(Of Integer, Tuple(Of String, Uri, String)) _
                     (CType(returnValue, Dictionary(Of Integer, Tuple(Of String, Uri, String)))))
        End Select
    End Function
End Module
