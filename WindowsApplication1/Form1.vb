Imports System.Collections.ObjectModel

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'AddHandler Fiddler.FiddlerApplication.OnNotification, AddressOf FiddlerApplication_OnNotification
        'AddHandler Fiddler.FiddlerApplication.Log.OnLogString, AddressOf Log_OnLogString
        AddHandler Fiddler.FiddlerApplication.AfterSessionComplete, AddressOf FiddlerApplication_AfterSessionComplete
        Fiddler.CONFIG.IgnoreServerCertErrors = False
        AddHandler LoadCompleted, AddressOf Logger.ClearMovie
        AddHandler Me.ListView1.DoubleClick, Sub(s, arg)
                                                 myOrbit.DumpAndSave(s.selecteditems(0).Tag, New IO.DirectoryInfo(Application.StartupPath))
                                             End Sub
    End Sub

    Private Sub FiddlerApplication_AfterSessionComplete(oSession As Fiddler.Session)
        Invoke(Sub() Logger.Push(String.Format("{0}:HTTP {1} for {2}", oSession.id, oSession.responseCode, oSession.fullUrl), oSession))
    End Sub
    'Private Sub FiddlerApplication_OnNotification(sender As Object, e As Fiddler.NotificationEventArgs)
    '    Invoke(Sub() Logger.Push("** NotifyUser: " & Convert.ToString(e.NotifyString), e))
    'End Sub
    'Private Sub Log_OnLogString(sender As Object, e As Fiddler.LogEventArgs)
    '    Invoke(Sub() Logger.Push("** LogString: " & Convert.ToString(e.LogString), e))
    'End Sub

    Class Logger

        Public Shared WithEvents log As New ObservableCollection(Of Fiddler.Session)
        Shared Sub Push(value As String, sessionDescription As Object)
            log.Add(sessionDescription)
            Form1.ListBox1.SelectedIndex = Form1.ListBox1.Items.Add(value + vbCrLf)
        End Sub

        Shared filterOfMIME As New List(Of String) From {"|application/octet-stream|", "|[A-F0-9]{8}|", "video/mp4"}
        Shared Async Sub Parse(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs) Handles log.CollectionChanged
            Dim mime As String = DirectCast(e.NewItems(0), Fiddler.Session).oResponse.MIMEType
            For Each f As String In filterOfMIME
                If Await Task.Run(Function()
                                      Return Evaluation(mime, f, strategy:=f.StartsWith("|") AndAlso f.EndsWith("|"))
                                  End Function) Then Form1.Invoke(Sub() RegisterMovie(DirectCast(e.NewItems(0), Fiddler.Session), Form1.WebBrowser1.Url)) : Exit For
            Next
        End Sub
        Shared Sub RegisterMovie(oSession As Fiddler.Session, WhereIsThis As Uri, Optional type As String = "Common")
            If type <> "Common" Then
                Dim item As ListViewItem = Form1.ListView1.Items.Add(New ListViewItem({CStr(Form1.ListView1.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type}) With {.ForeColor = Color.Red, .Tag = oSession})
                item.EnsureVisible()
            Else
                Dim item As ListViewItem = Form1.ListView1.Items.Add(New ListViewItem({CStr(Form1.ListView1.Items.Count), oSession.oRequest.headers.UriScheme, oSession.oRequest.headers.RequestPath, type}) With {.Tag = oSession})
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
        Shared Sub ClearMovie()
            Form1.ListView1.Items.Clear()
        End Sub
    End Class
    Private Sub InitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InitToolStripMenuItem.Click
        BrowserProxySetting.BrowserProxySetting.RefreshIESettings("127.0.0.1:9200")
        Fiddler.FiddlerApplication.Startup(9200, Fiddler.FiddlerCoreStartupFlags.CaptureLocalhostTraffic)
    End Sub
    Private Sub Shutdown() Handles Me.FormClosing
        Fiddler.FiddlerApplication.Shutdown()
        BrowserProxySetting.BrowserProxySetting.RefreshIESettings("")
    End Sub

    Private Sub ListBox1_DoubleClick(sender As Object, e As EventArgs) Handles ListBox1.DoubleClick
        Using s As New Dialog1(Logger.log(DirectCast(sender, ListBox).SelectedIndex))
            s.ShowDialog()
        End Using
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        If sender.Url = e.Url Then
            If Logger.CurrentHost <> e.Url.Host Then Logger.CurrentHost = e.Url.Host
            ToolStripTextBox1.Text = DirectCast(sender.Url, Uri).AbsoluteUri
            RaiseEvent LoadCompleted(sender, e)
        End If
    End Sub
    Event LoadCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)

    Sub Parse(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles Me.LoadCompleted
        myOrbit.UsingParserMain(e.Url.AbsoluteUri)
    End Sub

    Private Sub ToolStripTextBox1_KeyPress(sender As Object, e As KeyEventArgs) Handles ToolStripTextBox1.KeyDown
        If e.KeyCode = Keys.Return AndAlso Uri.IsWellFormedUriString(sender.text, UriKind.Absolute) Then e.Handled = False : WebBrowser1.Navigate(sender.text)
    End Sub

    Private Sub ToolStripTextBox1_Leave(sender As Object, e As EventArgs) Handles ToolStripTextBox1.Leave
        sender.Text = WebBrowser1.Url.AbsoluteUri
    End Sub
End Class