'Imports System.Threading

'Public MustInherit Class CPU
'    Implements IDisposable

'    Public IntRegisterCount As Integer
'    Public IntRegisterGetter() As Func(Of Long)
'    Public IntRegisterSetter() As Action(Of Long)
'    Public FloatRegisterCount As Integer
'    Public FloatRegisterGetter() As Func(Of Double)
'    Public FloatRegisterSetter() As Action(Of Double)
'    Public RegisterCount As Integer
'    Public RegisterName() As String
'    Public RegisterID As Dictionary(Of String, Integer)

'    Public InstructionCount As Integer
'    Public InstructionExecuter() As Action(Of ParamsInfo())
'    Public InstructionName() As String
'    Public InstructionID As Dictionary(Of String, Integer)

'    Public _DebugLevel As Integer = 0, _InterruptEnable As Boolean = False
'    Public _InterruptVector(255) As InterruptInfo

'    Public _HaltEvent As New ManualResetEvent(False)

'    Private _Memory As Memory

'    Public Overridable Sub ClockTick()

'    End Sub

'    Public Overridable Sub OnInterrput(id As Integer, action As Action)

'    End Sub

'    Public Overridable Sub Interrupt(id As Integer)

'    End Sub

'#Region "IDisposable Support"
'    Private disposedValue As Boolean ' 检测冗余的调用

'    ' IDisposable
'    Protected Overridable Sub Dispose(disposing As Boolean)
'        If Not Me.disposedValue Then
'            If disposing Then
'                ' TODO:  释放托管状态(托管对象)。
'            End If

'            ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
'            ' TODO:  将大型字段设置为 null。
'        End If
'        Me.disposedValue = True
'    End Sub

'    ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
'    'Protected Overrides Sub Finalize()
'    '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
'    '    Dispose(False)
'    '    MyBase.Finalize()
'    'End Sub

'    ' Visual Basic 添加此代码是为了正确实现可处置模式。
'    Public Sub Dispose() Implements IDisposable.Dispose
'        ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
'        Dispose(True)
'        GC.SuppressFinalize(Me)
'    End Sub
'#End Region

'End Class



'            {"dbg", Sub(data As ParameterInfo())
'                        _DebugLevel = getIntValue(data(0))
'                        Console.WriteLine("DebugLevel: {0}", _DebugLevel)
'                    End Sub},
'            {"dbg_prreg", Sub(data As ParameterInfo())
'                              Console.WriteLine("eax={0}, ebx={1}, ecx={2}, edx={3}, esp={4}, esfp={5}, eip={6}",
'                                                 eax, ebx, ecx, edx, esp, esfp, eip)
'                              Console.WriteLine("r0={0}, r1={1}, r2={2}, r3={3}, r4={4}, r5={5}, r6={6}, r7={7}",
'                                             r0, r1, r2, r3, r4, r5, r6, r7)
'                          End Sub},
'            {"dbg_prfreg", Sub(data As ParameterInfo())
'                               Console.WriteLine("f0={0}, f1={1}, f2={2}, f3={3}, f4={4}, f5={5}, f6={6}, f7={7}",
'                                                  f0, f1, f2, f3, f4, f5, f6, f7)
'                           End Sub},
'            {"dbg_pri", Sub(data As ParameterInfo())
'                            Console.Write(popi())
'                        End Sub},
'            {"dbg_prf", Sub(data As ParameterInfo())
'                            Console.Write(popf())
'                        End Sub},
'            {"dbg_prc", Sub(data As ParameterInfo())
'Dim ch = popi()
'                            Debug.Print(ch)
'                            Console.Write(vb.Chr(ch And &HFF))
'                        End Sub},
'            {"dbg_prs", Sub(data As ParameterInfo())
'Dim lastchar = popi()
'                            Do While lastchar <> 0
'                                Console.Write(vb.Chr(lastchar And &HFF))
'                                lastchar = popi()
'                            Loop
'                        End Sub},
'            {"dbg_ldc", Sub(data As ParameterInfo())
'                            Throw New CPUException("Instruction not implemented")
''If data = "" Then
''    data = " "
''End If
''pushi(Asc(data(0)))
'                        End Sub},
'            {"dbg_lds", Sub(data As ParameterInfo())
'                            Throw New CPUException("Instruction not implemented")
''pushi(0)
''For i = data.Length - 1 To 0 Step -1
''    pushi(Asc(data(i)))
''Next
'                        End Sub},
'            {"dbg_rdc", Sub(data As ParameterInfo())
'Dim k = Console.ReadKey(True)
'                            pushi(vb.Asc(k.KeyChar))
'                        End Sub},