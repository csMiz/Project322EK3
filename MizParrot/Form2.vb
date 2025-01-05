Imports System.IO
Imports System.Numerics
Imports System.Threading
Imports SharpDX

Public Class Form2

    Public BG As Bitmap

    Public BM As Bitmap

    Public AC As New List(Of Vector2)

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BG = New Bitmap("C:\Users\asdfg\Desktop\Project322EK3\WSamples\05.png")
        BM = New Bitmap(1000, 500)

        AC.Add(New Vector2(0, 0))
        AC.Add(New Vector2(1, 0))

        DrawAnchorMode()

    End Sub

    Public Sub DrawAnchorMode()
        Using g As Graphics = Graphics.FromImage(BM)
            g.Clear(Color.White)
            g.DrawImage(BG, New Rectangle(0, 0, 1000, 500))
            Dim RECT_R As Single = 10.0
            For i = 0 To AC.Count - 1
                Dim pt As Vector2 = AC(i)
                g.FillRectangle(Brushes.Black, New Rectangle(pt.X * 1000 - RECT_R, -pt.Y * 250 + 250 - RECT_R, RECT_R * 2, RECT_R * 2))
                If i > 0 Then
                    Dim pt_last As Vector2 = AC(i - 1)
                    g.DrawLine(Pens.Black, New Point(pt.X * 1000, -pt.Y * 250 + 250), New Point(pt_last.X * 1000, -pt_last.Y * 250 + 250))
                End If
            Next
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
        Dim file As New FileStream("C:\Users\asdfg\Desktop\Project322EK3\WSamples\dump.txt", FileMode.Create)
        Using sw As New StreamWriter(file)
            For Each pt In AC
                Dim line = pt.X.ToString & "," & pt.Y.ToString
                sw.WriteLine(line)
            Next
        End Using
        file.Close()
        file.Dispose()

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ' generate samples
        Dim sp(1023) As Single
        For i = 0 To 1023
            Dim x As Single = i / 1024.0
            sp(i) = Math.Sin(3.0 * (2.0 * Math.PI * x))
        Next
        Dim freq_table As Single() = DFT_v2(sp)
        Dim line As String = ""
        For Each f In freq_table
            line &= f.ToString & vbCrLf
        Next
        TextBox1.Text = line

    End Sub


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
            outputArray(t) = CSng(real / n) ' 除以n还原原始信号
        Next

        Return outputArray
    End Function



End Class