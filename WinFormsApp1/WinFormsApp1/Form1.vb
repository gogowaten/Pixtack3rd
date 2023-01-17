Public Class Form1
    Dim hitCounter As Long 'ここ、カウント用変数
    Dim hitFrg As Boolean
    Dim RR As Long

    '自機と隕石の当たり判定
    Private Sub hitCheck()
        Dim dis As Long
        If dis < RR * RR Then
            hitFrg = True
            hitCounter += 1 'ここ、カウント
            LabelHitCount.Text = hitCounter.ToString() 'ここ、ラベルの表示更新
        End If
    End Sub
End Class
