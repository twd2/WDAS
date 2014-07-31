Public Class CPUException
    Inherits Exception

    Private Shadows Message As String

    Sub New(Message As String)
        MyBase.New(Message)
        Me.Message = Message
    End Sub

End Class
