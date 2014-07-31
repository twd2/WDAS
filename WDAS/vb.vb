Public Class vb

    Public Shared CarriageReturn As String = Convert.ToChar(13)
    Public Shared LineFeed As String = Convert.ToChar(10)
    Public Shared CarriageReturnLineFeed As String = CarriageReturn + LineFeed
    Public Shared Tab As String = Convert.ToChar(9)

    Public Shared Function Asc(c As Char) As Byte
        Return Convert.ToByte(c)
    End Function

    Public Shared Function Chr(c As Byte) As Char
        Return Convert.ToChar(c)
    End Function

End Class
