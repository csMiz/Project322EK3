Imports SharpDX.Multimedia
Imports SharpDX.XAudio2

Module ParrotSoundManager

    Public Sub InitializeXAudio2()
        Dim xa As XAudio2 = New XAudio2()
        Dim mv As MasteringVoice = New MasteringVoice(xa)
        Dim wformat As New WaveFormat
        With wformat
            ' TODO:
        End With
        Dim sv As SourceVoice = New SourceVoice(xa, wformat)

        Dim data_buffer As AudioBuffer = New AudioBuffer()
        With data_buffer
            ' TODO:
        End With

        sv.SubmitSourceBuffer(data_buffer, Nothing)
        sv.Start(0)
    End Sub

End Module
