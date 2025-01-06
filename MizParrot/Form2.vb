Imports System.IO
Imports System.Numerics
Imports System.Text.RegularExpressions
Imports System.Threading
Imports SharpDX

Public Class Form2

    Public BG As Bitmap

    Public BM As Bitmap

    Public AC As New List(Of Vector2)

    Public ACSampleData As Single() = {}

    Public PhaseBuffer As Single() = {}

    Public RebuildData As Single() = {}

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BG = New Bitmap("C:\Users\asdfg\Desktop\Project322EK3\WSamples\07.png")
        BM = New Bitmap(1000, 500)

        AC.Add(New Vector2(0, 0))
        AC.Add(New Vector2(1, 0))

        DrawAnchorMode()

    End Sub

    Public Sub DrawAnchorMode()
        Using g As Graphics = Graphics.FromImage(BM)
            g.Clear(Color.White)
            g.DrawImage(BG, New Rectangle(0, 0, 1000, 500))
            Dim RECT_R As Single = 4.0
            For i = 0 To AC.Count - 1
                Dim pt As Vector2 = AC(i)
                g.FillRectangle(Brushes.Black, New Rectangle(pt.X * 1000 - RECT_R, -pt.Y * 250 + 250 - RECT_R, RECT_R * 2, RECT_R * 2))
                If i > 0 Then
                    Dim pt_last As Vector2 = AC(i - 1)
                    g.DrawLine(Pens.Black, New Point(pt.X * 1000, -pt.Y * 250 + 250), New Point(pt_last.X * 1000, -pt_last.Y * 250 + 250))
                End If
            Next

            If RebuildData.Count > 128 Then
                For i = 0 To RebuildData.Count - 1
                    Dim x As Single = i * 1.0 / RebuildData.Count
                    Dim y As Single = RebuildData(i)
                    If i > 0 Then
                        Dim x_last As Single = (i - 1) * 1.0 / RebuildData.Count
                        Dim y_last As Single = RebuildData(i - 1)
                        g.DrawLine(Pens.Blue, New Point(x * 1000, -y * 250 + 250), New Point(x_last * 1000, -y_last * 250 + 250))
                    End If
                Next
            End If
        End Using
        PictureBox1.Image = BM
        PictureBox1.Invalidate()
    End Sub

    Public Sub DrawWave()
        Dim max_v As Single = 0.0
        Dim bm As New Bitmap(1000, 500)
        Using g As Graphics = Graphics.FromImage(bm)
            Dim last_v As Single = 0.0
            For i = 1 To 999
                Dim t As Single = i / 1000.0
                Dim this_w_raw As Single = Wave(t)
                Dim this_v As Single = this_w_raw * 40.0
                g.DrawLine(Pens.Black, New Point(i - 1, Math.Clamp(-last_v + 250, 0, 499)), New Point(i, Math.Clamp(-this_v + 250, 0, 499)))
                max_v = Math.Max(max_v, this_w_raw)
                last_v = this_v
            Next
        End Using
        PictureBox1.Image = bm
        PictureBox1.Invalidate()
        Debug.WriteLine("Max: " & max_v)
    End Sub


    Public Function Wave(t As Single) As Single
        ' 三角波
        Dim res As Single = 0.0F
        Const ITER As Integer = 72
        For i = 1 To ITER Step 2
            Dim scale As Single = (1.0 / i) ^ 2
            scale *= (-1) ^ (Math.Floor(i / 2))
            res += Math.Sin(i * 2.0 * Math.PI * t) * scale
        Next
        Return res

        '' 方波
        'Dim res As Single = 0.0F
        'Const ITER As Integer = 36
        'For i = 1 To ITER Step 2
        '    Dim scale As Single = 1.0

        '    scale = 1.0 / i

        '    res += Math.Sin(i * 2.0 * Math.PI * t) * scale
        'Next
        'Return res

        '' 锯齿波
        'Dim res As Single = 0.0F
        'Const ITER As Integer = 36
        'For i = 1 To ITER
        '    Dim scale As Single = 1.0

        '    scale = 1.0 / i

        '    res += Math.Sin(i * 2.0 * Math.PI * t) * scale
        'Next
        'Return res
    End Function


    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        AC.Insert(AC.Count - 1, New Vector2(e.X / 600.0, (0.5 - e.Y / 300.0) * 2.0))
        DrawAnchorMode()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' generate samples
        ReDim ACSampleData(4410 - 1)
        For i = 0 To 4410 - 1
            Dim x As Single = i / 4410.0
            ' search left ac and right ac
            Dim ac_left_id As Integer = FindLeftAnchor(x)
            Dim ac_left As Vector2 = AC(ac_left_id)
            Dim ac_right As Vector2 = AC(ac_left_id + 1)
            ' do interpolation
            Dim progress As Single = (x - ac_left.X) / (ac_right.X - ac_left.X)
            Dim y As Single = ac_left.Y * (1.0 - progress) + ac_right.Y * progress
            ACSampleData(i) = y
        Next

    End Sub

    Public Function FindLeftAnchor(x As Single) As Integer
        For i = 0 To AC.Count - 1
            If AC(i).X <= x Then
            Else
                Return i - 1
            End If
        Next
        Return (AC.Count - 2)
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        '' test generate samples
        'Dim sp(1023) As Single
        'For i = 0 To 1023
        '    Dim x As Single = i / 1024.0
        '    sp(i) = Math.Sin(3.0 * (2.0 * Math.PI * x)) + Math.Sin(7.0 * (2.0 * Math.PI * x))
        'Next
        Dim sp = ACSampleData
        If sp.Count < 512 Then Return

        Dim freq_table As Single()
        Dim phase_table As Single()
        DFT_v3(sp, freq_table, phase_table)

        freq_table = FreqFilter(freq_table)

        Dim line = ""
        For Each f In freq_table
            line &= f.ToString & vbCrLf
        Next
        TextBox1.Text = line

        PhaseBuffer = phase_table

        If True Then
            Dim file_freq As New FileStream("C:\Users\asdfg\Desktop\Project322EK3\WSamples\dump_freq.txt", FileMode.Create)
            Using sw As New StreamWriter(file_freq)
                For Each f_val As Single In freq_table
                    sw.WriteLine(f_val.ToString)
                Next
            End Using
            file_freq.Close()
            file_freq.Dispose()
        End If
        If True Then
            Dim file_phase As New FileStream("C:\Users\asdfg\Desktop\Project322EK3\WSamples\dump_phase.txt", FileMode.Create)
            Using sw As New StreamWriter(file_phase)
                For Each p_val As Single In phase_table
                    sw.WriteLine(p_val.ToString)
                Next
            End Using
            file_phase.Close()
            file_phase.Dispose()
        End If

    End Sub

    Public Function FreqFilter(arr0 As Single()) As Single()
        Dim res(arr0.Count - 1) As Single
        For i = 0 To arr0.Count - 1
            If arr0(i) >= 0.0001 Then
                res(i) = arr0(i)
            Else
                res(i) = 0.0
            End If
        Next
        Return res
    End Function

    Public Function CalculateLoss(arr0 As Single(), arr1 As Single()) As Single
        Dim loss As Single = 0.0
        For i = 0 To arr0.Count - 1
            loss += Math.Abs(arr1(i) - arr0(i))
        Next
        Return loss
    End Function

    '    Private Double[] dft(Double[] data)
    '{
    '    int n = Data.Length;
    '    int m = n;// I use m = n / 2D;
    '    Double[] real = New Double[n];
    '    Double[] imag = New Double[n];
    '    Double[] result = New Double[m];
    '    Double pi_div = 2.0 * Math.PI / n;
    '    For (int w = 0; w < m; w++)
    '    {
    '        Double a = w * pi_div;
    '        For (int t = 0; t < n; t++)
    '        {
    '            real[w] += data[t] * Math.Cos(a * t);
    '            imag[w] += data[t] * Math.Sin(a * t);
    '        }
    '        result[w] = Math.Sqrt(real[w] * real[w] + imag[w] * imag[w]) / n;
    '    }
    '    Return result;
    '}

    Public Function DFT(data As Single()) As Single()
        Dim n As Integer = data.Count
        Dim m As Integer = n / 2
        Dim real(n) As Single
        Dim imag(n) As Single
        Dim res(m) As Single
        Dim pi_div As Double = 2.0 * Math.PI / n
        For w = 0 To m - 1
            Dim a As Double = w * pi_div
            For t = 0 To n - 1
                real(w) += data(t) * Math.Cos(a * t)
                imag(w) += data(t) * Math.Sin(a * t)
            Next
            res(w) = Math.Sqrt(real(w) * real(w) + imag(w) * imag(w)) / n
        Next
        Return res

    End Function


    Public Function DFT_v2(inputArray As Single()) As Single()
        Dim n As Integer = inputArray.Length
        Dim m As Integer = n / 2
        Dim outputArray(m - 1) As Single

        For k As Integer = 0 To m - 1
            Dim real As Single = 0
            Dim imag As Single = 0
            For t As Integer = 0 To n - 1
                Dim angle As Double = 2 * Math.PI * t * k / n
                real += inputArray(t) * CSng(Math.Cos(angle))
                imag -= inputArray(t) * CSng(Math.Sin(angle))
            Next
            outputArray(k) = CSng(Math.Sqrt(real * real + imag * imag)) / n
        Next

        Return outputArray
    End Function

    Public Sub DFT_v3(inputArray As Single(), ByRef magnitudeArray As Single(), ByRef phaseArray As Single())
        Dim n As Integer = inputArray.Length
        magnitudeArray = New Single(n - 1) {}
        phaseArray = New Single(n - 1) {}

        For k As Integer = 0 To n - 1
            Dim real As Single = 0
            Dim imag As Single = 0
            For t As Integer = 0 To n - 1
                Dim angle As Double = 2 * Math.PI * t * k / n
                real += inputArray(t) * CSng(Math.Cos(angle))
                imag -= inputArray(t) * CSng(Math.Sin(angle))
            Next
            magnitudeArray(k) = CSng(Math.Sqrt(real * real + imag * imag) / n)
            phaseArray(k) = CSng(Math.Atan2(imag, real))
        Next
    End Sub

    Public Function IDFT_v3(magnitudeArray As Single(), phaseArray As Single()) As Single()
        Dim n As Integer = magnitudeArray.Length
        Dim outputArray As Single() = New Single(n - 1) {}

        For t As Integer = 0 To n - 1
            Dim real As Single = 0
            Dim imag As Single = 0
            For k As Integer = 0 To n - 1
                Dim angle As Double = 2 * Math.PI * t * k / n
                real += magnitudeArray(k) * CSng(Math.Cos(angle + phaseArray(k)))
                imag += magnitudeArray(k) * CSng(Math.Sin(angle + phaseArray(k)))
            Next
            outputArray(t) = CSng(real)
        Next

        Return outputArray
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim freq_table(PhaseBuffer.Count - 1) As Single
        Dim txt As String = TextBox1.Text
        Dim lines As String() = Regex.Split(txt, vbCrLf)
        For i = 0 To lines.Count - 1
            freq_table(i) = CSng(lines(i))
        Next

        RebuildData = IDFT_v3(freq_table, PhaseBuffer)

        DrawAnchorMode()
    End Sub
End Class