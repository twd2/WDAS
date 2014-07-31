Imports System.IO

Public Class ParameterInfo

    Public Enum ParameterType
        Register
        Int
        Float
    End Enum

    Public Type As ParameterType
    Public Value As Long
    Public Value_Float As Double

    Public Shared Function Parse(arch As CPU, str As String) As ParameterInfo
        Dim pi As New ParameterInfo()
        Dim int As Long, flo As Double
        If Long.TryParse(str, int) Then
            pi.Type = ParameterType.Int
            pi.Value = int
        ElseIf Double.TryParse(str, flo) Then
            pi.Type = ParameterType.Float
            pi.Value_Float = flo
        Else
            pi.Type = ParameterType.Register
            If Not arch.RegisterID.ContainsKey(str) Then
                Throw New AssemblerException("Invaild Register """ & str & """")
            End If
            pi.Value = arch.RegisterID(str)
        End If
        Return pi
    End Function

    Public Shadows Function ToString() As String
        Return ToString(New CPU)
    End Function

    Public Shadows Function ToString(arch As CPU) As String
        If Type = ParameterType.Float Then
            Return Value_Float.ToString()
        ElseIf Type = ParameterType.Int Then
            Return Value.ToString()
        Else
            If Value >= arch.RegisterCount OrElse Value < 0 Then
                Throw New AssemblerException("Invaild Parameter")
            End If
            Return arch.RegisterName(Value)
        End If
    End Function

    Public Sub Write(s As Stream)
        s.WriteByte(Type And &HFF)
        If Type = ParameterType.Float Then
            BinaryCodeHelper.WriteDoubleLE(s, Value_Float)
        Else
            BinaryCodeHelper.WriteInt64LE(s, Value)
        End If
    End Sub

    Public Shared Function Read(s As Stream) As ParameterInfo
        Dim pi As New ParameterInfo()
        pi.Type = s.ReadByte()
        If pi.Type = ParameterType.Float Then
            pi.Value_Float = BinaryCodeHelper.ReadDoubleLE(s)
        Else
            pi.Value = BinaryCodeHelper.ReadInt64LE(s)
        End If
        Return pi
    End Function

End Class
