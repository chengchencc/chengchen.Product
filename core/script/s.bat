@echo off
pushd %~dp0
set mode=%1
set dir=..\bin\%mode%
set s_dir=..\bin\%mode%\signed
set sn_exe=C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\x64\sn.exe
set snk=core.snk

if not exist %s_dir% mkdir %s_dir%

for /f %%a in (sign_config.txt) do (
	if exist %dir%\%%a.dll (
		call "%sn_exe%" -R %dir%\%%a.dll %snk%
		call copy /y %dir%\%%a.dll %s_dir%\%%a.dll
	)
 )

popd

:: .publickeytoken = (2F 6B 00 D3 B7 B7 9D 4C )                         // .z\V.4..