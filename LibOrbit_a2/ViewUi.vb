Imports System.Drawing

Public Class ViewUi
    Sub New(items As SortedDictionary(Of Integer, Tuple(Of String, Uri, String)))

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        Dim i As New ImageList
        For Each t As KeyValuePair(Of Integer, Tuple(Of String, Uri, String)) In items
            ListView1.Items.Add(New ListViewItem({CStr(t.Key), t.Value.Item1, t.Value.Item2.AbsoluteUri, t.Value.Item3}) With {.Tag = t.Value})
            Dim img As Icon = Shellmgr.GetExtensionIcon(Me.Handle, "." & IO.Path.GetExtension(t.Value.Item1))
            If img Is Nothing Then
                img = Icon.FromHandle(My.Resources.Resource1._109_AllAnnotations_Help_16x16_72.GetHicon)
            End If
            i.Images.Add(img)
        Next
        Debug.Print(items.ToString)
        Me.ListView1.LargeImageList = i
        Me.ListView1.SmallImageList = i
    End Sub
    Event DownloadRequestedByUser(sender As ListViewItem, e As Tuple(Of String, Uri, String))
    Private Sub ListView1_DoubleClick(sender As Object, e As EventArgs) Handles ListView1.DoubleClick
        On Error GoTo e
        Dim tag As Tuple(Of String, Uri, String) = DirectCast(sender, ListView).SelectedItems(0).Tag
        RaiseEvent DownloadRequestedByUser(sender.SelectedItems(0), tag)
e:      Return
    End Sub
End Class
