Option Explicit

Dim shell
Dim fso
Dim appRoot
Dim hostPath

Set shell = CreateObject("WScript.Shell")
Set fso   = CreateObject("Scripting.FileSystemObject")

appRoot  = fso.GetParentFolderName(WScript.ScriptFullName)
hostPath = fso.BuildPath(appRoot, "TodoDesk.exe")

If Not fso.FileExists(hostPath) Then
  MsgBox "TodoDesk.exe not found: " & hostPath, vbCritical, "Todo Desk"
  WScript.Quit 1
End If

shell.Run Chr(34) & hostPath & Chr(34), 1, False
