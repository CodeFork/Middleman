@ECHO OFF

cls 
SET PATH=%PATH%;C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin

C:
cd %~dp0

rem makecert.exe -!
rem makecert.exe -?

del "*.cer"
del "*.pfx"
del "*.pvk"

cls

makecert -n "CN=Middleman.Server.RootCA_DO_NOT_TRUST" ^
         -cy authority ^
         -a sha1 ^
         -sv "Middleman.Server.RootCA.pvk" ^
         -r ^
         "Middleman.Server.RootCA.cer"
             
makecert -n "CN=localhost" ^
         -ic "Middleman.Server.RootCA.cer" ^
         -iv "Middleman.Server.RootCA.pvk" ^
         -a sha1 ^
         -sky exchange ^
         -pe ^
         -sv "Middleman.Server.Web.pvk" ^
         "Middleman.Server.Web.cer"

pvk2pfx -pvk "Middleman.Server.Web.pvk" -spc "Middleman.Server.Web.cer" -pfx "Middleman.Server.Web.pfx"

pause
