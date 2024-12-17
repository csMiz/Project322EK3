Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load



        Form1_Resize(Nothing, Nothing)
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Label1.Location = New Point(6, 6)
        Label1.Size = Me.ClientSize - New Size(12, 12)
    End Sub

End Class
