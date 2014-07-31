Public Class InterruptInfo

    Public Enum InterruptType
        inner
        outer
    End Enum

    Public _Type As InterruptType
    Public _inner_address As Long
    Public _outer_action As Action

    Sub New(Type As InterruptType)
        _Type = Type
    End Sub

End Class
