SET PATH=%PATH%;C:\Windows\Microsoft.NET\Framework\v4.0.30319

installutil /u "%~dp0Middleman.Service.exe"

NET STOP middleman

