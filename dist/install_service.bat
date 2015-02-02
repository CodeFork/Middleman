SET PATH=%PATH%;C:\Windows\Microsoft.NET\Framework\v4.0.30319

installutil "%~dp0Middleman.Service.exe"

NET START middleman
