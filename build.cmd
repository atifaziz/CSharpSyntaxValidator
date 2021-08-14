@echo off
setlocal
pushd "%~dp0"
call :main %*
set EXIT_CODE=%ERRORLEVEL%
popd
exit /b %EXIT_CODE%

:main
    dotnet restore ^
 && dotnet build --no-restore -c Debug ^
 && dotnet build --no-restore -c Release
exit /b %ERRORLEVEL%
