Public Class Dump
    Dim c As String() = {"／", "―", "＼", "｜"}
    Dim num As UInteger
    Function RegisterCancellationToken(ByRef obj As IDisposable) As Boolean
        On Error GoTo Err
        Me.ControlBox = True
        Me.Tag = obj
        Return True
Err:    Return False
    End Function
    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        Label2.Text = c(num Mod 4)
        num += 1
    End Sub

    Private Sub Dump_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            DirectCast(Me.Tag, IDisposable).Dispose()
        End If
    End Sub
End Class