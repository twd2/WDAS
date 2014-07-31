Public Class Memory

    Public _Size As Integer
    Public intdata As Long()
    Public floatdata As Double()
    Public program As InstructionInfo()
    Public _MaxAddress As Integer
    Public _IntOffset As Integer
    Public _FloatOffset As Integer

    Sub New(Optional size As Integer = 1024 * 1024)
        _Size = size
        _MaxAddress = size * 2
        _IntOffset = 0
        _FloatOffset = _Size
        ReDim intdata(size - 1)
        ReDim floatdata(size - 1)
        ReDim program(size - 1)
    End Sub

End Class
