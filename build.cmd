@echo off
setlocal
call "%VS100COMNTOOLS%vsvars32.bat"
msbuild Spider.sln /p:Configuration=%1
