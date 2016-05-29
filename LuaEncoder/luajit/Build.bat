@echo off
cd /d %~dp0

if not exist dir (mkdir jit)
if not exist Out (mkdir Out)

xcopy /Y /D ..\..\..\Luajit\jit jit
setlocal enabledelayedexpansion

for /r %%i in (*.lua) do (
 set v=%%~dpi  
 call :loop
 set v=!v:%~dp0=!
 if not exist %~dp0out\!v! (mkdir %~dp0Out\!v!)
 )

for /r %%i in (*.lua) do (
 set v=%%i 
 set v=!v:%~dp0=! 
 call :loop
 ..\..\..\Luajit\luajit.exe -b -g !v! Out\!v!.bytes 
)

rd /s/q jit
rd /s/q .\Out\jit\
setlocal disabledelayedexpansion

:loop 
if "!v:~-1!"==" " set "v=!v:~0,-1!" & goto loop 


