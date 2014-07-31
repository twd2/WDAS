Partial Public Class CPU

    Private _getIntRegister As Dictionary(Of String, Func(Of Long)),
          _setIntRegister As Dictionary(Of String, Action(Of Long)),
          _getFloatRegister As Dictionary(Of String, Func(Of Double)),
          _setFloatRegister As Dictionary(Of String, Action(Of Double))

    Public IntRegisterOffset As Integer
    Public IntRegisterCount As Integer
    Public IntRegisterGetter() As Func(Of Long)
    Public IntRegisterSetter() As Action(Of Long)
    Public FloatRegisterOffset As Integer
    Public FloatRegisterCount As Integer
    Public FloatRegisterGetter() As Func(Of Double)
    Public FloatRegisterSetter() As Action(Of Double)
    Public RegisterCount As Integer
    Public RegisterName() As String
    Public RegisterID As Dictionary(Of String, Integer)

    Private ebp_id, esp_id As Integer

    Public eax, ebx, ecx, edx, ebp, esp, ebfp, esfp, eip As Long
    Public r0, r1, r2, r3, r4, r5, r6, r7 As Long
    Public f0, f1, f2, f3, f4, f5, f6, f7 As Double

    Private Sub initRegister()
        initIntRegister()
        initFloatRegister()
        MakeRegisterCache()
    End Sub

    Private Sub initIntRegister()
        _getIntRegister = New Dictionary(Of String, Func(Of Long)) From {
            {"eax", Function() As Long
                        Return eax
                    End Function},
            {"ax", Function() As Long
                       Return eax And &HFFFF
                   End Function},
            {"al", Function() As Long
                       Return eax And &HFF
                   End Function},
            {"ah", Function() As Long
                       Return (eax And &HFF00) >> 8
                   End Function},
            {"ebx", Function() As Long
                        Return eax
                    End Function},
            {"bx", Function() As Long
                       Return ebx And &HFFFF
                   End Function},
            {"bl", Function() As Long
                       Return ebx And &HFF
                   End Function},
            {"bh", Function() As Long
                       Return (ebx And &HFF00) >> 8
                   End Function},
            {"ecx", Function() As Long
                        Return ecx
                    End Function},
            {"cx", Function() As Long
                       Return ecx And &HFFFF
                   End Function},
            {"cl", Function() As Long
                       Return ecx And &HFF
                   End Function},
            {"ch", Function() As Long
                       Return (ecx And &HFF00) >> 8
                   End Function},
            {"edx", Function() As Long
                        Return edx
                    End Function},
            {"dx", Function() As Long
                       Return edx And &HFFFF
                   End Function},
            {"dl", Function() As Long
                       Return edx And &HFF
                   End Function},
            {"dh", Function() As Long
                       Return (edx And &HFF00) >> 8
                   End Function},
            {"esp", Function() As Long
                        Return esp
                    End Function},
            {"sp", Function() As Long
                       Return esp And &HFFFF
                   End Function},
            {"ebp", Function() As Long
                        Return ebp
                    End Function},
            {"bp", Function() As Long
                       Return ebp And &HFFFF
                   End Function},
            {"esfp", Function() As Long
                         Return esfp
                     End Function},
            {"sfp", Function() As Long
                        Return esfp And &HFFFF
                    End Function},
            {"ebfp", Function() As Long
                         Return esfp
                     End Function},
            {"bfp", Function() As Long
                        Return esfp And &HFFFF
                    End Function},
            {"eip", Function() As Long
                        Return eip
                    End Function},
            {"ip", Function() As Long
                       Return eip And &HFFFF
                   End Function},
            {"r0", Function() As Long
                       Return r0
                   End Function},
            {"r1", Function() As Long
                       Return r1
                   End Function},
            {"r2", Function() As Long
                       Return r2
                   End Function},
            {"r3", Function() As Long
                       Return r3
                   End Function},
            {"r4", Function() As Long
                       Return r4
                   End Function},
            {"r5", Function() As Long
                       Return r5
                   End Function},
            {"r6", Function() As Long
                       Return r6
                   End Function},
            {"r7", Function() As Long
                       Return r7
                   End Function}
        }
        _setIntRegister = New Dictionary(Of String, Action(Of Long)) From {
           {"eax", Sub(data As Long)
                       eax = data
                   End Sub},
           {"ax", Sub(data As Long)
                      eax = (eax And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"al", Sub(data As Long)
                      eax = (eax And &HFFFFFFFFFFFFFF00) Or (data And &HFF)
                  End Sub},
           {"ah", Sub(data As Long)
                      eax = (eax And &HFFFFFFFFFFFF00FF) Or ((data And &HFF) << 8)
                  End Sub},
           {"ebx", Sub(data As Long)
                       ebx = data
                   End Sub},
           {"bx", Sub(data As Long)
                      ebx = (ebx And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"bl", Sub(data As Long)
                      ebx = (ebx And &HFFFFFFFFFFFFFF00) Or (data And &HFF)
                  End Sub},
           {"bh", Sub(data As Long)
                      ebx = (ebx And &HFFFFFFFFFFFF00FF) Or ((data And &HFF) << 8)
                  End Sub},
           {"ecx", Sub(data As Long)
                       ecx = data
                   End Sub},
           {"cx", Sub(data As Long)
                      ecx = (ecx And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"cl", Sub(data As Long)
                      ecx = (ecx And &HFFFFFFFFFFFFFF00) Or (data And &HFF)
                  End Sub},
           {"ch", Sub(data As Long)
                      ecx = (ecx And &HFFFFFFFFFFFF00FF) Or ((data And &HFF) << 8)
                  End Sub},
           {"edx", Sub(data As Long)
                       edx = data
                   End Sub},
           {"dx", Sub(data As Long)
                      edx = (edx And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"dl", Sub(data As Long)
                      edx = (edx And &HFFFFFFFFFFFFFF00) Or (data And &HFF)
                  End Sub},
           {"dh", Sub(data As Long)
                      edx = (edx And &HFFFFFFFFFFFF00FF) Or ((data And &HFF) << 8)
                  End Sub},
           {"esp", Sub(data As Long)
                       esp = data
                   End Sub},
           {"sp", Sub(data As Long)
                      esp = (esp And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"ebp", Sub(data As Long)
                       ebp = data
                   End Sub},
           {"bp", Sub(data As Long)
                      ebp = (ebp And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"esfp", Sub(data As Long)
                        esfp = data
                    End Sub},
           {"sfp", Sub(data As Long)
                       esfp = (esfp And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                   End Sub},
           {"ebfp", Sub(data As Long)
                        ebfp = data
                    End Sub},
           {"bfp", Sub(data As Long)
                       ebfp = (ebfp And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                   End Sub},
           {"eip", Sub(data As Long)
                       eip = data
                   End Sub},
           {"ip", Sub(data As Long)
                      eip = (eip And &HFFFFFFFF0000FFFF) Or ((data And &HFFFF) << 16)
                  End Sub},
           {"r0", Sub(data As Long)
                      r0 = data
                  End Sub},
           {"r1", Sub(data As Long)
                      r1 = data
                  End Sub},
           {"r2", Sub(data As Long)
                      r2 = data
                  End Sub},
           {"r3", Sub(data As Long)
                      r3 = data
                  End Sub},
           {"r4", Sub(data As Long)
                      r4 = data
                  End Sub},
           {"r5", Sub(data As Long)
                      r5 = data
                  End Sub},
           {"r6", Sub(data As Long)
                      r6 = data
                  End Sub},
           {"r7", Sub(data As Long)
                      r7 = data
                  End Sub}
       }
    End Sub

    Private Sub initFloatRegister()
        _getFloatRegister = New Dictionary(Of String, Func(Of Double)) From {
            {"f0", Function() As Double
                       Return f0
                   End Function},
            {"f1", Function() As Double
                       Return f1
                   End Function},
            {"f2", Function() As Double
                       Return f2
                   End Function},
            {"f3", Function() As Double
                       Return f3
                   End Function},
            {"f4", Function() As Double
                       Return f4
                   End Function},
            {"f5", Function() As Double
                       Return f5
                   End Function},
            {"f6", Function() As Double
                       Return f6
                   End Function},
            {"f7", Function() As Double
                       Return f7
                   End Function}
        }
        _setFloatRegister = New Dictionary(Of String, Action(Of Double)) From {
           {"f0", Sub(data As Double)
                      f0 = data
                  End Sub},
           {"f1", Sub(data As Double)
                      f1 = data
                  End Sub},
           {"f2", Sub(data As Double)
                      f2 = data
                  End Sub},
           {"f3", Sub(data As Double)
                      f3 = data
                  End Sub},
           {"f4", Sub(data As Double)
                      f4 = data
                  End Sub},
           {"f5", Sub(data As Double)
                      f5 = data
                  End Sub},
           {"f6", Sub(data As Double)
                      f6 = data
                  End Sub},
           {"f7", Sub(data As Double)
                      f7 = data
                  End Sub}
       }
    End Sub

    Public Sub MakeRegisterCache()
        IntRegisterCount = _getIntRegister.Count
        IntRegisterGetter = _getIntRegister.Values().ToArray()
        IntRegisterSetter = _setIntRegister.Values().ToArray()

        FloatRegisterCount = _getFloatRegister.Count
        FloatRegisterGetter = _getFloatRegister.Values().ToArray()
        FloatRegisterSetter = _setFloatRegister.Values().ToArray()

        RegisterCount = IntRegisterCount + FloatRegisterCount
        ReDim RegisterName(IntRegisterCount + FloatRegisterCount - 1)
        Array.Copy(_getIntRegister.Keys().ToArray(), RegisterName, IntRegisterCount)
        Array.Copy(_getFloatRegister.Keys().ToArray(), 0, RegisterName, IntRegisterCount, FloatRegisterCount)

        RegisterID = New Dictionary(Of String, Integer)
        For i = 0 To RegisterName.Length - 1
            RegisterID.Add(RegisterName(i), i)
        Next

        IntRegisterOffset = 0
        FloatRegisterOffset = IntRegisterCount

        ebp_id = RegisterID("ebp")
        esp_id = RegisterID("esp")
    End Sub

    Public Sub PrintRegisters()
        Console.WriteLine("Registers:")
        Console.WriteLine("eax={0}, ebx={1}, ecx={2}, edx={3}, ebp={4}, esp={5}, ebfp={6}, esfp={7}, eip={8}",
                                                eax, ebx, ecx, edx, ebp, esp, ebfp, esfp, eip)
        Console.WriteLine("r0={0}, r1={1}, r2={2}, r3={3}, r4={4}, r5={5}, r6={6}, r7={7}",
                       r0, r1, r2, r3, r4, r5, r6, r7)
        Console.WriteLine("f0={0}, f1={1}, f2={2}, f3={3}, f4={4}, f5={5}, f6={6}, f7={7}",
                                                f0, f1, f2, f3, f4, f5, f6, f7)
    End Sub

End Class
