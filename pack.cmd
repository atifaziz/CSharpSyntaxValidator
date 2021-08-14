@echo off
setlocal
pushd "%~dp0"
call :main %*
set EXIT_CODE=%ERRORLEVEL%
popd
exit /b %EXIT_CODE%

:main
setlocal
set VERSION_SUFFIX=
if not "%~1"=="" set VERSION_SUFFIX=--version-suffix %~1
call build && dotnet pack --no-build -c Release %VERSION_SUFFIX% src
exit /b %ERRORLEVEL%
