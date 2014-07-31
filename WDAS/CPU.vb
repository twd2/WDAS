Imports System.Threading
Imports System.IO
Imports System.Text

Partial Public Class CPU
    'Inherits CPU
    Implements IDisposable

    ' debug level: @see: Machine.vb
    Public _DebugLevel As Integer = 0, _InterruptEnable As Boolean = False
    Public _RanCount As UInteger = 0, _StartTime As DateTime
    Public _InterruptVector(255) As InterruptInfo

    Public _HaltEvent As New ManualResetEvent(False)

    Public _Symbols As Dictionary(Of Integer, String) = Nothing

    Private _IsRunning As Boolean
    Private _Memory As Memory

    Private Sub init()
        initRegister()
        initExecuteInstruction()
        'If _DebugLevel < 2 Then
        '    DeleteDebugInstruction()
        'End If
    End Sub

    Sub New(Optional mem As Memory = Nothing, Optional debug As Integer = 0)
        _DebugLevel = debug
        init()
        _Memory = mem
        _IsRunning = True
    End Sub

    Public Sub LoadSymbols(filename As String)
        If Not File.Exists(filename) Then
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("Symbols file not found")
            Console.ForegroundColor = ConsoleColor.Gray
            Return
        End If
        _Symbols = New Dictionary(Of Integer, String)
        Using sr As New StreamReader(filename, Encoding.UTF8)
            Do While Not sr.EndOfStream
                Dim s = sr.ReadLine().Split({","}, StringSplitOptions.None)
                Dim a = Integer.Parse(s(1))
                If Not _Symbols.ContainsKey(a) Then
                    _Symbols.Add(a, s(0))
                End If
            Loop
        End Using
    End Sub

    Private Sub pushi(num As Long)
        If esp >= _Memory._Size - 1 Then
            Throw New CPUException("Stack overflow")
        End If
        esp += 1
        _Memory.intdata(esp) = num
    End Sub

    Private Function popi() As Long
        If esp <= 0 Then
            Throw New CPUException("Stack overflow")
        End If
        Dim num = _Memory.intdata(esp)
        esp -= 1
        Return num
    End Function

    Private Sub pushf(num As Double)
        If esfp >= _Memory._Size Then
            Throw New CPUException("Stack overflow")
        End If
        esfp += 1
        _Memory.floatdata(esfp) = num
    End Sub

    Private Function popf() As Double
        If esfp <= 0 Then
            Throw New CPUException("Stack overflow")
        End If
        Dim num = _Memory.floatdata(esfp)
        esfp -= 1
        Return num
    End Function

    Public Sub ClockTick()
        Try
            If _IsRunning Then
                execute()
            End If
        Catch ex As Exception
            Console.WriteLine("Error:")
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine(ex.Message)
            Console.ForegroundColor = ConsoleColor.Gray
            PrintInfo()
            Halt()
        End Try
    End Sub

    Public Sub Start()
        _IsRunning = True
        _HaltEvent.Reset()
        If _DebugLevel >= 1 Then
            Console.WriteLine("CPU: Started")
        End If
        _StartTime = DateTime.Now
    End Sub

    Public Sub Halt()
        If _DebugLevel >= 1 Then
            PrintInfo()
            Console.WriteLine("CPU: Halt")
        End If
        _IsRunning = False
        _HaltEvent.Set()
    End Sub

    Public Sub OnInterrput(id As Integer, action As Action)
        Dim ii As New InterruptInfo(InterruptInfo.InterruptType.outer)
        ii._outer_action = action
        _InterruptVector(id) = ii
    End Sub

    Private Sub OnInterrupt(id As Integer, jmpto As Integer)
        Dim ii As New InterruptInfo(InterruptInfo.InterruptType.inner)
        ii._inner_address = jmpto
        _InterruptVector(id) = ii
    End Sub

    Private Sub checkInterruptID(id As Integer)
        If id < 0 OrElse id >= _InterruptVector.Length Then
            Throw New CPUException("Invaild Interrupt ID")
        End If
    End Sub

    Public Sub Interrupt(id As Integer)
        If Not _InterruptEnable Then
            Return
        End If
        checkInterruptID(id)
        Dim ii = _InterruptVector(id)
        If ii Is Nothing Then
            Throw New CPUException("Undefined interrupt function")
        End If
        If ii._Type = InterruptInfo.InterruptType.inner Then
            pushi(eip)
            eip = ii._inner_address
        Else
            If ii._outer_action Is Nothing Then
                Throw New CPUException("Undefined interrupt function")
            End If
            ii._outer_action()
        End If
    End Sub

    Private Sub execute()
        If eip >= _Memory._Size OrElse _Memory.program(eip) Is Nothing Then
            Throw New CPUException("Invaild Instruction Null")
            Halt()
            Return
        End If
        Dim ins = _Memory.program(eip)
        eip += 1
        'Dim indexofspace = cmddata.IndexOf(" "c)
        'Dim cmd = "", data = ""
        'If indexofspace >= 0 Then
        '    cmd = Trim(cmddata.Substring(0, indexofspace))
        '    data = Trim(cmddata.Substring(indexofspace + 1))
        'Else
        '    cmd = cmddata
        'End If
        'cmd = cmd.ToLower()

        'execute(ins)
        If ins.ID >= InstructionCount OrElse ins.ID < 0 Then
            Throw New CPUException("Invaild Instruction """ & ins.ID.ToString() & """")
        End If
        InstructionExecuter(ins.ID)(ins.Params)

        _RanCount += 1
        If _RanCount = UInteger.MaxValue Then
            _RanCount = 0
        End If

        If _DebugLevel > 0 Then
            '_RanCount += 1

            If _DebugLevel >= 1 AndAlso _DebugLevel <= 2 Then
                Console.WriteLine(ins.ToString() & "...")
            ElseIf _DebugLevel >= 3 Then
                PrintInfo()
            End If
        End If

        'execute(cmd, Data)
    End Sub

    'Private Sub execute(ins As InstructionInfo)
    '    If ins.ID >= InstructionCount OrElse ins.ID < 0 Then
    '        Throw New CPUException("Invaild Instruction """ & ins.ID.ToString() & """")
    '    End If
    '    InstructionExecuter(ins.ID)(ins.Params)
    '    'executeInstruction(cmd)(data)
    'End Sub

    'Private Sub execute(cmd As String, data As ParamsInfo)
    '    If Not executeInstruction.ContainsKey(cmd) Then
    '        Throw New CPUException("Invaild Instruction """ & cmd & """")
    '    End If
    '    executeInstruction(cmd)(data)
    'End Sub

    Private Function getIntValue(s As ParameterInfo) As Long
        If s.Type = ParameterInfo.ParameterType.Int Then
            Return s.Value
        ElseIf s.Type = ParameterInfo.ParameterType.Register Then
            Return getIntRegisterValue(s.Value)
        Else
            Throw New CPUException("Invalid Param")
        End If
    End Function

    Private Function getFloatValue(s As ParameterInfo) As Double
        If s.Type = ParameterInfo.ParameterType.Float Then
            Return s.Value_Float
        ElseIf s.Type = ParameterInfo.ParameterType.Register Then
            Return getFloatRegisterValue(s.Value)
        Else
            Throw New CPUException("Invalid Param")
        End If
    End Function

    Private Function getIntRegisterValue(id As Integer) As Long
        If id >= IntRegisterCount OrElse id < IntRegisterOffset Then
            Throw New CPUException("Invaild Register " & id.ToString())
        End If
        If id = ebp_id Then
            Return ebp
        ElseIf id = esp_id Then
            Return esp
        Else
            Return IntRegisterGetter(id)()
        End If
    End Function

    Private Sub setIntRegisterValue(id As Integer, data As Long)
        If id >= IntRegisterCount OrElse id < IntRegisterOffset Then
            Throw New CPUException("Invaild Register " & id.ToString())
        End If
        If id = ebp_id Then
            ebp = data
        ElseIf id = esp_id Then
            esp = data
        Else
            IntRegisterSetter(id)(data)
        End If
    End Sub

    Private Function getFloatRegisterValue(id As Integer) As Double
        If id >= RegisterCount OrElse id < FloatRegisterOffset Then
            Throw New CPUException("Invaild Register " & id.ToString())
        End If
        Return FloatRegisterGetter(id - FloatRegisterOffset)()
    End Function

    Private Sub setFloatRegisterValue(id As Integer, data As Double)
        If id >= RegisterCount OrElse id < FloatRegisterOffset Then
            Throw New CPUException("Invaild Register """ & id.ToString() & """")
        End If
        FloatRegisterSetter(id - FloatRegisterOffset)(data)
    End Sub

    Public Sub PrintInfo()
        If _DebugLevel > 0 Then
            PrintIPS()
        End If
        PrintInstructions()
        PrintRegisters()
        PrintProgram()
        PrintIntStack()
        PrintFloatStack()
    End Sub

    Public Sub PrintIPS()
        Console.WriteLine("Instructions:")
        Console.WriteLine("{0}", _RanCount)
        Console.WriteLine("IPS:")
        Console.WriteLine("{0} instructions per second", _RanCount / DateTime.Now.Subtract(_StartTime).TotalSeconds)
    End Sub

    Public Sub PrintProgram()
        Console.WriteLine("Program:")
        Dim istart = Math.Max(0, eip - 10),
            iend = Math.Min(_Memory._Size - 1, eip + 10)
        Dim dcount = iend.ToString().Length
        For i = istart To iend
            If i = eip Then
                Console.ForegroundColor = ConsoleColor.Cyan
            ElseIf i = eip - 1 Then
                Console.ForegroundColor = ConsoleColor.Yellow
            Else
                Console.ForegroundColor = ConsoleColor.Gray
            End If
            If _Memory.program(i) IsNot Nothing Then
                Console.Write(New String("0"c, dcount - i.ToString().Length))
                Console.Write(i.ToString())
                Console.Write("  ")
                Console.Write(_Memory.program(i).ToString(Me))
                If _Symbols IsNot Nothing AndAlso _Symbols.ContainsKey(i) Then
                    Console.Write("  ({0})", _Symbols(i))
                End If
                Console.WriteLine()
            End If
        Next
        Console.ForegroundColor = ConsoleColor.Gray
    End Sub

    Public Sub PrintIntStack()
        Console.WriteLine("Integer Stack:")
        Dim istart = Math.Max(0, esp - 20),
            iend = Math.Min(_Memory._Size - 1, esp + 5)
        Dim dcount = iend.ToString().Length
        For i = istart To iend
            If i = esp Then
                Console.ForegroundColor = ConsoleColor.Cyan
            Else
                Console.ForegroundColor = ConsoleColor.Gray
            End If
            Console.Write(New String("0"c, dcount - i.ToString().Length))
            Console.Write(i.ToString())
            Console.Write("  ")
            Console.WriteLine(_Memory.intdata(i))
        Next
        Console.ForegroundColor = ConsoleColor.Gray
    End Sub

    Public Sub PrintFloatStack()
        Console.WriteLine("Float Stack:")
        Dim istart = Math.Max(0, esfp - 20),
            iend = Math.Min(_Memory._Size - 1, esfp + 5)
        Dim dcount = iend.ToString().Length
        For i = istart To iend
            If i = esfp Then
                Console.ForegroundColor = ConsoleColor.Cyan
            Else
                Console.ForegroundColor = ConsoleColor.Gray
            End If
            Console.Write(New String("0"c, dcount - i.ToString().Length))
            Console.Write(i.ToString())
            Console.Write("  ")
            Console.WriteLine(_Memory.floatdata(i))
        Next
        Console.ForegroundColor = ConsoleColor.Gray
    End Sub


    Private Sub pushRegisters()
        pushi(eax)
        pushi(ebx)
        pushi(ecx)
        pushi(edx)
        pushi(esp)
        pushi(esfp)
        pushi(eip)
        pushi(r0)
        pushi(r1)
        pushi(r2)
        pushi(r3)
        pushi(r4)
        pushi(r5)
        pushi(r6)
        pushi(r7)
        pushf(f0)
        pushf(f1)
        pushf(f2)
        pushf(f3)
        pushf(f4)
        pushf(f5)
        pushf(f6)
        pushf(f7)
    End Sub

    Private Sub popRegisters()
        f7 = popf()
        f6 = popf()
        f5 = popf()
        f4 = popf()
        f3 = popf()
        f2 = popf()
        f1 = popf()
        f0 = popf()
        r7 = popi()
        r6 = popi()
        r5 = popi()
        r4 = popi()
        r3 = popi()
        r2 = popi()
        r1 = popi()
        r0 = popi()
        eip = popi()
        esfp = popi()
        esp = popi()
        edx = popi()
        ecx = popi()
        ebx = popi()
        eax = popi()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO:  释放托管状态(托管对象)。
                _HaltEvent.Close()
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
