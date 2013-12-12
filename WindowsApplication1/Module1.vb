﻿Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Linq
Imports System.Text

Namespace BrowserProxySetting
    Public Class BrowserProxySetting
        Public Structure INTERNET_PROXY_INFO
            Public dwAccessType As Integer
            Public proxy As IntPtr
            Public proxyBypass As IntPtr
        End Structure

        <DllImport("wininet.dll", SetLastError:=True)> _
        Private Shared Function InternetSetOption(hInternet As IntPtr, dwOption As Integer, lpBuffer As IntPtr, lpdwBufferLength As Integer) As Boolean
        End Function

        ''' <summary>
        ''' Proxy設定
        ''' </summary>
        ''' <param name="strProxy"></param>
        Public Shared Sub RefreshIESettings(strProxy As String)
            Const INTERNET_OPTION_PROXY As Integer = 38
            Const INTERNET_OPEN_TYPE_PROXY As Integer = 3
            Dim struct_IPI As INTERNET_PROXY_INFO

            ' Filling in structure
            struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_PROXY
            struct_IPI.proxy = Marshal.StringToHGlobalAnsi(strProxy)
            struct_IPI.proxyBypass = Marshal.StringToHGlobalAnsi("local")

            ' Allocating memory
            Dim intptrStruct As IntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(struct_IPI))

            ' Converting structure to IntPtr
            Marshal.StructureToPtr(struct_IPI, intptrStruct, True)
            Dim iReturn As Boolean = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(struct_IPI))
        End Sub
    End Class
End Namespace