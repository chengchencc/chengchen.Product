@echo off
pushd %~dp0

set gen=..\core\lib\protobuf\GenDotNetCodes.bat

if not exist %gen% echo the generator exe can not be found, please make sure your codes and your submodule codes are the latest.&goto :end

set protoDir=..\src\kolh.Domain.PbModels\Protos
set targetDir=..\src\kolh.Domain.PbModels


for /f %%i in ('echo %protoDir%') do call set protoDir=%%~fi

for /f %%i in ('echo %targetDir%') do call set targetDir=%%~fi

call "%gen%" %protoDir% %targetDir%
