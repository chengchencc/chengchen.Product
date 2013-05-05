@echo off

REM 
:: Usage: tarFile.bat all 20120725
REM
pushd %~dp0
set zip=..\7z\7z.exe
set md5=MD5.exe
set source=%1
set target=%2

set ftar=%target%.tar
set fgzip=%ftar%.gz
set fmd=%target%.md5

if exist %ftar% call del %ftar%
if exist %fgzip% call del %fgzip%
if exist %fmd% call del %fmd%

call %zip% a -ttar %ftar% %source%
call %zip% a -tgzip %fgzip% %ftar%
call %md5% %fgzip%>%fmd%

call del %ftar%

set tdir=c:\temp\output_baidu
if not exist %tdir% (call mkdir %tdir%)
call copy /Y %fgzip% %tdir%\%fgzip%
call copy /Y %fmd% %tdir%\%fmd%

call del %fgzip%
call del %fmd%
popd