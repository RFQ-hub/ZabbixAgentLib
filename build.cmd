@echo off
cls

paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

dir
dir packages
dir packages\build
dir packages\build\FAKE
dir packages\build\FAKE\tools\
packages\build\FAKE\tools\FAKE.exe build.fsx %* --nocache
