Imports System.IO
Imports System.Runtime.InteropServices

Public Class BinaryCodeHelper

    Public Shared Function Read(s As Stream) As InstructionInfo()
        Dim icount = ReadInt32LE(s)
        Dim ins(icount - 1) As InstructionInfo
        For i = 0 To icount - 1
            ins(i) = InstructionInfo.Read(s)
        Next
        Return ins
    End Function

    Public Shared Sub Write(s As Stream, ins As InstructionInfo())
        WriteInt32LE(s, ins.Length)
        For i = 0 To ins.Length - 1
            ins(i).Write(s)
        Next
    End Sub

    Public Shared Function ReadInt16LE(s As Stream) As Short
        Dim i As Short = 0
        i += CShort(s.ReadByte())
        i += CShort(s.ReadByte()) << 8
        Return i
    End Function

    Public Shared Sub WriteInt16LE(s As Stream, i As Short)
        s.WriteByte(i And &HFF)
        s.WriteByte(((i And &HFFFF) >> 8) And &HFF)
    End Sub

    Public Shared Function ReadInt64LE(s As Stream) As Long
        Dim i As Long = 0
        i += CLng(s.ReadByte())
        i += CLng(s.ReadByte()) << 8
        i += CLng(s.ReadByte()) << 16
        i += CLng(s.ReadByte()) << 24
        i += CLng(s.ReadByte()) << 32
        i += CLng(s.ReadByte()) << 40
        i += CLng(s.ReadByte()) << 48
        i += CLng(s.ReadByte()) << 56
        Return i
    End Function

    Public Shared Sub WriteInt64LE(s As Stream, i As Long)
        s.WriteByte(i And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 8) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 16) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 24) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 32) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 40) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 48) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 56) And &HFF)
    End Sub

    Public Shared Function ReadInt32LE(s As Stream) As Integer
        Dim i As Integer = 0
        i += CInt(s.ReadByte())
        i += CInt(s.ReadByte()) << 8
        i += CInt(s.ReadByte()) << 16
        i += CInt(s.ReadByte()) << 24
        Return i
    End Function

    Public Shared Sub WriteInt32LE(s As Stream, i As Integer)
        s.WriteByte(i And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 8) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 16) And &HFF)
        s.WriteByte(((i And &HFFFFFFFFFFFFFFFF) >> 24) And &HFF)
    End Sub

    Public Shared Sub WriteDoubleLE(s As Stream, i As Double)
        WriteInt64LE(s, DoubleToInt64(i))
    End Sub

    Public Shared Function ReadDoubleLE(s As Stream) As Double
        Return Int64ToDouble(ReadInt64LE(s))
    End Function

    Public Shared Function Int64ToDouble(i As Int64) As Double
        Dim float(0) As Double
        Marshal.Copy(Marshal.UnsafeAddrOfPinnedArrayElement({i}, 0), float, 0, 1)
        Return float(0)
    End Function

    Public Shared Function DoubleToInt64(f As Double) As Int64
        Dim int(0) As Int64
        Marshal.Copy(Marshal.UnsafeAddrOfPinnedArrayElement({f}, 0), int, 0, 1)
        Return int(0)
    End Function

End Class
