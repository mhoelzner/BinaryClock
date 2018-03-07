Public Class BinaryClock

    Private h As Integer = 0
    Private m As Integer = 0
    Private s As Integer = 0

    Private drag As Boolean
    Private mousex As Integer
    Private mousey As Integer

    Private flag_showTime As Boolean = False
    Private flag_stopColorWheel As Boolean = False

    Private timeForeColor As Color = Color.White
    Private timeBackColor As Color = Color.FromArgb(100, 100, 100)
    Private formBackColor As Color = Color.FromArgb(50, 50, 50)

    Private colorRefreshRate As Integer = 1000

    Private ClockMatrix(3, 5) As Point

    Private colorWheelThread As Threading.Thread

    Private Sub BinaryClock_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Init Clock Matrix for display

        ' Row 1
        ClockMatrix(0, 0) = New Point(25, 50)
        ClockMatrix(0, 1) = New Point(60, 50)
        ClockMatrix(0, 2) = New Point(95, 50)
        ClockMatrix(0, 3) = New Point(130, 50)
        ClockMatrix(0, 4) = New Point(165, 50)
        ClockMatrix(0, 5) = New Point(200, 50)

        ' Row 2
        ClockMatrix(1, 0) = New Point(25, 85)
        ClockMatrix(1, 1) = New Point(60, 85)
        ClockMatrix(1, 2) = New Point(95, 85)
        ClockMatrix(1, 3) = New Point(130, 85)
        ClockMatrix(1, 4) = New Point(165, 85)
        ClockMatrix(1, 5) = New Point(200, 85)

        ' Row 3
        ClockMatrix(2, 0) = New Point(25, 120)
        ClockMatrix(2, 1) = New Point(60, 120)
        ClockMatrix(2, 2) = New Point(95, 120)
        ClockMatrix(2, 3) = New Point(130, 120)
        ClockMatrix(2, 4) = New Point(165, 120)
        ClockMatrix(2, 5) = New Point(200, 120)

        ' Row 4
        ClockMatrix(3, 0) = New Point(25, 155)
        ClockMatrix(3, 1) = New Point(60, 155)
        ClockMatrix(3, 2) = New Point(95, 155)
        ClockMatrix(3, 3) = New Point(130, 155)
        ClockMatrix(3, 4) = New Point(165, 155)
        ClockMatrix(3, 5) = New Point(200, 155)

        colorWheelThread = New Threading.Thread(AddressOf SetColorWheel)
        colorWheelThread.IsBackground = False
        colorWheelThread.Start()

        ' optimize buffering
        Me.SetStyle(ControlStyles.UserPaint, True)
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

        Me.BackColor = formBackColor

        ' set form position
        If My.Settings.form_pos.X = 0 And My.Settings.form_pos.Y = 0 Then

            Me.CenterToScreen()

        Else

            Dim rect As Rectangle = setFormLocation(New Rectangle(My.Settings.form_pos, Me.Size))
            Me.Location = rect.Location

        End If

        Timer1.Enabled = True

        Me.Activate()
        Me.Show()

    End Sub

    ''' <summary>
    ''' double click on form to close it
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_DoubleClick(sender As Object, e As MouseEventArgs) Handles MyBase.DoubleClick

        If e.Button = Windows.Forms.MouseButtons.Left Then

            My.Settings.form_pos = Me.Location
            My.Settings.Save()

            flag_stopColorWheel = True

            Me.Close()

        End If

    End Sub

    ''' <summary>
    ''' show time as String for 3 seconds when right click on form
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_MouseClick(sender As Object, e As MouseEventArgs) Handles MyBase.MouseClick

        If e.Button = Windows.Forms.MouseButtons.Right Then

            Timer2.Enabled = True
            Timer2.Interval = 3000
            flag_showTime = True

            Me.Invalidate()

        End If

    End Sub

    ''' <summary>
    ''' timer to refresh time displayed every second
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        h = DateTime.Now.Hour
        m = DateTime.Now.Minute
        s = DateTime.Now.Second

        Me.Invalidate()

    End Sub

    ''' <summary>
    ''' timer to show time as String
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick

        Timer2.Enabled = False
        flag_showTime = False

    End Sub

    ''' <summary>
    ''' translate current time to matrix coordinates
    ''' </summary>
    ''' <param name="hms">hours; minutes; seconds</param>
    ''' <param name="lr">tens or ones digits</param>
    ''' <param name="value">bitsOfNumber value</param>
    ''' <returns></returns>
    Private Function Translate2DCoordinatesHMS(hms As Integer, lr As Integer, value As Integer) As Point

        'hms = 0 | lr = 0 'tens digits of hour
        'hms = 0 | lr = 1 'ones digits of hour

        Dim dic_values As New Dictionary(Of Integer, Integer) From {
            {1, 3},
            {2, 2},
            {4, 1},
            {8, 0}
        }

        Return ClockMatrix(dic_values(value), hms + lr)

    End Function

    ''' <summary>
    ''' paint method to display current time
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

        ' background
        e.Graphics.DrawRectangle(New Pen(timeForeColor), Me.ClientRectangle.Location.X, Me.ClientRectangle.Location.Y, Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1)


        For i As Integer = ClockMatrix.GetLowerBound(0) To ClockMatrix.GetUpperBound(0)
            For j As Integer = ClockMatrix.GetLowerBound(1) To ClockMatrix.GetUpperBound(1)
                e.Graphics.FillEllipse(New SolidBrush(timeBackColor), New Rectangle(ClockMatrix(i, j), New Size(30, 30)))
            Next
        Next

        ' hours
        Dim ha As Integer = Math.Floor(h / 10)

        Dim l_ha As List(Of Integer) = bitsOfNumber(ha)

        For i As Integer = 0 To l_ha.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(0, 0, l_ha.Item(i)), New Size(30, 30)))

        Next

        Dim hb As Integer = h - (ha * 10)

        Dim l_hb As List(Of Integer) = bitsOfNumber(hb)

        For i As Integer = 0 To l_hb.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(0, 1, l_hb.Item(i)), New Size(30.0, 30.0)))

        Next

        ' minutes
        Dim ma As Integer = Math.Floor(m / 10)

        Dim l_ma As List(Of Integer) = bitsOfNumber(ma)

        For i As Integer = 0 To l_ma.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(2, 0, l_ma.Item(i)), New Size(30.0, 30.0)))

        Next

        Dim mb As Integer = m - (ma * 10)

        Dim l_mb As List(Of Integer) = bitsOfNumber(mb)

        For i As Integer = 0 To l_mb.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(2, 1, l_mb.Item(i)), New Size(30.0, 30.0)))

        Next

        ' seconds
        Dim sa As Integer = Math.Floor(s / 10)

        Dim l_sa As List(Of Integer) = bitsOfNumber(sa)

        For i As Integer = 0 To l_sa.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(4, 0, l_sa.Item(i)), New Size(30.0, 30.0)))

        Next

        Dim sb As Integer = s - (sa * 10)

        Dim l_sb As List(Of Integer) = bitsOfNumber(sb)

        For i As Integer = 0 To l_sb.Count - 1

            e.Graphics.FillEllipse(New SolidBrush(timeForeColor), New Rectangle(Translate2DCoordinatesHMS(4, 1, l_sb.Item(i)), New Size(30.0, 30.0)))

        Next

        ' determine if time should be displayed as text
        If flag_showTime Then

            e.Graphics.DrawString(h.ToString("00") & " : " & m.ToString("00") & " : " & s.ToString("00"), New Font("Segoe UI", 12), New SolidBrush(timeForeColor), New Point(87, 200))

        End If

    End Sub

    ''' <summary>
    ''' divides a certian number into its 2^n counterparts
    ''' </summary>
    ''' <param name="n">number to be converted</param>
    ''' <returns></returns>
    Private Function BitsOfNumber(ByVal n As Integer) As List(Of Integer)

        ' returns a list of numbers in a 2^n format
        '   ( 7 = 2^2 + 2^1 + 2^0)
        '   ( 7 = 4   + 2   + 1  )

        Dim lst_bitsOfNumber As New List(Of Integer)

        If n = 0 Then
            Return lst_bitsOfNumber
        End If

        Do

            Dim bit As Integer = Math.Pow(2, Math.Floor(Math.Log(n, 2)))

            lst_bitsOfNumber.Add(bit)

            n -= lst_bitsOfNumber.Item(lst_bitsOfNumber.Count - 1)

        Loop Until n = 0

        Return lst_bitsOfNumber

    End Function

    ''' <summary>
    ''' determine if form should be moved
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown

        If e.Button = Windows.Forms.MouseButtons.Left Then

            drag = True 'Sets the variable drag to true.
            mousex = Windows.Forms.Cursor.Position.X - Me.Left 'Sets variable mousex
            mousey = Windows.Forms.Cursor.Position.Y - Me.Top 'Sets variable mousey

        End If

    End Sub

    ''' <summary>
    ''' move form when mouse down = true
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove

        If drag Then

            Me.Top = Windows.Forms.Cursor.Position.Y - mousey
            Me.Left = Windows.Forms.Cursor.Position.X - mousex

        End If

    End Sub

    ''' <summary>
    ''' stop movig form
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BinaryClock_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp

        If e.Button = Windows.Forms.MouseButtons.Left Then

            drag = False 'Sets drag to false, so the form does not move according to the code in MouseMove

        End If


    End Sub

    ''' <summary>
    ''' sets the current color according to minute within an hour
    ''' </summary>
    Private Sub SetColorWheel()

        Do While True

            Dim now As DateTime = DateTime.Now
            Dim h As Integer = now.Hour
            Dim m As Integer = now.Minute
            Dim s As Integer = now.Second

            Dim minuteSecondValue As Integer = s + (m * 60)

            For j As Integer = minuteSecondValue To 3600

                Dim hue As Double = j / 3600.0

                timeForeColor = HSL_to_RGB(hue, 1.0, 0.66)

                Threading.Thread.Sleep(colorRefreshRate)

                If flag_stopColorWheel Then
                    Exit Do
                End If

            Next

            If flag_stopColorWheel Then
                Exit Do
            End If

        Loop

    End Sub

    ''' <summary>
    ''' Convert HSL Values to RGB Values
    ''' </summary>
    ''' <param name="h">Hue</param>
    ''' <param name="s">Saturation</param>
    ''' <param name="l">Luminance</param>
    ''' <returns></returns>
    Private Function HSL_to_RGB(h As Double, s As Double, l As Double) As Color

        Dim r As Double = 0
        Dim g As Double = 0
        Dim b As Double = 0

        Dim temp1, temp2 As Double

        If l = 0 Then
            r = 0
            g = 0
            b = 0
        Else
            If s = 0 Then
                r = l
                g = l
                b = l
            Else
                temp2 = IIf(l <= 0.5, l * (1.0 + s), l + s - l * s)
                temp1 = 2.0 * l - temp2

                Dim t3() As Double = {h + 1.0 / 3.0, h, h - 1.0 / 3.0}
                Dim clr() As Double = {0, 0, 0}
                Dim i As Integer
                For i = 0 To 2
                    If t3(i) < 0 Then
                        t3(i) += 1.0
                    End If
                    If t3(i) > 1 Then
                        t3(i) -= 1.0
                    End If
                    If 6.0 * t3(i) < 1.0 Then
                        clr(i) = temp1 + (temp2 - temp1) * t3(i) * 6.0
                    ElseIf 2.0 * t3(i) < 1.0 Then
                        clr(i) = temp2
                    ElseIf 3.0 * t3(i) < 2.0 Then
                        clr(i) = temp1 + (temp2 - temp1) * (2.0 / 3.0 - t3(i)) * 6.0
                    Else
                        clr(i) = temp1
                    End If
                Next i
                r = clr(0)
                g = clr(1)
                b = clr(2)
            End If
        End If

        Return Color.FromArgb(CInt(255 * r), CInt(255 * g), CInt(255 * b))

    End Function

    ''' <summary>
    ''' checks and sets form location saved from previous session
    ''' </summary>
    ''' <param name="savedRect">location and size of form previously saved</param>
    ''' <returns></returns>
    Private Function SetFormLocation(savedRect As Rectangle) As Rectangle

        Dim resultRect As Rectangle = New Rectangle(New Point((Screen.PrimaryScreen.WorkingArea.Width - Me.MinimumSize.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - Me.MinimumSize.Height) / 2), Me.MinimumSize)

        Try

            Dim settingsPoint As Point

            Dim newWidth As Integer
            Dim newHeight As Integer

            settingsPoint = savedRect.Location

            newWidth = savedRect.Size.Width
            newHeight = savedRect.Size.Height

            Dim screenNo As Integer = pointOnOneOfConnectedScreens(settingsPoint)

            If screenNo <> -1 Then

                If Screen.AllScreens(screenNo).WorkingArea.Width < newWidth Then

                    newWidth = Screen.AllScreens(screenNo).WorkingArea.Width

                End If

                If Screen.AllScreens(screenNo).WorkingArea.Height < newHeight Then

                    newHeight = Screen.AllScreens(screenNo).WorkingArea.Height

                End If

                If settingsPoint.Y + newHeight > Screen.AllScreens(screenNo).Bounds.Bottom Then

                    settingsPoint = New Point(settingsPoint.X, Screen.AllScreens(screenNo).Bounds.Bottom - newHeight)

                End If

                If settingsPoint.X + newWidth > Screen.AllScreens(screenNo).Bounds.Right Then

                    settingsPoint = New Point(Screen.AllScreens(screenNo).Bounds.Right - newWidth, settingsPoint.Y)

                End If

                resultRect = New Rectangle(New Point(settingsPoint.X, settingsPoint.Y), New Size(newWidth, newHeight))

            End If

        Catch ex As Exception

        End Try

        Return resultRect

    End Function

    ''' <summary>
    ''' check if point is visible on any screen
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    Private Function PointOnOneOfConnectedScreens(ByVal p As Point) As Integer

        Return Screen.AllScreens.ToList.FindIndex(Function(x) x.Bounds.Contains(p))

    End Function

End Class