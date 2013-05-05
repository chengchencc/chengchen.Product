@echo off 
pushd %~dp0

set ftpIP=211.151.180.86
set ftpUser=ylFtpUser
set ftpPass=uajsgsil3uto

:: copy daily to itest.kk570.com
set tdir=c:\temp\output_baidu
if not exist %tdir% (call mkdir %tdir%)

::call robocopy %1 %tdir% /MIR /NDL /NFL
call ncftpput.exe -R -v -u %ftpUser% -p %ftpPass% %ftpIP% / %tdir%

call rmdir /s /q %tdir%
popd

goto :eof