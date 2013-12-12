Imports System.Net
Imports System.Text.RegularExpressions
Public Module kk_movDlForUser
    Function yt(ByVal targetUri As String) As SortedDictionary(Of Integer, String)
        Dim cc As New CookieContainer
        Dim req As HttpWebRequest = CType(WebRequest.Create(targetUri), HttpWebRequest)
        req.CookieContainer = cc : req.Timeout = 5000
        req.GetResponse().Close()
        req = DirectCast(WebRequest.Create("http://www.youtube.com/get_video_info?video_id=" & Regex.Match(targetUri, "(?<=v=)[\w-]+").Value), HttpWebRequest)
        req.CookieContainer = cc
        Dim _info As String
        Dim res As Net.WebResponse = req.GetResponse()
        Dim sr As New IO.StreamReader(res.GetResponseStream())
        _info = sr.ReadToEnd
        sr.Close()
        res.Close()
        Dim info As New Hashtable
        Dim _tmp As New Dictionary(Of String, String)
        Dim fmtmap As New SortedDictionary(Of Integer, String)

        For Each item As String In _info.Split("&"c)
            info.Add(item.Split("="c)(0), Uri.UnescapeDataString(item.Split("="c)(1)))
        Next
        If CStr(info("status")) = "fail" Then
            Throw New UnauthorizedAccessException
        End If
        For Each item As String In CStr(info("url_encoded_fmt_stream_map")).Split(","c)
            For Each a As String In item.Split("&"c)
                _tmp.Add(a.Split("="c)(0), Uri.UnescapeDataString(a.Split("="c)(1)))
            Next
            fmtmap.Add(CInt(_tmp("itag")), (_tmp("url")) + "&signature=" + _tmp("sig"))
            _tmp.Clear()
        Next

        req = DirectCast(WebRequest.Create("http://www.youtube.com/get_video_info?video_id=" & Regex.Match(targetUri, "(?<=v=)\w+").Value & "&t=" & CStr(info("token"))), HttpWebRequest)
        req.CookieContainer = cc
        req.Timeout = 1500
        req.GetResponse().Close()

        fmtmap(-2) = CStr(info("title"))
        fmtmap(-1) = cc.GetCookieHeader(New Uri("http://www.youtube.com"))
        info.Clear()
        Return fmtmap
    End Function
End Module