Imports System.Runtime.InteropServices
Imports System.Drawing

Module Shellmgr
    <DllImport("shell32.dll", CharSet:=CharSet.Auto)> Private Function ExtractIcon(ByVal hInst As IntPtr, ByVal lpszExeFileName As String, ByVal nIconIndex As Integer) As IntPtr
    End Function

    Public Function GetExtensionIcon(ByVal callHandle As IntPtr, ByVal exeStr As String) As Icon
        Const DefaultIconPath As String = "%SystemRoot%\System32\shell32.dll"
        Try
            '拡張子のチェック
            If exeStr Is Nothing OrElse exeStr.Length = 0 OrElse ".ico".Equals(exeStr, StringComparison.CurrentCultureIgnoreCase) Then
                '拡張子がicoの場合、アイコンは存在しない
                Return Nothing
            ElseIf ".exe".Equals(exeStr, StringComparison.CurrentCultureIgnoreCase) Then
                'exeアイコンは例外とする
                Return Icon.FromHandle(ExtractIcon(callHandle, DefaultIconPath, 2))
            End If
        Catch ex As Exception
            Return Nothing
        End Try

        Dim regkey As Microsoft.Win32.RegistryKey = Nothing
        Dim regDefaultIconKey As Microsoft.Win32.RegistryKey = Nothing
        Try
            '指定された拡張子のレジストリキーを開く
            regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(exeStr, False)
            If regkey Is Nothing Then
                Return Nothing
            End If

            Dim regStr As String = CStr(regkey.GetValue(""))
            regDefaultIconKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(regStr & "\DefaultIcon", False)
            If regDefaultIconKey Is Nothing Then
                regkey.Close()
                regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(regStr & "\CurVer", False)
                If regkey Is Nothing Then
                    Return Nothing
                End If

                regDefaultIconKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(CStr(regkey.GetValue("")) & "\DefaultIcon", False)
                If regDefaultIconKey Is Nothing Then
                    Return Nothing
                End If
            End If

            Dim exeIconPath As String() = CStr(regDefaultIconKey.GetValue("")).Split(New Char() {","c}, 2)
            If exeIconPath IsNot Nothing AndAlso exeIconPath.Length = 2 Then
                Return Icon.FromHandle(ExtractIcon(callHandle, exeIconPath(0).Replace("""", ""), CInt(exeIconPath(1))))
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        Finally
            '開放
            If regkey IsNot Nothing Then
                regkey.Close()
            End If
            If regDefaultIconKey IsNot Nothing Then
                regDefaultIconKey.Close()
            End If
        End Try
    End Function
End Module