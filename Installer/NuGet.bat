@ECHO OFF
IF NOT EXIST nuget.exe (
    curl https://dist.nuget.org/win-x86-commandline/latest/nuget.exe > nuget.exe
)
