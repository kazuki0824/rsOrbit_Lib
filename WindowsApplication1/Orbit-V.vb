Module OrbitV
#Region "ReflectionBlock"
    Public Function Getniconico(url As String, Optional ByRef videoinfo As Dictionary(Of String, String) = Nothing) As SortedDictionary(Of Integer, Tuple(Of String, Uri, String))
        Dim u As New Uri(url)
        Dim nchs As Net.Cookie
        Dim player_raw As String = ""
        Dim watchId As String = u.AbsolutePath.Replace("/watch/", "")
        Dim getflv_param As String = n_getflv.getflv(watchId, nchs, player_raw)
        videoinfo = n_getflv.getflvParse(getflv_param)
        Dim downloadUri As String = videoinfo("url")
        Dim r As New System.Text.RegularExpressions.Regex("(?<=movieType\: )\'.+?\'", System.Text.RegularExpressions.RegexOptions.ECMAScript Or System.Text.RegularExpressions.RegexOptions.Compiled)
        Dim ext As String = r.Matches(player_raw)(0).Value.Replace("'", "")
        Dim returnvalue As New SortedDictionary(Of Integer, Tuple(Of String, Uri, String)) From {{0, New Tuple(Of String, Uri, String)("*." & ext, New Uri(downloadUri), nchs.ToString)}}
        getflv_param = Nothing
        Return returnvalue
    End Function
    Public Function GetYoutube(ByVal url As String)
        Dim yt_ck As String = ""
        Dim yt_title As String = ""
        Dim dic As SortedDictionary(Of Integer, String) = yt(url)
        yt_ck = dic(-1) : dic.Remove(-1)
        yt_title = dic(-2) : dic.Remove(-2)

        Dim newdict As New SortedDictionary(Of Integer, Tuple(Of String, Uri, String))
        For Each t In dic
            newdict.Add(t.Key, New Tuple(Of String, Uri, String)(yt_title & ".flv", New Uri(t.Value), yt_ck))
        Next
        Return newdict
    End Function
    Const dm_reflimit As Int32 = 300
    Public Function GetDailymotion()
        Dim dmVideoPattern As String = ""

        Dim fiddlerPattern As IEnumerable(Of Fiddler.Session) = Form1.Logger.log _
                                                                .Reverse _
                                                                .TakeWhile(Function(item As Fiddler.Session, i As Int32) As Boolean
                                                                               Return i < dm_reflimit AndAlso (item.fullUrl Like dmVideoPattern)
                                                                           End Function) _
                                                                       .Reverse

    End Function
#End Region
End Module
