Imports System.Runtime.InteropServices
Imports SharpDX
Imports SharpDX.Multimedia
Imports SharpDX.XAudio2

Module ParrotSoundManager

    Public XAudio2Context As XAudio2 = Nothing
    Public XAMasteringVoice As MasteringVoice = Nothing
    'Public XASourceVoice As SourceVoice = Nothing
    Public SourceVoiceChannel As New List(Of SourceVoice)
    Public SourceVoiceStatus As New List(Of Integer)
    Public StatusMutex As New Object

    Public NoteBuffer As New Dictionary(Of Integer, SharpDX.DataStream)

    Public Sub InitializeXAudio2()
        XAudio2Context = New XAudio2()
        XAMasteringVoice = New MasteringVoice(XAudio2Context, 2, 44100)
        Dim wformat As New WaveFormat(44100, 16, 2)  ' int16, 44100HZ, 2chan
        'XASourceVoice = New SourceVoice(XAudio2Context, wformat)
        For i = 0 To 36 - 1
            Dim chan_id As Integer = i
            Dim sv As New SourceVoice(XAudio2Context, wformat, True)
            AddHandler sv.BufferEnd, Sub()
                                         SyncLock StatusMutex
                                             SourceVoiceStatus(chan_id) = 0
                                         End SyncLock
                                     End Sub
            SourceVoiceChannel.Add(sv)
            SyncLock StatusMutex
                SourceVoiceStatus.Add(0)
            End SyncLock
        Next

        For i = 10 To 70
            GenerateNoteBuffer(i)
        Next

    End Sub

    Public Sub PlayNote(pitch As Integer)
        Dim data_buffer As AudioBuffer = New AudioBuffer()
        With data_buffer
            .AudioBytes = 44100 * 4 * 1
            .AudioDataPointer = NoteBuffer(pitch).DataPointer
            .Context = Nothing
            .Flags = BufferFlags.EndOfStream
            .LoopCount = 0
            .LoopBegin = 0
            .LoopCount = 0
            .PlayBegin = 0
            .PlayLength = 0
        End With

        Dim chan_id As Integer = AllocateChannel()
        If chan_id >= 0 Then
            Dim sv As SourceVoice = SourceVoiceChannel(chan_id)
            sv.SubmitSourceBuffer(data_buffer, Nothing)
            sv.Start(0)
        End If

    End Sub

    Public Sub DisposeXAudio2()
        For Each pitch In NoteBuffer.Values
            pitch.Dispose()
        Next
        NoteBuffer.Clear()

        For Each sv In SourceVoiceChannel
            sv.Dispose()
        Next
        SourceVoiceChannel.Clear()
        SourceVoiceStatus.Clear()
        'XASourceVoice.Dispose()

        XAMasteringVoice.Dispose()

        XAudio2Context.Dispose()

    End Sub

    Public Sub GenerateNoteBuffer(pitch As Integer)
        Dim freq As Single = 440.0 * (2.0 ^ ((pitch - 49) / 12.0))

        Dim dstream As New SharpDX.DataStream(44100 * 4 * 1, True, True)
        dstream.Position = 0
        For i = 0 To 44100 - 1
            Dim t As Single = i / 44100.0
            Dim val As Single = Math.Sin(Math.PI * 2.0 * freq * t)

            dstream.Write(CShort(val * 32767))
            dstream.Write(CShort(val * 32767))
        Next

        NoteBuffer.Add(pitch, dstream)
    End Sub

    Public Function AllocateChannel() As Integer
        SyncLock StatusMutex
            For i = 0 To SourceVoiceStatus.Count - 1
                Dim status As Integer = SourceVoiceStatus(i)
                If status = 0 Then
                    SourceVoiceStatus(i) = 1
                    Return i
                End If
            Next
        End SyncLock
        Return -1
    End Function

    Public Sub DeallocateChannel(channel_id As Integer)
        SyncLock StatusMutex
            SourceVoiceStatus(channel_id) = 0
        End SyncLock
    End Sub


End Module
