Imports System.Threading

Public Class Clock

    ' debug level: @see: Machine.vb
    Public _DebugLevel As Integer = 0
    Public _Frequency As UInteger
    Private _PeriodMS As Double, _Thread As Thread, _IsRunning As Boolean

    Private _TickCallback As Action

    Sub New(Frequency As UInteger, TickCallback As Action, Optional DebugLevel As Integer = 0)
        _Frequency = Frequency
        If Frequency < UInteger.MaxValue Then
            _PeriodMS = 1000 / Frequency
        Else
            _PeriodMS = 0
        End If
        _TickCallback = TickCallback
        _DebugLevel = DebugLevel
    End Sub

    Public Sub Start()
        _Thread = New Thread(AddressOf entry)
        _IsRunning = True
        _Thread.Start()
        If _DebugLevel >= 1 Then
            Console.WriteLine("Clock: Started")
        End If
    End Sub

    Public Sub StartSync()
        _IsRunning = True
        If _DebugLevel >= 1 Then
            Console.WriteLine("Clock: Started")
        End If
        entry()
    End Sub

    Public Sub [Stop]()
        _IsRunning = False
        If _Thread IsNot Nothing Then
            _Thread.Join()
        End If
        If _DebugLevel >= 1 Then
            Console.WriteLine("Clock: Stopped")
        End If
    End Sub

    Private Sub entry()
        If _Frequency > 0 Then
            If _PeriodMS > 0 Then
                Do While _IsRunning
                    Dim sw As New Stopwatch()
                    sw.Start()
                    If _DebugLevel >= 1 Then
                        Console.WriteLine("Clock: Tick")
                    End If
                    _TickCallback()
                    sw.Stop()
                    Dim sleeptime = _PeriodMS - sw.ElapsedMilliseconds
                    If sleeptime > 0 Then
                        Thread.Sleep(sleeptime)
                    End If
                Loop
            Else
                Do While _IsRunning
                    _TickCallback()
                Loop
            End If
        End If
    End Sub

End Class
