Imports SharpDX
Imports SharpDX.Multimedia
Imports SharpDX.XAudio2

Module ParrotSoundManager

    Public XAudio2Context As XAudio2 = Nothing
    Public XAMasteringVoice As MasteringVoice = Nothing
    'Public XASourceVoice As SourceVoice = Nothing
    Public SourceVoiceChannel As New List(Of SourceVoice)
    Public SourceVoiceStatus As New List(Of Integer)
    Public SourceVoicePitch As New List(Of Integer)
    Public StatusMutex As New Object

    Public NoteBuffer40 As SharpDX.DataStream

    Public Sub InitializeXAudio2()
        XAudio2Context = New XAudio2()
        XAMasteringVoice = New MasteringVoice(XAudio2Context, 2, 44100)
        Dim wformat As New WaveFormat(44100, 16, 2)  ' int16, 44100HZ, 2chan
        'XASourceVoice = New SourceVoice(XAudio2Context, wformat)
        For i = 0 To 36 - 1
            Dim chan_id As Integer = i
            Dim sv As New SourceVoice(XAudio2Context, wformat, VoiceFlags.None, 4.0, True)
            AddHandler sv.BufferEnd, Sub()
                                         DeallocateChannel(chan_id)
                                     End Sub
            SourceVoiceChannel.Add(sv)
            SyncLock StatusMutex
                SourceVoiceStatus.Add(0)
                SourceVoicePitch.Add(-1)
            End SyncLock
        Next

        GenerateNoteBuffer2()

    End Sub

    Public Sub PlayNote2(pitch As Integer)
        Dim data_buffer As AudioBuffer = New AudioBuffer()
        With data_buffer
            .AudioBytes = 44100 * 4 * 3
            .AudioDataPointer = NoteBuffer40.DataPointer
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
            SyncLock StatusMutex
                SourceVoicePitch(chan_id) = pitch
            End SyncLock
            sv.SubmitSourceBuffer(data_buffer, Nothing)
            sv.SetFrequencyRatio(2.0 ^ ((pitch - 40) / 12.0))
            sv.Start(0)
        End If

    End Sub

    Public Sub DisposeXAudio2()
        NoteBuffer40.Dispose()
        For Each sv In SourceVoiceChannel
            sv.Dispose()
        Next
        SourceVoiceChannel.Clear()
        SourceVoiceStatus.Clear()
        SourceVoicePitch.Clear()

        XAMasteringVoice.Dispose()

        XAudio2Context.Dispose()

    End Sub

    Public Sub GenerateNoteBuffer(Optional pitch As Integer = 40)
        Dim freq As Single = 440.0 * (2.0 ^ ((pitch - 49) / 12.0))

        Dim dstream As New SharpDX.DataStream(44100 * 4 * 3, True, True)
        dstream.Position = 0
        For i = 0 To 44100 * 3 - 1
            Dim t As Single = i / 44100.0
            Dim val As Single = Math.Sin(Math.PI * 2.0 * freq * t)

            dstream.Write(CShort(val * 32767))
            dstream.Write(CShort(val * 32767))
        Next

        NoteBuffer40 = dstream
    End Sub

    Public Sub GenerateNoteBuffer2(Optional pitch As Integer = 40)
        Dim freq As Single = 440.0 * (2.0 ^ ((pitch - 49) / 12.0))

        Dim fx = Function(freq_in As Single, t As Single) As Single
                     ' 三角波
                     Dim res As Single = 0.0F
                     Const ITER As Integer = 72
                     For i = 1 To ITER Step 2
                         Dim scale As Single = (1.0 / i) ^ 2
                         scale *= (-1) ^ (CInt(Math.Floor(i / 2)))
                         res += Math.Sin(i * 2.0 * Math.PI * freq * t) * scale
                     Next
                     res /= 1.23
                     Return res

                     '' 方波
                     'Dim res As Single = 0.0F
                     'Const ITER As Integer = 72
                     'For i = 1 To ITER Step 2
                     '    Dim scale As Single = 1.0 / i
                     '    res += Math.Sin(i * 2.0 * Math.PI * freq * t) * scale
                     'Next
                     'res /= 0.93
                     'Return res

                     '' 锯齿波
                     'Dim res As Single = 0.0F
                     'Const ITER As Integer = 72
                     'For i = 1 To ITER
                     '    Dim scale As Single = 1.0 / i
                     '    res += Math.Sin(i * 2.0 * Math.PI * freq * t) * scale
                     'Next
                     'res /= 1.85
                     'Return res
                 End Function

        Dim dstream As New SharpDX.DataStream(44100 * 4 * 3, True, True)
        dstream.Position = 0
        For i = 0 To 44100 * 3 - 1
            Dim t As Single = i / 44100.0
            Dim val As Single = fx(freq, t)

            dstream.Write(CShort(val * 32767))
            dstream.Write(CShort(val * 32767))
        Next

        NoteBuffer40 = dstream
    End Sub



    Public Function AllocateChannel() As Integer
        SyncLock StatusMutex
            For i = 0 To SourceVoiceStatus.Count - 1
                Dim status As Integer = SourceVoiceStatus(i)
                If status = 0 Then
                    SourceVoiceStatus(i) = 1
                    SourceVoicePitch(i) = -1
                    Return i
                End If
            Next
        End SyncLock
        Return -1
    End Function

    Public Sub DeallocateChannel(channel_id As Integer)
        SyncLock StatusMutex
            SourceVoiceChannel(channel_id).Stop()
            SourceVoiceChannel(channel_id).FlushSourceBuffers()
            SourceVoiceStatus(channel_id) = 0
            SourceVoicePitch(channel_id) = -1
        End SyncLock
    End Sub


    Public Sub ADSREnvelopeLoop()
        Dim now_time As Date
        Dim last_time As Date = DateTime.Now
        While DirectInputEnabled
            now_time = DateTime.Now
            If (now_time - last_time).TotalMilliseconds < 16.6667 Then
                Continue While
            End If

            SyncLock StatusMutex
                For i = 0 To SourceVoiceStatus.Count - 1
                    If SourceVoiceStatus(i) = 1 Then
                        Dim pitch_speed_rate As Single = 1.2 ^ ((SourceVoicePitch(i) - 40) / 12.0)
                        Dim vol As Single = 0.0
                        SourceVoiceChannel(i).GetVolume(vol)
                        vol += 0.2 * pitch_speed_rate
                        If vol >= 1.0 Then
                            SourceVoiceStatus(i) = 2
                        End If
                        SourceVoiceChannel(i).SetVolume(Math.Min(vol, 1.0))
                    ElseIf SourceVoiceStatus(i) = 2 Then
                        Dim pitch_speed_rate As Single = 1.2 ^ ((SourceVoicePitch(i) - 40) / 12.0)
                        Dim vol As Single = 0.0
                        SourceVoiceChannel(i).GetVolume(vol)
                        vol -= 0.03 * pitch_speed_rate
                        If vol <= 0.0 Then
                            vol = 0.0
                            DeallocateChannel(i)
                        End If
                        SourceVoiceChannel(i).SetVolume(vol)
                    End If
                Next
            End SyncLock

            last_time = now_time
        End While
    End Sub

End Module
