Imports System.Threading

Public Class Form2
    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

End Class