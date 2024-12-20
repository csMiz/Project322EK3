Imports System.ComponentModel


Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeXAudio2()
        InitDirectInput()
        AddHandler OrchidGameInput.OrchidGameKeyboardDown, AddressOf DefaultKeyboardDownEvent
        AddHandler OrchidGameInput.OrchidGameKeyboardUp, AddressOf DefaultKeyboardUpEvent

        Dim adsr_task As New Task(AddressOf ADSREnvelopeLoop)
        adsr_task.Start()

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
        Dim total_chan As Integer = 1
        Dim busy_chan As Integer = 0
        SyncLock StatusMutex
            total_chan = SourceVoiceStatus.Count
            For Each chan_status In SourceVoiceStatus
                If chan_status > 0 Then
                    busy_chan += 1
                End If
            Next
        End SyncLock

        Label1.Text = "Current: " & vbCrLf & busy_chan.ToString & " / " & total_chan.ToString
    End Sub
End Class

Public Module FormSharedVar

    Public DisplayText As String = ""

End Module