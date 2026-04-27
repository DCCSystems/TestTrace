@echo off
setlocal
set "TESTTRACE_PROJECTS_ROOT=C:\Users\Dan\Desktop\DCC Systems Backup 17.04.26\sandbox-data\TestTraceProjects"
cd /d "%~dp0"
dotnet run --project "TestTrace V1.csproj"
