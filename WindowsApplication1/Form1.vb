Imports System.Collections.ObjectModel

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Me.ListView1.DoubleClick, Sub(s, arg)
                                                 myOrbit.DumpAndSave(s.selecteditems(0).Tag, New IO.DirectoryInfo(Application.StartupPath))
                                             End Sub
        InitFiddler()
    End Sub

    Private Sub FiddlerApplication_AfterSessionComplete(oSession As Fiddler.Session)
        System.Diagnostics.Debug.WriteLine(String.Format("Session {0}({3}):HTTP {1} for {2}",
                    oSession.id, oSession.responseCode, oSession.fullUrl, oSession.oResponse.MIMEType))
        Invoke(Sub() Logger.Push(String.Format("{0}:HTTP {1} for {2}", oSession.id, oSession.responseCode, oSession.fullUrl), {oSession, oSession.oRequest, oSession.oResponse}))
    End Sub

    Class Logger

        Public Shared WithEvents log As New ObservableCollection(Of Object)
        Shared Sub Push(value As String, sessionDescription As Object)
            log.Add(sessionDescription)
            Form1.ListBox1.SelectedIndex = Form1.ListBox1.Items.Add(value + vbCrLf)
            Debug.Print(value)
        End Sub
        Shared ReadOnly filterOfMIME As New List(Of String) From {"|[A-F0-9]{8}|", "video/mp4", "video/mp2t"}
        Shared ReadOnly filterOfMIME_streaming As New List(Of String) From {"|application/x-mpegURL|"}
        Shared Sub Parse(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs) Handles log.CollectionChanged
            Dim mime As String = DirectCast(e.NewItems(0)(0), Fiddler.Session).oResponse.MIMEType
            Parallel.ForEach(filterOfMIME, Sub(f)
                                               If Evaluation(mime, f, strategy:=f.StartsWith("|") AndAlso f.EndsWith("|")) Then
                                                   Form1.Invoke(Sub() RegisterMovie(DirectCast(e.NewItems(0), Fiddler.Session), Form1.WebBrowser1.Url, mime))
                                               End If
                                           End Sub)
            Parallel.ForEach(filterOfMIME_streaming, Sub(f)
                                                         If Evaluation(mime, f, strategy:=f.StartsWith("|") AndAlso f.EndsWith("|")) Then
                                                             Form1.Invoke(Sub() RegisterMovie(DirectCast(e.NewItems(0), Fiddler.Session), Form1.WebBrowser1.Url, mime, "Streaming"))
                                                         End If
                                                     End Sub)
        End Sub
        Shared Sub RegisterMovie(oSession As Fiddler.Session, WhereIsThis As Uri, Optional MIMEtype As String = "(null)", Optional type As String = "Common")
            If type <> "Common" Then
                Dim item As ListViewItem = Form1.ListView1.Items.Add(New ListViewItem({CStr(Form1.ListView1.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type, MIMEtype}) With {.ForeColor = Color.Green, .Tag = oSession})
                item.EnsureVisible()
            Else
                Dim item As ListViewItem = Form1.ListView1.Items.Add(New ListViewItem({CStr(Form1.ListView1.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type, MIMEtype}) With {.Tag = oSession})
                item.EnsureVisible()
            End If
        End Sub
        Private Shared _c As String
        Shared Property CurrentHost As String
            Get
                Return _c
            End Get
            Set(value As String)
                _c = value
                Form1.Label1.Text = "Current Host: " & _c
            End Set
        End Property
    End Class
    Private Sub InitFiddler()
        AddHandler Fiddler.FiddlerApplication.ResponseHeadersAvailable, AddressOf FiddlerApplication_AfterSessionComplete
        Fiddler.CONFIG.IgnoreServerCertErrors = True
        Fiddler.CONFIG.bStreamAudioVideo = True
        Fiddler.FiddlerApplication.Startup(0, Fiddler.FiddlerCoreStartupFlags.CaptureLocalhostTraffic Or Fiddler.FiddlerCoreStartupFlags.DecryptSSL)
        Fiddler.URLMonInterop.SetProxyInProcess(String.Format("127.0.0.1:{0}", Fiddler.FiddlerApplication.oProxy.ListenPort), "<local>")
    End Sub
    Private Sub Shutdown() Handles Me.FormClosing
        Fiddler.URLMonInterop.ResetProxyInProcessToDefault()
        Fiddler.FiddlerApplication.Shutdown()
    End Sub

    Private Sub ListBox1_DoubleClick(sender As Object, e As EventArgs) Handles ListBox1.DoubleClick
        Using s As New Dialog1(Logger.log(DirectCast(sender, ListBox).SelectedIndex))
            s.ShowDialog()
        End Using
    End Sub

    ReadOnly DefaultTabIndexDefinition As New Dictionary(Of String, Integer) From {{"www.youtube.com", 1}}
    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        If CType(sender.Url, Uri) = e.Url Then
            If Logger.CurrentHost <> e.Url.Host Then Logger.CurrentHost = e.Url.Host
            ToolStripTextBox1.Text = DirectCast(sender.Url, Uri).AbsoluteUri
            RaiseEvent LoadCompleted(sender, e)
        End If
        Dim index As Integer = -1
        If DefaultTabIndexDefinition.TryGetValue(e.Url.Host, index) Then
            Me.TabControl1.SelectedIndex = index
        End If
    End Sub
    Event LoadCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
    Private Sub SetName(sender, e) Handles Me.LoadCompleted
        Me.Text = DirectCast(sender, WebBrowser).DocumentTitle & " - RS-Orbit"
    End Sub
    Sub Parse(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles Me.LoadCompleted
        myOrbit.UsingParserMain(e.Url.AbsoluteUri)
    End Sub

    Private Sub ToolStripTextBox1_KeyPress(sender As Object, e As KeyEventArgs) Handles ToolStripTextBox1.KeyDown
        If e.KeyCode = Keys.Return AndAlso Uri.IsWellFormedUriString(sender.text, UriKind.Absolute) Then e.Handled = False : WebBrowser1.Navigate(sender.text)
    End Sub

    Private Sub ToolStripTextBox1_Leave(sender As Object, e As EventArgs) Handles ToolStripTextBox1.Leave
        sender.Text = WebBrowser1.Url.AbsoluteUri
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.ListView1.Items.Clear()
    End Sub
End Class