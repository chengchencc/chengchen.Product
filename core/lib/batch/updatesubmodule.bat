@echo off

set currentDirectory=%1
set ifQuiet=%2

set quietParam=
if not defined currentDirectory goto :eof
if not defined ifQuiet (set quietParam=
	) else (
	set quietParam=-q
	)

set currentDirectory=%currentDirectory:/=\%

pushd %currentDirectory%

call git submodule %quietParam% foreach git pull %quietParam% origin master

popd