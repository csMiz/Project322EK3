Imports System.ComponentModel

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeXAudio2()

        Form1_Resize(Nothing, Nothing)
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Label1.Location = New Point(6, 6)
        Label1.Size = Me.ClientSize - New Size(12, 12)
    End Sub

    Private Sub Form1_Click(sender As Object, e As EventArgs) Handles Me.Click, Label1.Click
        Play()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        DisposeXAudio2()
    End Sub

End Class
