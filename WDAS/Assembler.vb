Imports System.Text

Public Class Assembler

    Public Shared Function Assemble(arch As CPU, prog As String, ByRef out_symbols As Dictionary(Of String, Integer)) As InstructionInfo()
        Dim lines = prog.Split({vb.CarriageReturnLineFeed, vb.CarriageReturn, vb.LineFeed}, StringSplitOptions.None)
        Dim lis As New List(Of LineInfo)
        For i = 0 To lines.Length - 1
            lis.Add(New LineInfo() With {.LineID = i + 1, .Code = lines(i)})
        Next
        DropComment(lis)
        DropNull(lis)
        Dim symbols = ProcessJMP(lis)
        out_symbols = symbols
        NullToNOP(lis)
        Dim ins(lis.Count - 1) As InstructionInfo

        For i = 0 To lis.Count - 1
            Try
                ins(i) = InstructionInfo.Parse(arch, lis(i).Code)
            Catch ex As AssemblerException
                ex.code = lis(i)
                Throw
            End Try
        Next
        Return ins
    End Function

    Public Shared Function Disassemble(arch As CPU, prog As InstructionInfo(), Optional symbols As Dictionary(Of Integer, String) = Nothing) As String
        Dim sb As New StringBuilder()
        For i = 0 To prog.Length - 1
            sb.AppendLine(prog(i).ToString(arch))
        Next
        Return sb.ToString()
    End Function

    Private Shared Sub DropComment(lines As List(Of LineInfo))
        For i = 0 To lines.Count - 1
            Dim line = lines(i).Code
            Dim indexof = line.IndexOf(";"c)
            Dim label = "", cmd = ""
            If indexof >= 0 Then
                lines(i).Code = line.Substring(0, indexof)
            Else
                lines(i).Code = line
            End If
        Next
    End Sub

    Private Shared Sub DropNull(lines As List(Of LineInfo))
        lines.RemoveAll(Function(li As LineInfo)
                            Return li.Code.Trim() = ""
                        End Function)
        'Dim l As New List(Of String)
        'For i = 0 To lines.Count - 1
        '    Dim line = Trim(lines(i).Code)
        '    If line <> "" Then
        '        l.Add(line)
        '    End If
        'Next
        'Return l.ToArray()
    End Sub

    Private Shared Sub NullToNOP(lines As List(Of LineInfo))
        For i = 0 To lines.Count - 1
            Dim line = lines(i).Code
            If line = "" Then
                lines(i).Code = "nop"
            End If
        Next
    End Sub

    Private Shared Function ProcessJMP(lines As List(Of LineInfo)) As Dictionary(Of String, Integer)
        Dim labels As New Dictionary(Of String, Integer)

        'find labels
        For i = 0 To lines.Count - 1
            Dim line = lines(i).Code
            Dim indexof = line.IndexOf(":"c)
            Dim label = "", cmd = ""
            If indexof >= 0 Then
                label = line.Substring(0, indexof).Trim()
                cmd = line.Substring(indexof + 1).Trim()
                If label = "" OrElse labels.ContainsKey(label) Then
                    Throw New AssemblerException("Invaild Label", lines(i))
                End If
                labels.Add(label, lines(i).LineID)
                lines(i).Code = cmd
            End If
        Next

        DropNull(lines)

        Dim jmplabels As New Dictionary(Of String, Integer)
        For Each k In labels.Keys
            jmplabels.Add(k, FindJmpID(lines, labels(k)))
        Next

        'jmp label to address
        For i = 0 To lines.Count - 1
            Dim line = lines(i).Code
            If line.ToLower().StartsWith("j") OrElse line.ToLower().StartsWith("call") Then
                Dim indexof = line.IndexOf(" "c)
                Dim cmd = "", label = ""
                If indexof >= 0 Then
                    cmd = line.Substring(0, indexof).Trim()
                    label = line.Substring(indexof + 1).Trim()
                    Dim x As Integer
                    If labels.ContainsKey(label) Then
                        lines(i).Code = cmd & " " & jmplabels(label) 'labels(label)
                    ElseIf label = "$" Then
                        lines(i).Code = cmd & " " & i
                    ElseIf Integer.TryParse(label, x) Then
                        'do nothing
                    Else
                        Throw New AssemblerException("Invaild Label, may call to an undefined function", lines(i))
                    End If
                Else
                    Throw New AssemblerException("Invaild JMP instruction: No label", lines(i))
                End If
            End If
        Next
        Return jmplabels
    End Function

    Private Shared Function FindJmpID(lines As List(Of LineInfo), lineid As Integer)
        Return lines.FindIndex(Function(li As LineInfo)
                                   Return li.LineID >= lineid
                               End Function)
    End Function

End Class
