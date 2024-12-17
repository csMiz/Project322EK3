Imports System.Runtime.InteropServices
Imports SharpDX.Multimedia
Imports SharpDX.XAudio2

Module ParrotSoundManager

    Public XAudio2Context As XAudio2 = Nothing
    Public XAMasteringVoice As MasteringVoice = Nothing
    Public XASourceVoice As SourceVoice = Nothing

    Public NoteBuffer As New Dictionary(Of Integer, SharpDX.DataStream)

    Public Sub InitializeXAudio2()
        XAudio2Context = New XAudio2()
        XAMasteringVoice = New MasteringVoice(XAudio2Context, 2, 44100)
        Dim wformat As New WaveFormat(44100, 16, 2)  ' int16, 44100HZ, 2chan
        XASourceVoice = New SourceVoice(XAudio2Context, wformat)

        For i = 30 To 60
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

        XASourceVoice.SubmitSourceBuffer(data_buffer, Nothing)
        XASourceVoice.Start(0)

    End Sub

    Public Sub DisposeXAudio2()
        For Each pitch In NoteBuffer.Values
            pitch.Dispose()
        Next
        NoteBuffer.Clear()

        XAudio2Context.Dispose()
        XAMasteringVoice.Dispose()
        XASourceVoice.Dispose()
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


End Module
