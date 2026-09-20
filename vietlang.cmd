@echo off
setlocal

set "VL=%~dp0VietLang"

if "%~1"=="" (
  echo VietLang v0.1 — Ngon ngu lap trinh tieng Viet
  echo.
  echo Cach dung:
  echo   vietlang ^<file.vl^>          Chay chuong trinh .vl
  echo   vietlang test                Chay bo tu kiem tra
  echo.
  echo Vi du:
  echo   vietlang demo\bai_01.vl
  echo   vietlang test
  echo.
  goto :eof
)

if /i "%~1"=="test" (
  dotnet run --project "%VL%" -- test
  exit /b %errorlevel%
)

dotnet run --project "%VL%" -- %*