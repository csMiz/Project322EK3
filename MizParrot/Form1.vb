Imports System.ComponentModel


Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeXAudio2()
        InitDirectInput()
        AddHandler OrchidGameInput.OrchidGameKeyboardDown, AddressOf DefaultKeyboardDownEvent
        AddHandler OrchidGameInput.OrchidGameKeyboardUp, AddressOf DefaultKeyboardUpEvent


        Form1_Resize(Nothing, Nothing)
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Label1.Location = New Point(6, 6)
        Label1.Size = Me.ClientSize - New Size(12, 12)
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        StopDirectInput()
        Threading.Thread.Sleep(100)
        DisposeDirectInput()
        DisposeXAudio2()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label1.Text = "Current: " & vbCrLf & FormSharedVar.DisplayText
    End Sub
End Class

Public Module FormSharedVar

    Public DisplayText As String = ""

End Module