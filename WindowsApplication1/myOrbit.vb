Imports System.Net
Imports Fiddler
Imports System.Reactive.Linq
Imports System.Reflection

Module myOrbit
    Public Async Sub DumpAndSave(s As Session, dir As IO.DirectoryInfo)
        Dim t As String
        Using d As New Dump
            d.Show()
            Dim sugfn As String = String.Empty
            If s.fullUrl.Contains("@"c) Or s.fullUrl.Contains("?"c) Then
                t = s.fullUrl.Remove(s.fullUrl.IndexOfAny({"@"c, "?"c}))
                t = t.Substring(t.LastIndexOf("/"c) + 1)
            End If
            If [String].IsNullOrEmpty(t) Then
                sugfn = s.SuggestedFilename
            ElseIf t <> s.SuggestedFilename Then
                sugfn = t
            End If
            If sugfn Is Nothing Then sugfn = InputBox("Filename?")
            d.Label1.Text = "Waiting for the end of stream..."
            Await Task.Run(Function()
                               Do Until s.state = SessionStates.Done
                                   If s.state = SessionStates.Aborted Then Return -1
                               Loop
                               Return 0
                           End Function)
            d.Label1.Text = "Creating a file..."
            Dim stream As IO.FileStream = IO.File.Create(dir.FullName & "\" & sugfn)
            Dim writer As New IO.StreamWriter(stream)
            d.Label1.Text = "Writing..."
            writer.Write(s.responseBodyBytes)
            Await writer.FlushAsync
            writer.Dispose()
        End Using
    End Sub
    Public Async Sub DownloadSave(req As HttpWebRequest, dir As IO.DirectoryInfo, Optional filenamewithExt As String = "")
        Dim r As HttpWebResponse = CType(Await req.GetResponseAsync, HttpWebResponse)

        'TODO UI作成

        If String.IsNullOrEmpty(filenamewithExt) Then
            filenamewithExt = DateTime.Now.ToString("yyyyMMddhhmmss") & ".flv"
        End If
        filenamewithExt = dir.FullName & "\" & filenamewithExt
        Using w As New IO.FileStream(filenamewithExt, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
            Await r.GetResponseStream.CopyToAsync(w).ContinueWith(Sub() r.Close())
        End Using
    End Sub

    Enum vServiceKind
        No
        Niconico
        Niconama
        Youtube
        Ustream
        Anitube
        Youku
        Pandoratv
    End Enum
    Dim reflectionTarget As New Hashtable From {{vServiceKind.Youtube, "GetYoutube"}, {vServiceKind.Niconico, "Getniconico"}}
    Public Function UsingParserCk(u As String) As vServiceKind
        Dim attribute As vServiceKind
        If (Evaluation(u, "http://(www\.)?youtube\.com/watch\?.*", evalStrategy.RegularExpression)) OrElse Evaluation(u, "http://youtu\.be/\w+") Then
            attribute = vServiceKind.Youtube
        ElseIf Evaluation(u, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+", evalStrategy.RegularExpression) Then
            attribute = vServiceKind.Niconico
        ElseIf Evaluation(u, "(?<=http://v.youku.com/v_show/id_)\w+(?=\.html)", evalStrategy.RegularExpression) Then
            attribute = vServiceKind.Youku
        Else
            attribute = vServiceKind.No
        End If
        Return attribute
    End Function

    Public Sub UsingParserMain(u As String)
        Dim invoke As String = reflectionTarget(UsingParserCk(u))
        If (invoke Is Nothing) Then Exit Sub
        Dim t As Type = GetType(OrbitV)
        Dim returnValue As Object = t.InvokeMember(invoke, _
          BindingFlags.InvokeMethod, _
          Nothing, _
          Nothing, _
          New Object() {u})
        Dim c As New UserControl1(returnValue) With {.Dock = DockStyle.Fill}
        With Form1.TabPage2.Controls
            .Clear()
            .Add(c)
        End With
    End Sub
End Module
