@ECHO OFF
CALL NuGet.bat
nuget.exe restore "%MSIXSIGNERSLN%"

DEL "Installer.exe"
RMDIR /S /Q "MSIX Signer\Packages"
RMDIR /S /Q "Windows 10 Mobile"

"%MSIXSIGNERMSBUILD%" "%MSIXSIGNERSLN%" -r -p:Configuration=Release;AppxBundle=Never;Platform=ARM;AppxPackageDir="%~dp0MSIX Signer\Packages";AppxPackageSigningEnabled=False
"%MSIXSIGNERMSBUILD%" "%MSIXSIGNERSLN%" -r -p:Configuration=Release;AppxBundle=Never;Platform=x64;AppxPackageDir="%~dp0MSIX Signer\Packages";AppxPackageSigningEnabled=False
"%MSIXSIGNERMSBUILD%" "%MSIXSIGNERSLN%" -r -p:Configuration=Release;AppxBundle=Never;Platform=x86;AppxPackageDir="%~dp0MSIX Signer\Packages";AppxPackageSigningEnabled=False
"%MSIXSIGNERMSBUILD%" "%MSIXSIGNERSLN%" -r -p:Configuration=Release;AppxBundle=Never;Platform=ARM64;AppxPackageDir="%~dp0MSIX Signer\Packages";AppxPackageSigningEnabled=False

MKDIR "%~dp0Windows 10 Mobile"
MOVE "%~dp0MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM%MSIXSIGNERAFTER%\%MSIXSIGNERBEFORE%ARM.appx" "%~dp0Windows 10 Mobile\Package ARM.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM%MSIXSIGNERAFTER%\Dependencies\arm\Microsoft.VCLibs.ARM.%MSIXSIGNERVCLIBSVERSION%.appx" "Windows 10 Mobile"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM%MSIXSIGNERAFTER%\Dependencies\arm\Microsoft.NET.Native.Runtime.%MSIXSIGNERNETNATIVEVERSION%.appx" "Windows 10 Mobile"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM%MSIXSIGNERAFTER%\Dependencies\arm\Microsoft.NET.Native.Framework.%MSIXSIGNERNETNATIVEVERSION%.appx" "Windows 10 Mobile"

MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x64%MSIXSIGNERAFTER%\%MSIXSIGNERBEFORE%x64.appx" "MSIX Signer\Packages\Package x64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x64%MSIXSIGNERAFTER%\Dependencies\x64\Microsoft.VCLibs.x64.%MSIXSIGNERVCLIBSVERSION%.appx" "MSIX Signer\Packages\VCLibs x64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x64%MSIXSIGNERAFTER%\Dependencies\x64\Microsoft.NET.Native.Runtime.%MSIXSIGNERNETNATIVEVERSION%.appx" "MSIX Signer\Packages\.NET Native Runtime x64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x64%MSIXSIGNERAFTER%\Dependencies\x64\Microsoft.NET.Native.Framework.%MSIXSIGNERNETNATIVEVERSION%.appx" "MSIX Signer\Packages\.NET Native Framework x64.appx"

MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x86%MSIXSIGNERAFTER%\%MSIXSIGNERBEFORE%x86.appx" "MSIX Signer\Packages\Package x86.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x86%MSIXSIGNERAFTER%\Dependencies\x86\Microsoft.VCLibs.x86.%MSIXSIGNERVCLIBSVERSION%.appx" "MSIX Signer\Packages\VCLibs x86.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x86%MSIXSIGNERAFTER%\Dependencies\x86\Microsoft.NET.Native.Runtime.%MSIXSIGNERNETNATIVEVERSION%.appx" "MSIX Signer\Packages\.NET Native Runtime x86.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x86%MSIXSIGNERAFTER%\Dependencies\x86\Microsoft.NET.Native.Framework.%MSIXSIGNERNETNATIVEVERSION%.appx" "MSIX Signer\Packages\.NET Native Framework x86.appx"

MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM64%MSIXSIGNERAFTER%\%MSIXSIGNERBEFORE%ARM64.appx" "MSIX Signer\Packages\Package ARM64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM64%MSIXSIGNERAFTER%\Dependencies\arm64\Microsoft.VCLibs.ARM64.%MSIXSIGNERVCLIBSVERSION%.appx" "MSIX Signer\Packages\VCLibs ARM64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM64%MSIXSIGNERAFTER%\Dependencies\arm64\Microsoft.NET.Native.Runtime.%MSIXSIGNERNETNATIVEVERSIONARM64%.appx" "MSIX Signer\Packages\.NET Native Runtime ARM64.appx"
MOVE "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM64%MSIXSIGNERAFTER%\Dependencies\arm64\Microsoft.NET.Native.Framework.%MSIXSIGNERNETNATIVEVERSIONARM64%.appx" "MSIX Signer\Packages\.NET Native Framework ARM64.appx"

RMDIR /S /Q "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM%MSIXSIGNERAFTER%"
RMDIR /S /Q "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x64%MSIXSIGNERAFTER%"
RMDIR /S /Q "MSIX Signer\Packages\%MSIXSIGNERBEFORE%x86%MSIXSIGNERAFTER%"
RMDIR /S /Q "MSIX Signer\Packages\%MSIXSIGNERBEFORE%ARM64%MSIXSIGNERAFTER%"

"%MSIXSIGNERMSBUILD%" "MSIX Signer\MSIX Signer.vcxproj" -r -p:Configuration=Release;Platform=x86
COPY "MSIX Signer\Release\MSIX Signer.exe" "Installer.exe"
