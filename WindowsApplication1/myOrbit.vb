 Imports System.Net
Imports Fiddler
Imports System.Reactive.Linq
Imports System.Reflection
Imports AsynchronousExtensions

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
            s.utilDecodeResponse()
            d.Label1.Text = "Writing to a file..."
         Await Task.Run(Sub() IO.File.WriteAllBytes(dir.FullName & "\" & sugfn,s.responseBodyBytes))
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
                                                                            Using w As New IO.FileStream(filenamewithExt, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
                                                                                Await w.WriteAsync(t.ToArray, 0, t.Count)
                                                                            End Using
                                                                            System.Diagnostics.Process.Start( _
                                                                        "EXPLORER.EXE", "/select," + dir.FullName & "\" & filenamewithExt)
                                                                            d.BeginInvoke(Sub() d.Close())
                                                                        End Sub) _
                                                                    )
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
          BindingFlags.InvokeMethod Or BindingFlags.OptionalParamBinding, _
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
