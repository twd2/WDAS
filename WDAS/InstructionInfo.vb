Imports System.IO
Imports System.Text

Public Class InstructionInfo


    Public ID As UShort
    Public Params() As ParameterInfo

    Sub New()

    End Sub

    Sub New(id As UShort, Params() As ParameterInfo)
        Me.ID = id
        Me.Params = Params
    End Sub

    Public Shared Function Parse(arch As CPU, str As String) As InstructionInfo
        Dim ii As New InstructionInfo()

        'split cmd data
        Dim indexofspace = str.IndexOf(" "c)
        Dim cmd = "", data = ""
        If indexofspace >= 0 Then
            cmd = str.Substring(0, indexofspace).Trim()
            data = str.Substring(indexofspace + 1).Trim()
        Else
            cmd = str
        End If

        If Not arch.InstructionID.ContainsKey(cmd) Then
            Throw New AssemblerException("Invaild Instruction """ & cmd & """")
        End If
        ii.ID = arch.InstructionID(cmd)

        If data <> "" Then
            Dim params = data.Split(","c)
            Dim paraminfos(params.Length - 1) As ParameterInfo
            For i = 0 To params.Length - 1
                paraminfos(i) = ParameterInfo.Parse(arch, params(i).Trim())
            Next
            ii.Params = paraminfos
        Else
            ii.Params = {}
        End If
        Return ii
    End Function

    Public Shadows Function ToString() As String
        Return ToString(New CPU)
    End Function

    Public Shadows Function ToString(arch As CPU) As String
        Dim sb As New StringBuilder()
        If ID >= arch.InstructionCount OrElse ID < 0 Then
            Throw New AssemblerException("Invaild Instrucion")
        End If
        Dim insname = arch.InstructionName(ID)
        If arch._Symbols IsNot Nothing AndAlso (insname.StartsWith("j") OrElse insname = "call") Then
            sb.Append(insname)
            If Params.Length > 0 Then
                sb.Append(" ")
                For i = 0 To Params.Length - 1
                    If Params(i).Type = ParameterInfo.ParameterType.Int Then
                        sb.Append(arch._Symbols(Params(i).Value))
                    Else
                        sb.Append(Params(i).ToString(arch))
                    End If
                    If i < Params.Length - 1 Then
                        sb.Append(", ")
                    End If
                Next
            End If
        Else
            sb.Append(insname)
            If Params.Length > 0 Then
                sb.Append(" ")
                For i = 0 To Params.Length - 1
                    sb.Append(Params(i).ToString(arch))
                    If i < Params.Length - 1 Then
                        sb.Append(", ")
                    End If
                Next
            End If
        End If
        Return sb.ToString()
    End Function

    Public Sub Write(s As Stream)
        'ID
        BinaryCodeHelper.WriteInt16LE(s, ID)
        'Param count
        s.WriteByte(Params.Length And &HFF)
        For i = 0 To Params.Length - 1
            Params(i).Write(s)
        Next
    End Sub

    Public Shared Function Read(s As Stream) As InstructionInfo
        Dim ii As New InstructionInfo
        ii.ID = BinaryCodeHelper.ReadInt16LE(s)
        Dim pcount = s.ReadByte()
        ReDim ii.Params(pcount - 1)
        For i = 0 To pcount - 1
            ii.Params(i) = ParameterInfo.Read(s)
        Next
        Return ii
    End Function

End Class
