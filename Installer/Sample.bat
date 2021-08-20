@ECHO OFF

REM Configuration

REM MSBuild from Visual Studio (can probably leave unchanged unless not using Community 2019)
SET MSIXSIGNERMSBUILD=D:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe

REM Path to your app .sln file
SET MSIXSIGNERSLN=D:\Repos\yoshiask\FluentStore\FluentStore.sln

REM Name of package output before architecture (see Visual Studio folder output after making app packages)
SET MSIXSIGNERBEFORE=FluentStore_0.1.0.0_

REM Name of package output after architecture (see Visual Studio folder output after making app packages)
SET MSIXSIGNERAFTER=_Test

REM Version of Microsoft.VCLibs package (see Visual Studio folder output after making app packages)
SET MSIXSIGNERVCLIBSVERSION=14.00

REM Version of Microsoft.NET.Native packages on non-ARM64 (see Visual Studio folder output after making app packages)
SET MSIXSIGNERNETNATIVEVERSION=2.2

REM Version of Microsoft.NET.Native packages on ARM64 (different option is available if targeting both old Windows versions and ARM64, but must be set regardless)
SET MSIXSIGNERNETNATIVEVERSIONARM64=2.2

REM End of configuration

CALL Generate.bat
