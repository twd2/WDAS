Partial Public Class CPU

    Private _executeInstruction As Dictionary(Of String, Action(Of ParameterInfo()))

    Public InstructionCount As Integer
    Public InstructionExecuter() As Action(Of ParameterInfo())
    Public InstructionName() As String
    Public InstructionID As Dictionary(Of String, Integer)

    Private Function GetInstructionID(cmd As String)
        Return InstructionName(cmd)
    End Function

    Private Sub AddInstructionAlias(cname As String, target As String)
        _executeInstruction.Add(cname, _executeInstruction(target))
    End Sub

    Private Sub initExecuteInstruction()
        _executeInstruction = New Dictionary(Of String, Action(Of ParameterInfo())) From {
            {"nop", Sub(data As ParameterInfo())
                        'nop
                    End Sub},
            {"hlt", Sub(data As ParameterInfo())
                        Halt()
                    End Sub},
            {"ldri", Sub(data As ParameterInfo())
                         Dim addr = popi()
                         If addr >= _Memory._MaxAddress OrElse addr <= 0 Then
                             Throw New CPUException("Memory overflow")
                         ElseIf addr >= _Memory._FloatOffset Then 'float to int
                             pushi(BinaryCodeHelper.DoubleToInt64(_Memory.floatdata(addr - _Memory._FloatOffset)))
                         Else
                             pushi(_Memory.intdata(addr))
                         End If
                     End Sub},
            {"ldrf", Sub(data As ParameterInfo())
                         Dim addr = popi()
                         If addr >= _Memory._MaxAddress OrElse addr <= 0 Then
                             Throw New CPUException("Memory overflow")
                         ElseIf addr < _Memory._FloatOffset Then 'int to float
                             pushf(BinaryCodeHelper.Int64ToDouble(_Memory.intdata(addr)))
                         Else
                             pushf(_Memory.floatdata(addr))
                         End If
                     End Sub},
            {"stri", Sub(data As ParameterInfo())
                         Dim da As Long = popi()
                         Dim addr = popi()
                         If addr >= _Memory._MaxAddress OrElse addr <= 0 Then
                             Throw New CPUException("Memory overflow")
                         ElseIf addr >= _Memory._FloatOffset Then 'int to float
                             _Memory.floatdata(addr - _Memory._FloatOffset) = BinaryCodeHelper.Int64ToDouble(da)
                         Else
                             _Memory.intdata(addr) = da
                         End If
                     End Sub},
            {"strf", Sub(data As ParameterInfo())
                         Dim da As Double = popf()
                         Dim addr = popi()
                         If addr >= _Memory._MaxAddress OrElse addr <= 0 Then
                             Throw New CPUException("Memory overflow")
                         ElseIf addr < _Memory._FloatOffset Then 'float to int
                             _Memory.intdata(addr) = BinaryCodeHelper.DoubleToInt64(da)
                         Else
                             _Memory.floatdata(addr - _Memory._FloatOffset) = da
                         End If
                     End Sub},
            {"popi", Sub(data As ParameterInfo())
                         If data.Length > 0 Then
                             setIntRegisterValue(data(0).Value, popi())
                         Else
                             popi()
                         End If
                     End Sub},
            {"pushi", Sub(data As ParameterInfo())
                          pushi(getIntValue(data(0)))
                      End Sub},
            {"popf", Sub(data As ParameterInfo())
                         If data.Length > 0 Then
                             setFloatRegisterValue(data(0).Value, popf())
                         Else
                             popf()
                         End If
                     End Sub},
            {"pushf", Sub(data As ParameterInfo())
                          pushf(getFloatValue(data(0)))
                      End Sub},
            {"itof", Sub(data As ParameterInfo())
                         pushf(popi())
                     End Sub},
            {"ftoi", Sub(data As ParameterInfo())
                         pushi(popf())
                     End Sub},
            {"and", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi(a And b)
                    End Sub},
            {"or", Sub(data As ParameterInfo())
                       Dim a, b As Long
                       a = popi()
                       b = popi()
                       pushi(a Or b)
                   End Sub},
            {"xor", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi(a Xor b)
                    End Sub},
            {"not", Sub(data As ParameterInfo())
                        Dim a As Long
                        a = popi()
                        pushi(Not a)
                    End Sub},
            {"addi", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi(a + b)
                     End Sub},
            {"subi", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi(b - a)
                     End Sub},
            {"muli", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi(a * b)
                     End Sub},
            {"divi", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         If a = 0 Then
                             pushi(0)
                             pushi(0)
                             Interrupt(0) '除法溢出中断
                             Return
                         End If
                         Dim m As Long
                         Dim c = Math.DivRem(b, a, m)
                         pushi(m)
                         pushi(c)
                     End Sub},
            {"negi", Sub(data As ParameterInfo())
                         pushi(-popi())
                     End Sub},
            {"inci", Sub(data As ParameterInfo())
                         pushi(popi() + 1)
                     End Sub},
            {"deci", Sub(data As ParameterInfo())
                         pushi(popi() - 1)
                     End Sub},
            {"addf", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushf(a + b)
                     End Sub},
            {"subf", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushf(b - a)
                     End Sub},
            {"mulf", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushf(a * b)
                     End Sub},
            {"divf", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushf(b / a)
                     End Sub},
            {"negf", Sub(data As ParameterInfo())
                         pushf(-popf())
                     End Sub},
            {"incf", Sub(data As ParameterInfo())
                         pushf(popf() + 1)
                     End Sub},
            {"decf", Sub(data As ParameterInfo())
                         pushf(popf() - 1)
                     End Sub},
            {"gti", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi((b > a) And 1)
                    End Sub},
            {"gtei", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi((b >= a) And 1)
                     End Sub},
            {"lti", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi((b < a) And 1)
                    End Sub},
            {"ltei", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi((b <= a) And 1)
                     End Sub},
            {"eqi", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi((b = a) And 1)
                    End Sub},
            {"neqi", Sub(data As ParameterInfo())
                         Dim a, b As Long
                         a = popi()
                         b = popi()
                         pushi((b <> a) And 1)
                     End Sub},
            {"gtf", Sub(data As ParameterInfo())
                        Dim a, b As Double
                        a = popf()
                        b = popf()
                        pushi((b > a) And 1)
                    End Sub},
            {"gtef", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushi((b >= a) And 1)
                     End Sub},
            {"ltf", Sub(data As ParameterInfo())
                        Dim a, b As Double
                        a = popf()
                        b = popf()
                        pushi((b < a) And 1)
                    End Sub},
            {"ltef", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushi((b <= a) And 1)
                     End Sub},
            {"eqf", Sub(data As ParameterInfo())
                        Dim a, b As Double
                        a = popf()
                        b = popf()
                        pushi((b = a) And 1)
                    End Sub},
            {"neqf", Sub(data As ParameterInfo())
                         Dim a, b As Double
                         a = popf()
                         b = popf()
                         pushi((b <> a) And 1)
                     End Sub},
            {"shl", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi(b << a)
                    End Sub},
            {"shr", Sub(data As ParameterInfo())
                        Dim a, b As Long
                        a = popi()
                        b = popi()
                        pushi(b >> a)
                    End Sub},
            {"jmp", Sub(data As ParameterInfo())
                        eip = getIntValue(data(0))
                    End Sub},
            {"jt", Sub(data As ParameterInfo())
                       If popi() And 1 Then
                           eip = getIntValue(data(0))
                       End If
                   End Sub},
            {"jf", Sub(data As ParameterInfo())
                       If Not (popi() And 1) And 1 Then
                           eip = getIntValue(data(0))
                       End If
                   End Sub},
            {"call", Sub(data As ParameterInfo())
                         'pushRegisters()
                         pushi(eip)
                         eip = getIntValue(data(0))
                     End Sub},
            {"ret", Sub(data As ParameterInfo())
                        eip = popi()
                        'popRegisters()
                    End Sub},
            {"pushreg", Sub(data As ParameterInfo())
                            pushRegisters()
                        End Sub},
            {"popreg", Sub(data As ParameterInfo())
                           popRegisters()
                       End Sub},
            {"randi", Sub(data As ParameterInfo())
                          Dim rand As New Random()
                          pushi(rand.Next())
                      End Sub},
            {"randf", Sub(data As ParameterInfo())
                          Dim rand As New Random()
                          pushf(rand.NextDouble())
                      End Sub},
            {"cli", Sub(data As ParameterInfo())
                        If _DebugLevel Then
                            Console.WriteLine("Disable Interrupt")
                        End If
                        _InterruptEnable = False
                    End Sub},
            {"sei", Sub(data As ParameterInfo())
                        If _DebugLevel Then
                            Console.WriteLine("Enable Interrupt")
                        End If
                        _InterruptEnable = True
                    End Sub},
            {"int", Sub(data As ParameterInfo())
                        Dim id = getIntValue(data(0))
                        Interrupt(id)
                    End Sub},
            {"jint", Sub(data As ParameterInfo())
                         OnInterrupt(popi(), getIntValue(data(0)))
                     End Sub},
            {"rdtsc", Sub(data As ParameterInfo())
                          pushi(_RanCount)
                      End Sub}
        }
        'AddInstructionAlias("pop", "popi")
        'AddInstructionAlias("push", "pushi")
        'AddInstructionAlias("add", "addi")
        'AddInstructionAlias("sub", "subi")
        'AddInstructionAlias("mul", "muli")
        'AddInstructionAlias("div", "divi")
        'AddInstructionAlias("neg", "negi")
        'AddInstructionAlias("inc", "inci")
        'AddInstructionAlias("dec", "deci")
        'AddInstructionAlias("gt", "gti")
        'AddInstructionAlias("gte", "gtei")
        'AddInstructionAlias("lt", "lti")
        'AddInstructionAlias("lte", "ltei")
        'AddInstructionAlias("eq", "eqi")
        'AddInstructionAlias("equ", "eqi")
        'AddInstructionAlias("equi", "eqi")
        'AddInstructionAlias("neq", "neqi")
        'AddInstructionAlias("dbg_pr", "dbg_pri")
        'AddInstructionAlias("dbg_print", "dbg_pri")
        'AddInstructionAlias("dbg_printi", "dbg_pri")
        'AddInstructionAlias("dbg_printireg", "dbg_prreg")
        'AddInstructionAlias("dbg_printreg", "dbg_prreg")
        'AddInstructionAlias("dbg_printiregisters", "dbg_prreg")
        'AddInstructionAlias("dbg_printregisters", "dbg_prreg")
        'AddInstructionAlias("dbg_printf", "dbg_prf")
        'AddInstructionAlias("dbg_printfreg", "dbg_prfreg")
        'AddInstructionAlias("dbg_printfregisters", "dbg_prfreg")
        'AddInstructionAlias("dbg_prints", "dbg_prs")
        'AddInstructionAlias("dbg_printstring", "dbg_prs")
        'AddInstructionAlias("dbg_printc", "dbg_prc")
        'AddInstructionAlias("dbg_printchar", "dbg_prc")
        'AddInstructionAlias("dbg_loads", "dbg_lds")
        'AddInstructionAlias("dbg_loadstring", "dbg_lds")
        'AddInstructionAlias("dbg_loadc", "dbg_ldc")
        'AddInstructionAlias("dbg_loadchar", "dbg_ldc")
        'AddInstructionAlias("dbg_readchar", "dbg_rdc")
        'AddInstructionAlias("ldr", "ldri")
        'AddInstructionAlias("str", "stri")
        'AddInstructionAlias("jtrue", "jt")
        'AddInstructionAlias("jfalse", "jf")
        'AddInstructionAlias("j", "jmp")
        'AddInstructionAlias("jcall", "call")
        'AddInstructionAlias("rand", "randi")
        'AddInstructionAlias("jmpint", "jint")
        'AddInstructionAlias("jmpwhenint", "jint")
        'AddInstructionAlias("jmpwheninterrupt", "jint")
        'AddCommandAlias("swap", "swapi")

        MakeInstructionCache()
    End Sub

    Public Sub MakeInstructionCache()
        InstructionCount = _executeInstruction.Count
        InstructionExecuter = _executeInstruction.Values().ToArray()
        InstructionName = _executeInstruction.Keys().ToArray()
        InstructionID = New Dictionary(Of String, Integer)
        For i = 0 To InstructionName.Length - 1
            InstructionID.Add(InstructionName(i), i)
        Next
    End Sub

    'Public Sub DeleteDebugInstruction()
    '    Dim keys = _executeInstruction.Keys.ToList().FindAll(
    '        Function(m As String)
    '            Return m.StartsWith("dbg")
    '        End Function
    '        )
    '    For Each k In keys
    '        _executeInstruction.Remove(k)
    '    Next
    '    MakeInstructionCache()
    'End Sub

    Public Sub PrintInstructions()
        Console.WriteLine("Supported Instructions:")
        Dim keys = InstructionName
        For i = 0 To keys.Count - 1
            Console.Write(keys(i))
            If i < keys.Count - 1 Then
                Console.Write(", ")
            End If
        Next
        Console.WriteLine()
    End Sub

End Class
