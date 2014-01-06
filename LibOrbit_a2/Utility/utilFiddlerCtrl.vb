Imports System.Collections.ObjectModel

Module utilFiddlerCtrl
    Private Sub FiddlerApplication_AfterSessionComplete(oSession As Fiddler.Session)
        System.Diagnostics.Debug.WriteLine(String.Format("Session {0}({3}):HTTP {1} for {2}",
                    oSession.id, oSession.responseCode, oSession.fullUrl, oSession.oResponse.MIMEType))
        Logger.Push(String.Format("{0}:HTTP {1} for {2}", oSession.id, oSession.responseCode, oSession.fullUrl), {oSession, oSession.oRequest, oSession.oResponse})
    End Sub

    Class Logger
        Friend Shared RecognizedList As New ListView
        Protected Friend Shared WithEvents log As New ObservableCollection(Of Object)
        Shared Sub Push(value As String, sessionDescription As Object)
            log.Add(sessionDescription)
            Debug.Print(value)
        End Sub
        Shared ReadOnly filterOfMIME As New List(Of String) From {"|[A-F0-9]{8}|", "video/mp4", "video/mp2t"}
        Shared ReadOnly filterOfMIME_streaming As New List(Of String) From {"|application/x-mpegURL|"}
        Shared Sub Parse(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs) Handles log.CollectionChanged
            Dim mime As String = DirectCast(e.NewItems(0)(0), Fiddler.Session).oResponse.MIMEType
            Parallel.ForEach(filterOfMIME, Sub(f)
                                               If Evaluation(mime, f, strategy:=f.StartsWith("|") AndAlso f.EndsWith("|")) Then
                                                   recognizedList.Invoke(Sub() RegisterMovie(e.NewItems(0), DirectCast(e.NewItems(0), Fiddler.Session).fullUrl, mime))
                                               End If
                                           End Sub)
            Parallel.ForEach(filterOfMIME_streaming, Sub(f)
                                                         If Evaluation(mime, f, strategy:=f.StartsWith("|") AndAlso f.EndsWith("|")) Then
                                                             recognizedList.Invoke(Sub() RegisterMovie(e.NewItems(0), DirectCast(e.NewItems(0), Fiddler.Session).fullUrl, mime, "Streaming"))
                                                         End If
                                                     End Sub)
        End Sub
        Shared Sub RegisterMovie(oSession As Fiddler.Session, WhereIsThis As String, Optional MIMEtype As String = "(null)", Optional type As String = "Common")
            If type <> "Common" Then
                Dim item As ListViewItem = recognizedList.Items.Add(New ListViewItem({CStr(recognizedList.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type, MIMEtype}) With {.ForeColor = Drawing.Color.Green, .Tag = oSession})
                item.EnsureVisible()
            Else
                Dim item As ListViewItem = recognizedList.Items.Add(New ListViewItem({CStr(recognizedList.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type, MIMEtype}) With {.Tag = oSession})
                item.EnsureVisible()
            End If
        End Sub
    End Class
    Private Sub InitFiddler()
        AddHandler Fiddler.FiddlerApplication.ResponseHeadersAvailable, AddressOf FiddlerApplication_AfterSessionComplete
        Fiddler.CONFIG.IgnoreServerCertErrors = True
        Fiddler.CONFIG.bStreamAudioVideo = True
        Fiddler.FiddlerApplication.Startup(0, Fiddler.FiddlerCoreStartupFlags.CaptureLocalhostTraffic Or Fiddler.FiddlerCoreStartupFlags.DecryptSSL)
        Fiddler.URLMonInterop.SetProxyInProcess(String.Format("127.0.0.1:{0}", Fiddler.FiddlerApplication.oProxy.ListenPort), "<local>")
    End Sub
    Private Sub Shutdown()
        Fiddler.URLMonInterop.ResetProxyInProcessToDefault()
        Fiddler.FiddlerApplication.Shutdown()
    End Sub

End Module
