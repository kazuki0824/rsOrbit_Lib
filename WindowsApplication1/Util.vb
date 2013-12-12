Module Util
    Enum evalStrategy
        WildCard
        RegularExpression = -1
    End Enum
    Function Evaluation(ByVal src As String, pattern As String, Optional strategy As evalStrategy = evalStrategy.WildCard) As String
        If (strategy = evalStrategy.RegularExpression) Then
            Return System.Text.RegularExpressions.Regex.IsMatch(src, pattern.Trim("|"))
        Else
            Return src Like pattern
        End If
    End Function
End Module
