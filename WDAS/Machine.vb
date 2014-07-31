Imports System.IO

Public Class Machine
    Implements IDisposable

    ' debug level:
    '    0: no debug
    '    1: debug info
    '    2: debug instructions
    '    3: clock ticks

    Public _DebugLevel As Integer
    Public _Clock As Clock
    Public _Memory As Memory
    Public _CPU As CPU
    Public _HDD As Stream

    Sub New(Optional freq As UInteger = UInteger.MaxValue, Optional memsize As Integer = 1024 * 1024, Optional DebugLevel As Integer = 0)
        _DebugLevel = DebugLevel
        _Memory = New Memory(memsize)
        _CPU = New CPU(_Memory, DebugLevel)
        _Clock = New Clock(freq, AddressOf _CPU.ClockTick, DebugLevel)
        AddInterrupts()
    End Sub

    Public Sub SetDebugMode(dbg As Integer)
        _DebugLevel = dbg
        _CPU._DebugLevel = _DebugLevel
        _Clock._DebugLevel = _DebugLevel
    End Sub

    Public Sub CtrlC(ByVal sender As Object, ByVal args As ConsoleCancelEventArgs)
        args.Cancel = True
        _Clock.Stop()
        INT3()
        'Console.WriteLine("aaa")
        _Clock.Start()
    End Sub

    Private Sub INT3()
        _CPU.PrintIPS()
        '_CPU.PrintInfo()
        Dim debugcommand = ""
        Do While True
            Console.Write("debug> ")
            debugcommand = Console.ReadLine()
            If debugcommand = "" Then
                Continue Do
            End If
            Dim cmddata = debugcommand.Split({" "c}, 2)
            Select Case cmddata(0)
                Case "run"
                    Return
                Case "reg"
                    _CPU.PrintRegisters()
                Case "halt", "hlt"
                    _CPU.Halt()
                    Return
                Case "prog"
                    _CPU.PrintProgram()
                Case "info"
                    _CPU.PrintInfo()
                Case "stack"
                    _CPU.PrintIntStack()
                    _CPU.PrintFloatStack()
                Case "ins"
                    _CPU.PrintInstructions()
                    _CPU.PrintIPS()
                Case "ips"
                    _CPU.PrintIPS()
                Case "loadsymbols"
                    _CPU.LoadSymbols(cmddata(1))
                Case Else
                    Console.WriteLine("run, reg, halt, hlt, prog, info, stack, ins, ips, loadsymbols")
            End Select
        Loop
    End Sub

    Private Sub AddInterrupts()
        _CPU.OnInterrput(5, Sub()
                                'print char
                                If _DebugLevel > 0 Then
                                    Debug.Print(_CPU.eax)
                                End If
                                Console.Write(vb.Chr(_CPU.eax And &HFF))
                            End Sub)
        _CPU.OnInterrput(9, Sub()
                                'readkey
                                _CPU.eax = vb.Asc(Console.ReadKey(True).KeyChar)
                            End Sub)
        _CPU.OnInterrput(3, AddressOf INT3)
        _CPU.OnInterrput(1, Sub()
                                'print info
                                _CPU.PrintInfo()
                            End Sub)
        _CPU.OnInterrput(13, Sub()
                                 'read/write hdd, read: AH=1, write AH=2
                                 Dim ah As Byte = (_CPU.eax And &HFF00) >> 8
                                 Dim addr = _CPU.ecx
                                 If addr < 0 OrElse addr >= _HDD.Length Then
                                     Throw New CPUException("HDD overflow")
                                 End If
                                 If ah = 1 Then
                                     _HDD.Seek(addr, SeekOrigin.Begin)
                                     _CPU.eax = (_CPU.eax And &HFFFFFFFFFFFFFF00) Or _HDD.ReadByte()
                                 ElseIf ah = 2 Then
                                     'write hdd
                                     _HDD.Seek(addr, SeekOrigin.Begin)
                                     _HDD.WriteByte(_CPU.eax And &HFF)
                                     _HDD.Flush()
                                 End If
                             End Sub)
    End Sub

    Public Sub Start()
        If _DebugLevel >= 1 Then
            PrintInfo()
        End If
        _CPU.Start()
        _Clock.Start()
    End Sub

    Public Sub StartSync()
        Start()
        _CPU._HaltEvent.WaitOne()
        _Clock.Stop()
    End Sub

    Public Sub Wait()
        _CPU._HaltEvent.WaitOne()
    End Sub

    Public Sub StartSyncClock()
        If _DebugLevel >= 1 Then
            PrintInfo()
        End If
        _CPU.Start()
        _Clock.StartSync()
    End Sub

    Public Sub PrintInfo()
        Console.WriteLine("CPU Frequency: {0}MHz, Memory Size: {1}KiB", _Clock._Frequency / 1000000, _Memory._Size / 1024)
    End Sub

    Public Sub LoadFile(filename As String)
        Using fs As New FileStream(filename, FileMode.Open, FileAccess.Read)
            Load(BinaryCodeHelper.Read(fs))
        End Using
    End Sub

    Private Sub PutIntoMemory(prog As InstructionInfo())
        For i = 0 To prog.Length - 1
            _Memory.program(i) = prog(i)
        Next
    End Sub

    Public Sub Load(prog As InstructionInfo())
        If _Memory._Size < prog.Length Then
            Throw New Exception("Memory overflow")
        End If

        PutIntoMemory(prog)
    End Sub

    Public Sub Halt()
        _CPU.Halt()
        _Clock.Stop()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO:  释放托管状态(托管对象)。
                _CPU.Dispose()
                If _HDD IsNot Nothing Then
                    Try
                        _HDD.Close()
                    Catch ex As Exception

                    End Try
                End If
            End If

            ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
            ' TODO:  将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub

    ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码是为了正确实现可处置模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
