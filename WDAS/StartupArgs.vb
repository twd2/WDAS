Public Class StartupArgs

    Public isRun As Boolean = False
    Public isAssemble As Boolean = False
    Public isDisassemble As Boolean = False
    Public InputFilename As String = ""
    Public OutputFilename As String = ""
    Public HDDFilename As String = "hdd.txt"
    Public SymbolFilename As String = ""
    Public Debug As Integer = 0
    Public Frequency As UInteger = UInteger.MaxValue
    Public Memsize As Integer = 1024 * 2
    Public HDDsize As Integer = 1024 * 1024
    Public GenCppCode As Boolean = False

    Public Shared Function Parse(args As String()) As StartupArgs
        If args.Length = 0 Then
            Throw New ArgumentException("No Arg")
        End If
        Dim sa As New StartupArgs
        Dim i = 0
        Do While i < args.Length
            Dim arg = args(i)
            If arg.StartsWith("--") Then
                Select Case arg.Substring(2).ToLower()
                    Case "debug"
                        Dim dbg As Integer
                        If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), dbg) Then
                            i += 1
                        Else
                            dbg = 1
                        End If
                        sa.Debug = dbg
                    Case "freq"
                        Dim f As Integer
                        If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), f) Then
                            sa.Frequency = f
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "mem"
                        Dim m As Integer
                        If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), m) Then
                            sa.Memsize = m
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "run"
                        sa.isRun = True
                    Case "assemble"
                        sa.isAssemble = True
                    Case "disassemble"
                        sa.isDisassemble = True
                    Case "symbols"
                        If i + 1 < args.Length Then
                            sa.SymbolFilename = args(i + 1)
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "hdd"
                        If i + 1 < args.Length Then
                            sa.HDDFilename = args(i + 1)
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "hddsize"
                        Dim m As Integer
                        If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), m) Then
                            sa.HDDsize = m
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "output"
                        If i + 1 < args.Length Then
                            sa.OutputFilename = args(i + 1)
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "input"
                        If i + 1 < args.Length Then
                            sa.InputFilename = args(i + 1)
                            i += 1
                        Else
                            Throw New ArgumentException("Wrong Arg """ & arg & """")
                        End If
                    Case "gencppcode"
                        sa.GenCppCode = True
                    Case Else
                        Throw New ArgumentException("Wrong Arg """ & arg & """")
                End Select
            ElseIf arg.StartsWith("-") Then
                For j = 1 To arg.Length - 1
                    Dim ch = arg.ToLower()(j)
                    Select Case ch
                        Case "d"c
                            Dim dbg As Integer
                            If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), dbg) Then
                                i += 1
                            Else
                                dbg = 1
                            End If
                            sa.Debug = dbg
                        Case "f"c
                            Dim f As Integer
                            If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), f) Then
                                sa.Frequency = f
                                i += 1
                            End If
                        Case "m"c
                            Dim m As Integer
                            If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), m) Then
                                sa.Memsize = m
                                i += 1
                            End If
                        Case "r"c
                            sa.isRun = True
                        Case "a"c
                            sa.isAssemble = True
                        Case "s"c
                            sa.isDisassemble = True
                        Case "h"c
                            If i + 1 < args.Length Then
                                sa.HDDFilename = args(i + 1)
                                i += 1
                            Else
                                Throw New ArgumentException("Wrong Arg """ & arg & """")
                            End If
                        Case "z"c
                            Dim m As Integer
                            If i + 1 < args.Length AndAlso Integer.TryParse(args(i + 1), m) Then
                                sa.HDDsize = m
                                i += 1
                            Else
                                Throw New ArgumentException("Wrong Arg """ & arg & """")
                            End If
                        Case "o"c
                            If i + 1 < args.Length Then
                                sa.OutputFilename = args(i + 1)
                                i += 1
                            Else
                                Throw New ArgumentException("Wrong Arg """ & arg & """")
                            End If
                        Case "i"c
                            If i + 1 < args.Length Then
                                sa.InputFilename = args(i + 1)
                                i += 1
                            Else
                                Throw New ArgumentException("Wrong Arg """ & arg & """")
                            End If
                        Case Else
                            Throw New ArgumentException("Wrong Arg """ & ch & """")
                    End Select
                Next
            Else
                Throw New ArgumentException("Wrong Arg """ & arg & """")
            End If

            i += 1
        Loop
        Return sa
    End Function

End Class
