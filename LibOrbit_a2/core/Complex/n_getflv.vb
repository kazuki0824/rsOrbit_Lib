Imports System.Net
Public Module n_getflv
#Region "internal"
    Public Enum authflagkind
        fail = 0
        Authenticated = 1
        Authenticated_As_Premium = 3
    End Enum
    Property x_niconico_authflag As Integer

    Private Function setWatchTicket(ByVal watchId As String, ByRef thumbplaykey As String, ByRef response As String, ByRef p As String) As Net.Cookie
        Dim req As HttpWebRequest = CType(WebRequest.Create("http://ext.nicovideo.jp/thumb_watch/" & watchId), HttpWebRequest)
        Dim res As HttpWebResponse

        req.Referer = "http://www.kazukikuroda.co.cc/"
        Dim ctor As New CookieContainer
        req.CookieContainer = ctor
        res = CType(req.GetResponse(), HttpWebResponse)
        Dim responsedData As String
        Using strm As New IO.StreamReader(res.GetResponseStream)
            responsedData = strm.ReadToEnd
            p = responsedData
        End Using
        'Regexオブジェクトを作成 
        Dim r As New System.Text.RegularExpressions.Regex( _
            "(?<=\'thumbPlayKey\'\: )\'.+?\'", _
            System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        Dim mc As System.Text.RegularExpressions.MatchCollection = r.Matches(responsedData)
        thumbplaykey = mc.Item(0).Value.Replace("'", "")
        r = New System.Text.RegularExpressions.Regex("(?<=Nicovideo\.playerUrl = )\'.+?\'")
        mc = r.Matches(responsedData)
        Dim referer As String = mc.Item(0).Value.Replace("'", "")

        req = CType(WebRequest.Create("http://ext.nicovideo.jp/thumb_watch"), HttpWebRequest)
        req.CookieContainer = ctor
        req.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "ja,en-US;q=0.8,en;q=0.6")
        req.ContentType = "application/x-www-form-urlencoded"
        req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.79 Safari/537.1"
        req.Method = "post"
        req.KeepAlive = True
        req.Referer = referer
        With New IO.StreamWriter(req.GetRequestStream)
            Dim post As String = Uri.EscapeUriString(String.Format("as3=1&k={0}&v={1}", thumbplaykey, watchId))
            .Write(post)
            .Close()
        End With
        res = CType(req.GetResponse(), HttpWebResponse)
        Using strm As New IO.StreamReader(res.GetResponseStream)
            response = strm.ReadToEnd
        End Using

        Return res.Cookies("nicohistory")
    End Function
    Function getflv(ByVal watchId As String, ByRef nicohistory As Net.Cookie, ByRef player As String) As String
        Dim thumbplaykey, resbody As String
        nicohistory = n_getflv.setWatchTicket(watchId, thumbplaykey, resbody, player)
        Return resbody
    End Function
    Function getflvParse(ByVal arg As String) As Dictionary(Of String, String)

        Dim d As New Dictionary(Of String, String)
        For Each x As String In arg.Split("&"c)
            Dim kv() As String
            kv = x.Split("="c)
            d.Add(kv(0), Uri.UnescapeDataString(kv(1)))
            Erase kv
        Next
        Return d
    End Function
    Function getthumbinfo(watchId As String) As Xml.XmlDocument
        Dim d As New Xml.XmlDocument()
        d.Load(String.Format("http://ext.nicovideo.jp/api/getthumbinfo/{0}", watchId))
        Return d
    End Function
#End Region

End Module
