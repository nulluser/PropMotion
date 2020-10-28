@echo off

path %path%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319

del PropMotionSimulator.exe

echo Building

csc src\*.cs src\STL_Tools\*.cs /r:DLL\OpenTK.dll 

pause


