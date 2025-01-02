Imports MizParrot.OrchidGameInput
Imports SharpDX.DirectInput
Imports System.Numerics

Public Module OrchidGameInput

    Public DirectInputEnabled As Boolean = False

    Public D3DDirectInput As DirectInput = Nothing

    Public DInputKeyboard As Keyboard = Nothing
    Public Event OrchidGameKeyboardDown(keycode As DirectInputKeyboardCode)
    Public Event OrchidGameKeyboardUp(keycode As DirectInputKeyboardCode)



    Public Enum DirectInputKeyboardCode As UInt32
        Q = 16
        W = 17
        RightBracket = 27
        A = 30
        S = 31
        D = 32
        F = 33
        G = 34
        H = 35
        J = 36
        K = 37
        L = 38
        Semicolon = 39
        Quotation = 40
        Enter = 28
        Z = 44
        RightShift = 54
        CapsLock = 58
        LeftShift = 42
        Tab = 15
        BackSlash = 43
    End Enum


    Public Sub InitDirectInput()
        If (D3DDirectInput Is Nothing) Then
            D3DDirectInput = New DirectInput()
            InitKeyboardInput()

            DirectInputEnabled = True
            Dim listen_task As New Task(AddressOf ListenInputLoop)
            listen_task.Start()
        End If
    End Sub

    Public Sub InitKeyboardInput()
        Dim devices = D3DDirectInput.GetDevices(DeviceType.Keyboard, DeviceEnumerationFlags.AllDevices)
        If (devices.Count > 0) Then
            DInputKeyboard = New Keyboard(D3DDirectInput)
            DInputKeyboard.Properties.BufferSize = 128  ' enable buffer
            DInputKeyboard.Acquire()
        End If
    End Sub

    Public Sub StopDirectInput()
        DirectInputEnabled = False
    End Sub

    Public Sub DisposeDirectInput()
        If DirectInputEnabled Then
            Throw New Exception("DInput is still running!")
            Return
        End If
        If DInputKeyboard IsNot Nothing Then
            DInputKeyboard.Dispose()
        End If
        D3DDirectInput.Dispose()
    End Sub

    Public Sub ListenInputSingle()
        If DInputKeyboard IsNot Nothing Then
            DInputKeyboard.Poll()
            Dim input_data As KeyboardUpdate() = DInputKeyboard.GetBufferedData()
            For Each line As KeyboardUpdate In input_data
                If line.Value = 128 Then
                    ' 128 for keydown
                    RaiseEvent OrchidGameKeyboardDown(line.RawOffset)
                ElseIf line.Value = 0 Then
                    ' 0 for keyup
                    RaiseEvent OrchidGameKeyboardUp(line.RawOffset)
                End If
            Next
        End If
    End Sub

    Public Sub ListenInputLoop()
        'Dim this_time As Date = DateTime.Now
        'Dim last_time As Date = this_time
        While (DirectInputEnabled)
            Threading.Thread.Sleep(1)
            'this_time = DateTime.Now
            'If (this_time - last_time).TotalMilliseconds < 0.0 Then
            '    Continue While
            'End If
            'Debug.WriteLine("listeninput update = " & (this_time - last_time).TotalMilliseconds.ToString("0.0"))

            ListenInputSingle()

            'last_time = this_time
        End While
    End Sub

    Public Sub DefaultKeyboardDownEvent(keycode As UInt32)
        Debug.WriteLine(keycode)
        Dim chan_id As Integer = -1
        '' normal layout
        'If keycode >= DirectInputKeyboardCode.Q AndAlso keycode <= DirectInputKeyboardCode.RightBracket Then
        '    chan_id = ParrotSoundManager.PlayNote2(keycode + 36)
        'ElseIf keycode >= DirectInputKeyboardCode.A AndAlso keycode <= DirectInputKeyboardCode.Quotation Then
        '    chan_id = ParrotSoundManager.PlayNote2(keycode + 10)
        'ElseIf keycode >= DirectInputKeyboardCode.Z AndAlso keycode <= DirectInputKeyboardCode.RightShift Then
        '    chan_id = ParrotSoundManager.PlayNote2(keycode - 16)
        'ElseIf keycode = DirectInputKeyboardCode.Enter Then
        '    chan_id = ParrotSoundManager.PlayNote2(51)
        'ElseIf keycode = DirectInputKeyboardCode.CapsLock Then
        '    chan_id = ParrotSoundManager.PlayNote2(39)
        'ElseIf keycode = DirectInputKeyboardCode.LeftShift Then
        '    chan_id = ParrotSoundManager.PlayNote2(27)
        'ElseIf keycode = DirectInputKeyboardCode.Tab Then
        '    chan_id = ParrotSoundManager.PlayNote2(51)
        'End If

        ' simplified layout
        Dim LEFT_SHIFT As Integer = 0
        Dim RIGHT_SHIFT As Integer = 0
        If keycode >= DirectInputKeyboardCode.Q AndAlso keycode <= DirectInputKeyboardCode.RightBracket Then
            Dim pitch_list_left As Integer() = {28, 30, 32, 33, 35, 37}
            Dim pitch_list_right As Integer() = {39, 40, 42, 44, 45, 47}
            Dim id As Integer = keycode - DirectInputKeyboardCode.Q
            If id < 6 Then
                chan_id = ParrotSoundManager.PlayNote2(pitch_list_left(id) + LEFT_SHIFT)
            Else
                chan_id = ParrotSoundManager.PlayNote2(pitch_list_right(id - 6) + RIGHT_SHIFT)
            End If
        ElseIf keycode >= DirectInputKeyboardCode.A AndAlso keycode <= DirectInputKeyboardCode.Quotation Then
            Dim pitch_list_left As Integer() = {28, 30, 32, 33, 35}
            Dim pitch_list_right As Integer() = {37, 39, 40, 42, 44, 45}
            Dim id As Integer = keycode - DirectInputKeyboardCode.A
            If id < 5 Then
                chan_id = ParrotSoundManager.PlayNote2(pitch_list_left(id) + LEFT_SHIFT)
            Else
                chan_id = ParrotSoundManager.PlayNote2(pitch_list_right(id - 5) + RIGHT_SHIFT)
            End If
        ElseIf keycode >= DirectInputKeyboardCode.Z AndAlso keycode <= DirectInputKeyboardCode.RightShift Then
        ElseIf keycode = DirectInputKeyboardCode.Enter Then
        ElseIf keycode = DirectInputKeyboardCode.CapsLock Then
        ElseIf keycode = DirectInputKeyboardCode.LeftShift Then
        ElseIf keycode = DirectInputKeyboardCode.Tab Then
        ElseIf keycode = DirectInputKeyboardCode.BackSlash Then
        End If

        SyncLock StatusMutex
            KeyDownChannelStub(keycode) = chan_id
            If chan_id >= 0 Then
                KeyUpChannelStub(chan_id) = False
            End If
        End SyncLock

    End Sub

    Public Sub DefaultKeyboardUpEvent(keycode As UInt32)
        SyncLock StatusMutex
            Dim chan_id As Integer = KeyDownChannelStub(keycode)
            If chan_id >= 0 Then
                KeyUpChannelStub(chan_id) = True
            End If
        End SyncLock
    End Sub

End Module
