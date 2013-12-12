Public Class UserControl1
    Sub New(items As SortedDictionary(Of Integer, Tuple(Of String, Uri, String)))

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        For Each t As KeyValuePair(Of Integer, Tuple(Of String, Uri, String)) In items
            ListView1.Items.Add(New ListViewItem({CStr(t.Key), t.Value.Item1, t.Value.Item2.AbsoluteUri, t.Value.Item3}) With {.Tag = t.Value})
        Next
    End Sub

    Private Sub ListView1_DoubleClick(sender As Object, e As EventArgs) Handles ListView1.DoubleClick
        Dim src As Uri = DirectCast(sender, ListView).SelectedItems(0).Tag.Item2
        Net.WebRequest.CreateHttp(src)
    End Sub
End Class
