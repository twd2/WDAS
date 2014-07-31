Imports System.Threading
Imports System.IO
Imports System.Text

Module EntryPoint

    Dim m As Machine

    Private Sub PrintUsage()
        Console.WriteLine("Usage: MyASM -adfhimorsz" + vb.CarriageReturnLineFeed +
                           "    -a, --assemble:{0}{0}Assemble input file" + vb.CarriageReturnLineFeed +
                           "    -d, --debug=0/1/2/3:{0}Debug mode" + vb.CarriageReturnLineFeed +
                           "    -f, --freq=?:{0}{0}Virtual Machine clock frequency" + vb.CarriageReturnLineFeed +
                           "    -h, --hdd=?:{0}{0}Virtual Machine HDD file name" + vb.CarriageReturnLineFeed +
                           "    -i, --input=?:{0}{0}Input file name, ""-"" for standard input" + vb.CarriageReturnLineFeed +
                           "    -m, --mem=?:{0}{0}Virtual Machine memory size" + vb.CarriageReturnLineFeed +
                           "    -o, --output=?:{0}{0}Output file name, ""-"" for standard output" + vb.CarriageReturnLineFeed +
                           "    -r, --run:{0}{0}{0}Run input file" + vb.CarriageReturnLineFeed +
                           "    -s, --disassemble:{0}{0}Disassemble input file" + vb.CarriageReturnLineFeed +
                           "    --symbols:{0}{0}Symbols file name" + vb.CarriageReturnLineFeed +
                           "    -z, --hddsize=?:{0}{0}Virtual Machine HDD size", vb.Tab)
    End Sub

    Sub Main(args As String())
        Dim sa As StartupArgs

        Try
            sa = StartupArgs.Parse(args) '({"-ar", "-i", "a.asm", "--debug", "-o", "a.bin"}
        Catch ex As ArgumentException
            PrintUsage()
            Return
        End Try

        If sa.GenCppCode Then
            Dim helper As New CppHelper(New CPU)
            helper.GenCppCode()
        End If

        Dim ins() As InstructionInfo = Nothing
        Dim inputStream As Stream = Nothing, outputStream As Stream = Nothing
        If sa.InputFilename <> "" Then
            If sa.InputFilename = "-" Then
                inputStream = Console.OpenStandardInput()
            Else
                inputStream = New FileStream(sa.InputFilename, FileMode.Open, FileAccess.Read)
            End If
        End If

        If sa.OutputFilename <> "" Then
            If sa.OutputFilename = "-" Then
                outputStream = Console.OpenStandardOutput()
            Else
                outputStream = New FileStream(sa.OutputFilename, FileMode.Create, FileAccess.Write)
            End If
        End If

        If sa.isDisassemble AndAlso sa.isAssemble Then
            Console.WriteLine("Cannot assemble+disassemble")
            Return
        End If

        If sa.isAssemble Then
            Dim arch As New CPU
            Dim sr As New StreamReader(inputStream, Encoding.UTF8)
            Dim symbols As Dictionary(Of String, Integer) = Nothing
            Try
                ins = Assembler.Assemble(arch, sr.ReadToEnd(), symbols)
            Catch ex As AssemblerException
                ex.Print()
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine("Failed")
                Console.ForegroundColor = ConsoleColor.Gray
                Return
            Catch ex As Exception
                Console.WriteLine("Error: {0}", ex.Message)
                Return
            End Try
            If outputStream IsNot Nothing Then
                BinaryCodeHelper.Write(outputStream, ins)
            End If
            If sa.SymbolFilename <> "" Then
                Using sww As New StreamWriter(sa.SymbolFilename, False, Encoding.UTF8)
                    For Each k In symbols.Keys
                        sww.WriteLine("{0},{1}", k, symbols(k))
                    Next
                End Using
            End If
            outputStream.Flush()
            If outputStream IsNot Nothing Then
                outputStream.Close()
            End If
        End If

        If sa.isDisassemble Then
            Dim arch As New CPU
            If sa.SymbolFilename <> "" Then
                arch.LoadSymbols(sa.SymbolFilename)
            End If
            Try
                ins = BinaryCodeHelper.Read(inputStream)
                If outputStream IsNot Nothing Then
                    Dim sw As New StreamWriter(outputStream, Encoding.UTF8)
                    sw.Write(Assembler.Disassemble(arch, ins))
                    sw.Flush()
                    outputStream.Flush()
                    outputStream.Close()
                End If
            Catch ex As AssemblerException
                ex.Print()
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine("Failed")
                Console.ForegroundColor = ConsoleColor.Gray
                Return
            Catch ex As Exception
                Console.WriteLine("Error: {0}", ex.Message)
                Return
            End Try
        End If

        If sa.isRun Then
            If ins Is Nothing Then
                ins = BinaryCodeHelper.Read(inputStream)
            End If
            m = New Machine(sa.Frequency, sa.Memsize)
            If sa.SymbolFilename <> "" Then
                m._CPU.LoadSymbols(sa.SymbolFilename)
            End If
            m._HDD = New FileStream(sa.HDDFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite)
            m._HDD.Seek(sa.HDDsize - 1, SeekOrigin.Begin)
            m._HDD.WriteByte(0)
            m._HDD.Flush()
            m._HDD.Seek(0, SeekOrigin.Begin)
            m.Load(ins)
            AddHandler Console.CancelKeyPress, AddressOf m.CtrlC
            Dim sw As New Stopwatch()
            sw.Start()
            m.Start()
            m.Wait()
            m.Halt()
            sw.Stop()
            Console.WriteLine("Time usage: {0}ms", sw.ElapsedMilliseconds)
        End If
        If inputStream IsNot Nothing Then
            inputStream.Close()
        End If

        'Dim fn = "a.asm"
        'Dim freq = UInteger.MaxValue, memsize = 2048 'UInteger.MaxValue
        'If args.Length > 0 Then
        '    fn = args(0)
        'End If
        'If args.Length > 1 Then
        '    If args(1) <> "max" Then
        '        freq = Val(args(1))
        '    End If
        'End If
        'If args.Length > 1 Then
        '    memsize = Val(args(2))
        'End If
        'm = New Machine(freq, memsize)
        'm.LoadFile(fn)
        'm.Start()
        'Dim t As New Thread(AddressOf int_test)
        't.Start()
        'm.Wait()
        'm.Halt()
        'Console.ReadKey()
    End Sub

    Sub int_test()
        'Do
        'm._CPU.Interrupt(2)
        Thread.Sleep(100)
        'Loop
    End Sub

End Module
