Public Class AssemblerException
    Inherits Exception

    Private Shadows Message As String
    Public code As LineInfo = Nothing

    Sub New(Message As String, Optional code As LineInfo = Nothing)
        MyBase.New(Message)
        Me.Message = Message
        Me.code = code
    End Sub

    Public Sub Print()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("Assembler Exception: {0}", Message)
        Console.ForegroundColor = ConsoleColor.Gray
        If code IsNot Nothing Then
            Console.WriteLine("In line {0}: {1}", code.LineID, code.Code)
        End If
    End Sub

End Class
