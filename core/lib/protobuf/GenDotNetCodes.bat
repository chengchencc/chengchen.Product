@echo off &setlocal enabledelayedexpansion
set in=%1
set pgen=%~dp0\ProtoGen.exe

set out=%2

if not exist %out% (
	md %out% 2>NUL 1>NUL
)

dir /s /b /a-d %in%\*.proto

for /f %%i in ('dir /s /b /a-d %in%\*.proto') do (
	call !pgen! -output_directory=%out% --include_imports=%in% --proto_path=%in% %%i
)

for /f %%i in ('dir /s /b /a-d %in%\*.proto') do (

set o=%%i
call set o1=%%o:%in%=%%
call set o1=%%o1:.proto=.cs%%


for /f %%j in ('dir /b %%i') do (
	set fileName=%%j
)
call set filename=%%filename:.proto=.cs%%
call set dest=%out%!o1!
call set destDir=%%dest:!filename!=%%
md !destDir! 2>NUL 1>NUL
move %out%\!filename! %out%!o1! >NUL
)
