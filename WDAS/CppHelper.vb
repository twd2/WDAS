Imports System.Text
Imports System.IO

Public Class CppHelper


    Private arch As CPU
    Private sb As New StringBuilder
    Private Implemention As New Dictionary(Of String, String())

    Private return_code As String = "return;"

    Sub New(arch As CPU)
        Me.arch = arch
    End Sub

    Public Sub LoadImplemention(path As String)
        Implemention.Clear()
        For Each fn In Directory.GetFiles(path)
            Dim fi As New FileInfo(fn)
            Dim k = fi.Name.Substring(0, fi.Name.Length - 4) ' - ".cpp"
            Implemention.Add(k, File.ReadAllLines(fn))
        Next
    End Sub

    Private Sub CopyCode(lines As String(), Optional space As String = "")
        For i = 0 To lines.Length - 1
            sb.AppendLine(space + lines(i))
        Next
    End Sub

    'Public Function ToCode() As String
    '    Return sb.ToString()
    'End Function

    Public Sub GenCppCode()
        'GenDefineCode(sb, arch)
        sb.AppendLine("#include ""CPU.hpp""")
        sb.AppendLine()
        Dim regname As New Dictionary(Of Integer, String)
        For i = 0 To arch.RegisterName.Length - 1
            Dim defname = String.Format("CPU_Reg_{0}", arch.RegisterName(i))
            sb.AppendLine(String.Format("#define {0} {1}", defname, i))
            regname.Add(i, defname)
            'File.WriteAllText(String.Format("cppimpl\CPU_Reg_{0}.cpp", arch.RegisterName(i)), "")
        Next
        sb.AppendLine()

        LoadImplemention("cppimpl\read_int_reg")
        return_code = "return 0;"
        sb.AppendLine("cpubasetype CPU::getIntRegisterValue(cpubasetype id)")
        sb.AppendLine("{")
        sb.AppendLine("    if (id == CPU_Reg_esp)")
        sb.AppendLine("    {")
        sb.AppendLine("        return esp;")
        sb.AppendLine("    }")
        sb.AppendLine("    if (id == CPU_Reg_ebp)")
        sb.AppendLine("    {")
        sb.AppendLine("        return ebp;")
        sb.AppendLine("    }")
        sb.AppendLine("    if (id >= CPU_Reg_f0) //is float register")
        sb.AppendLine("    {")
        sb.AppendLine("        " + return_code)
        sb.AppendLine("    }")
        GenSelectCode("id", regname, 0, arch.RegisterName.Length - 1, "    ")
        sb.AppendLine("}")

        LoadImplemention("cppimpl\write_int_reg")
        return_code = "return;"
        sb.AppendLine("void CPU::setIntRegisterValue(cpubasetype id, cpubasetype data)")
        sb.AppendLine("{")
        sb.AppendLine("    if (id == CPU_Reg_esp)")
        sb.AppendLine("    {")
        sb.AppendLine("        esp=data;")
        sb.AppendLine("        return;")
        sb.AppendLine("    }")
        sb.AppendLine("    if (id == CPU_Reg_ebp)")
        sb.AppendLine("    {")
        sb.AppendLine("        ebp=data;")
        sb.AppendLine("        return;")
        sb.AppendLine("    }")
        sb.AppendLine("    if (id >= CPU_Reg_f0) //is float register")
        sb.AppendLine("    {")
        sb.AppendLine("        " + return_code)
        sb.AppendLine("    }")
        GenSelectCode("id", regname, 0, arch.RegisterName.Length - 1, "    ")
        sb.AppendLine("}")

        LoadImplemention("cppimpl\read_float_reg")
        return_code = "return 0.0;"
        sb.AppendLine("double CPU::getFloatRegisterValue(cpubasetype id)")
        sb.AppendLine("{")
        sb.AppendLine("    if (id <= CPU_Reg_r7) //is int register")
        sb.AppendLine("    {")
        sb.AppendLine("        " + return_code)
        sb.AppendLine("    }")
        GenSelectCode("id", regname, 0, arch.RegisterName.Length - 1, "    ")
        sb.AppendLine("}")

        LoadImplemention("cppimpl\write_float_reg")
        return_code = "return;"
        sb.AppendLine("void CPU::setFloatRegisterValue(cpubasetype id, double data)")
        sb.AppendLine("{")
        sb.AppendLine("    if (id <= CPU_Reg_r7) //is int register")
        sb.AppendLine("    {")
        sb.AppendLine("        " + return_code)
        sb.AppendLine("    }")
        GenSelectCode("id", regname, 0, arch.RegisterName.Length - 1, "    ")
        sb.AppendLine("}")

        File.WriteAllText("cppimpl\CPU_Registers.cpp", sb.ToString())

        sb = New StringBuilder
        LoadImplemention("cppimpl\ins")
        sb.AppendLine("#include ""CPU.hpp""")
        sb.AppendLine()
        Dim insname As New Dictionary(Of Integer, String)
        For i = 0 To arch.InstructionName.Length - 1
            Dim defname = String.Format("CPU_Ins_{0}", arch.InstructionName(i))
            sb.AppendLine(String.Format("#define {0} {1}", defname, i))
            insname.Add(i, defname)
            'File.WriteAllText(String.Format("cppimpl\CPU_Ins_{0}.cpp", arch.InstructionName(i)), "")
        Next
        sb.AppendLine()
        sb.AppendLine("void CPU::executeInstruction(InstructionInfo& ii)")
        sb.AppendLine("{")
        sb.AppendLine("    unsigned short id = ii.ID;")
        GenSelectCode("id", insname, 0, arch.InstructionName.Length - 1, "    ")
        sb.AppendLine("}")
        File.WriteAllText("cppimpl\CPU_Instructions.cpp", sb.ToString())
    End Sub

    'Public Shared Sub GenDefineCode(sb As StringBuilder, arch As CPU)
    '    For i = 0 To arch.RegisterName.Length - 1
    '        sb.AppendLine(String.Format("#define CPU_Reg_{0} {1}", arch.RegisterName(i), i))
    '    Next
    '    For i = 0 To arch.InstructionName.Length - 1
    '        sb.AppendLine(String.Format("#define CPU_Ins_{0} {1}", arch.InstructionName(i), i))
    '    Next
    'End Sub

    Public Sub GenSelectCode(var As String, def As Dictionary(Of Integer, String), left As Integer, right As Integer, Optional space As String = "")
        If left > right Then
            Return
        End If
        If left = right Then
            sb.AppendLine(space + String.Format("//{0}", def(left)))
            If Implemention.ContainsKey(def(left)) Then
                CopyCode(Implemention(def(left)), space)
            Else
                Console.WriteLine("Warning: No implemention of {0}", def(left))
                CopyCode({return_code}, space)
            End If
            Return
        End If
        Dim middle = (left + right) \ 2
        sb.AppendLine(space + String.Format("if ({0} <= {1})", var, def(middle)))
        sb.AppendLine(space + "{")
        GenSelectCode(var, def, left, middle, space + "    ")
        sb.AppendLine(space + "}")
        sb.AppendLine(space + "else")
        sb.AppendLine(space + "{")
        GenSelectCode(var, def, middle + 1, right, space + "    ")
        sb.AppendLine(space + "}")
    End Sub

End Class
