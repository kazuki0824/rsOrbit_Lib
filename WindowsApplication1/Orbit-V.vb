Imports System.Net
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports System.IO

Module OrbitV
#Region "ReflectionBlock"
    Public Function Getniconico(url As String, Optional ByRef videoinfo As Dictionary(Of String, String) = Nothing) As Dictionary(Of Integer, Tuple(Of String, Uri, String))
        Dim u As New Uri(url)
        Dim nchs As Net.Cookie
        Dim player_raw As String = ""
        Dim watchId As String = u.AbsolutePath.Replace("/watch/", "")
        Dim getflv_param As String = n_getflv.getflv(watchId, nchs, player_raw)
        videoinfo = n_getflv.getflvParse(getflv_param)
        Dim downloadUri As String = videoinfo("url")
        Dim r As New Regex("(?<=movieType\: )\'.+?\'", System.Text.RegularExpressions.RegexOptions.ECMAScript Or System.Text.RegularExpressions.RegexOptions.Compiled)
        Dim ext As String = r.Matches(player_raw)(0).Value.Replace("'", "")
        Dim returnvalue As New Dictionary(Of Integer, Tuple(Of String, Uri, String)) From {{0, New Tuple(Of String, Uri, String)("*." & ext, New Uri(downloadUri), nchs.ToString)}}
        getflv_param = Nothing
        Return returnvalue
    End Function
    Public Function GetYoutube(ByVal url As String) As Dictionary(Of Integer, Tuple(Of String, Uri, String))
        Dim yt_ck As String = ""
        Dim yt_title As String = ""
        Dim dic As SortedDictionary(Of Integer, String) = yt(url)
        yt_ck = dic(-1) : dic.Remove(-1)
        yt_title = dic(-2) : dic.Remove(-2)

        Dim newdict As New Dictionary(Of Integer, Tuple(Of String, Uri, String))
        For Each t In dic
            newdict.Add(t.Key, New Tuple(Of String, Uri, String)(yt_title & ".flv", New Uri(t.Value), yt_ck))
        Next
        Return newdict
    End Function
    Private Const _dmCountLimit As Int32 = 300
    Private Const _dmUriPattern As String = ""
    <FiddlerLogAccess>
    Public Function Dailymotion(ByVal url As String) As Dictionary(Of Integer, Tuple(Of String, Uri, String))
        'Dim fObj As IEnumerable(Of Fiddler.Session)
        'With myOrbit.getFiddlerLog()
        '    fObj = .Reverse().
        '        TakeWhile(Function(item, i) i < _dmCountLimit AndAlso item.fullUrl Like _dmUriPattern).
        '        Reverse
        'End With
        Dim m As String
        Dim req As HttpWebRequest = WebRequest.CreateHttp(url.Replace("/video/", "/embed/video/"))
        With req.GetResponse()
            Using r As New StreamReader(.GetResponseStream)
                m = r.ReadToEnd
            End Using
            .Close()
        End With
        m = m.Substring(m.IndexOf("var info = ") + "var info = ".Length)
        Debug.Print(m)
        m = m.Remove(m.IndexOf("," & vbLf))
        Dim j As Linq.JObject = Linq.JObject.Parse(m)
        qList.Reverse()
        Dim rList As New List(Of Tuple(Of String, Uri, String))
        qList.AsParallel.AsOrdered.ForAll(Sub(s)
                                              Dim realUrl As String = j(s).ToString
                                              If String.IsNullOrEmpty(realUrl) Then Exit Sub
                                              Dim savename As String = realUrl.Remove(realUrl.IndexOf("?"c))
                                              savename = savename.Substring(savename.LastIndexOf("/"c) + 1)
                                              rList.Add(tuple.Create(Of String, Uri, String)(savename, New Uri(realUrl, UriKind.Absolute), ""))
                                          End Sub)
        Dim d As New SortedDictionary(Of Integer, String)
        Dim ind As Integer = -1
        Return rList.ToDictionary(Of Integer)(Function(item)
                                                  ind += 1
                                                  Return ind + 1
                                              End Function)

    End Function
    ReadOnly qList As New List(Of String) From {"stream_h264_hd1080_url", "stream_h264_hq_url", "stream_h264_hd_url", "stream_h264_url", "stream_h264_ld_url"}
#End Region
    Class FiddlerLogAccessAttribute
        Inherits Attribute
    End Class
End Module
