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
        Select Case keycode
            Case DirectInputKeyboardCode.A
                FormSharedVar.DisplayText = "1"
                ParrotSoundManager.PlayNote(40)
            Case DirectInputKeyboardCode.D
                FormSharedVar.DisplayText = "2"
                ParrotSoundManager.PlayNote(42)
            Case DirectInputKeyboardCode.G
                FormSharedVar.DisplayText = "3"
                ParrotSoundManager.PlayNote(44)
            Case DirectInputKeyboardCode.H
                FormSharedVar.DisplayText = "4"
                ParrotSoundManager.PlayNote(45)
            Case DirectInputKeyboardCode.K
                FormSharedVar.DisplayText = "5"
                ParrotSoundManager.PlayNote(47)
            Case DirectInputKeyboardCode.Semicolon
                FormSharedVar.DisplayText = "6"
                ParrotSoundManager.PlayNote(49)
            Case DirectInputKeyboardCode.Enter
                FormSharedVar.DisplayText = "7"
                ParrotSoundManager.PlayNote(51)
            Case DirectInputKeyboardCode.Q
                FormSharedVar.DisplayText = "1~"
                ParrotSoundManager.PlayNote(52)

        End Select
    End Sub

    Public Sub DefaultKeyboardUpEvent(keycode As UInt32)

    End Sub

End Module
