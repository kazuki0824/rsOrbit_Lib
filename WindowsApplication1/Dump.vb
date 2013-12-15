Public Class Dump
    Dim c As String() = {"／", "―", "＼", "｜"}
    Dim num As UInteger
    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        Label2.Text = c(num Mod 4)
        num += 1
    End Sub
End Class