Public Class UserControl1
    Sub New(items As SortedDictionary(Of Integer, Tuple(Of String, Uri, String)))

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        Debug.Print(items.ToString)
        For Each t As KeyValuePair(Of Integer, Tuple(Of String, Uri, String)) In items
            ListView1.Items.Add(New ListViewItem({CStr(t.Key), t.Value.Item1, t.Value.Item2.AbsoluteUri, t.Value.Item3}) With {.Tag = t.Value})
        Next
    End Sub

    Private Sub ListView1_DoubleClick(sender As Object, e As EventArgs) Handles ListView1.DoubleClick

        Dim tag As Tuple(Of String, Uri, String) = DirectCast(sender, ListView).SelectedItems(0).Tag
        Dim src As Uri = tag.Item2
        Dim w As Net.HttpWebRequest = Net.WebRequest.CreateHttp(src)
        With w
            .AllowAutoRedirect = True
            .Headers.Add(Net.HttpRequestHeader.Cookie, tag.Item3)
        End With
        myOrbit.DownloadSave(w, New IO.DirectoryInfo(Application.StartupPath))
    End Sub
End Class
