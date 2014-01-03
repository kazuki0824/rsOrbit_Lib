Module Util
    Enum evalStrategy
        WildCard = 0
        RegularExpression = 1
    End Enum
    Function Evaluation(ByVal src As String, pattern As String, Optional strategy As evalStrategy = evalStrategy.WildCard) As Boolean
        If (strategy = evalStrategy.RegularExpression) Then
            Return System.Text.RegularExpressions.Regex.IsMatch(src, pattern.Trim("|"c))
        Else
            Return src Like pattern
        End If
    End Function
End Module
