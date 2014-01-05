Imports System.Net
Imports Fiddler
Imports System.Reflection
Imports AsynchronousExtensions
Imports System.Reactive.Linq
Imports System.Collections.ObjectModel

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
            If sugfn Is Nothing OrElse sugfn.Contains(".txt") Then sugfn = InputBox("Filename?")
            d.Label1.Text = "Waiting for the end of stream..."
            Await Task.Run(Function()
                               Do Until s.state = SessionStates.Done
                                   If s.state = SessionStates.Aborted Then Return -1
                               Loop
                               Return 0
                           End Function)
            s.utilDecodeResponse()
            d.Label1.Text = "Check existense """ + dir.FullName + """..."
            If Not dir.Exists Then MsgBox("1")
            d.Label1.Text = "Writing to a file..."
            Using streamobj As New IO.FileStream((dir.FullName & "\" & sugfn).Normalize, IO.FileMode.CreateNew, IO.FileAccess.Write, IO.FileShare.Write, 8, True)
                Await streamobj.WriteAsync(s.ResponseBody)
            End Using
            d.Label1.Text = "Launching explorer..."
            System.Diagnostics.Process.Start( _
                "EXPLORER.EXE", "/select," + dir.FullName & "\" & sugfn)
        End Using
    End Sub
    Public Sub DownloadSave(req As HttpWebRequest, dir As IO.DirectoryInfo, Optional filenamewithExt As String = "")
        Dim d As New Dump With {.Text = req.RequestUri.AbsoluteUri}
        d.Label1.Text = "Downloading..."
        d.Show() 'todo
        d.RegisterCancellationToken( _
        req.DownloadDataAsyncWithProgress().Do(Sub(p)
                                                   Dim s As String = String.Format("Downloading... {0}MB / {1}MB", Format(p.BytesReceived / 1000000, "0.000"), Format((p.TotalBytesToReceive) / 1000000, "0.000"))
                                                   d.Invoke(Sub()
                                                                d.Label1.Text = s
                                                            End Sub)
                                               End Sub).
                                      Aggregate(New List(Of Byte),
                                                Function(list, p)
                                                    list.AddRange(p.Value)
                                                    Return list
                                                End Function).Subscribe(Async Sub(t)
                                                                            If String.IsNullOrEmpty(filenamewithExt) Then
                                                                                filenamewithExt = DateTime.Now.ToString("yyyyMMddhhmmss") & ".flv"
                                                                            ElseIf filenamewithExt.StartsWith("*.") AndAlso filenamewithExt.Substring(2) Like "???" Then
                                                                                With IO.Path.GetInvalidPathChars.Aggregate(InputBox("filename?"), Function(s, c) s.Replace(c.ToString(), ""))
                                                                                    If IO.Path.HasExtension(.Normalize) Then
                                                                                        filenamewithExt = IO.Path.ChangeExtension(.Normalize, .Substring(.LastIndexOf("."c) + 1))
                                                                                    Else
                                                                                        filenamewithExt = .Normalize
                                                                                    End If
                                                                                End With
                                                                            End If
                                                                            filenamewithExt = dir.FullName & "\" & filenamewithExt
                                                                            d.Invoke(Sub()
                                                                                         d.Label1.Text = "Writing..."
                                                                                         d.ProgressBar1.Style = ProgressBarStyle.Marquee
                                                                                     End Sub)
                                                                            Await Task.Run(Sub() IO.File.WriteAllBytes(filenamewithExt, t.ToArray))
                                                                            System.Diagnostics.Process.Start( _
                                                                        "EXPLORER.EXE", "/select," + dir.FullName & "\" & filenamewithExt)
                                                                            d.BeginInvoke(Sub() d.Close())
                                                                        End Sub))
    End Sub
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
    Public Function UsingParserCk(u As String) As vServiceKind
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

    Public Function UsingParserMain(u As String)
        Dim invoke As String = CStr(reflectionTarget(UsingParserCk(u)))
        If (invoke Is Nothing) Then Return Nothing
        Dim t As Type = GetType(OrbitV)
        Dim returnValue As Object = t.InvokeMember(invoke, _
            BindingFlags.InvokeMethod Or BindingFlags.OptionalParamBinding, _
            Nothing, _
            Nothing, _
            New Object() {u})
        Static c As Control : If Not (Not c Is Nothing) AndAlso (Not c.IsDisposed) Then c = New ListView
        Select Case True
            Case returnValue.GetType.Name.Contains("Dictionary")
                c = New ViewUi _
                    (New SortedDictionary(Of Integer, Tuple(Of String, Uri, String)) _
                     (CType(returnValue, Dictionary(Of Integer, Tuple(Of String, Uri, String))))) With
                 {.Dock = DockStyle.Fill}
                returnValue = Nothing
            Case returnValue.GetType Is GetType(Control) 'for dailymotion
                c = returnValue
        End Select
    End Function
End Module
