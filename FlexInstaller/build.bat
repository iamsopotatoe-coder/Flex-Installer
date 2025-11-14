@echo off
cls
echo FLEXINSTALLER BUILDER v3.0
echo ============================

set /p OUTPUT_NAME=Filename: 
if "%OUTPUT_NAME%"=="" set OUTPUT_NAME=Installer

for %%I in (csc.exe) do set CSC_PATH=%%~$PATH:I
if "%CSC_PATH%"=="" (
    for /d %%F in (C:\Windows\Microsoft.NET\Framework*) do (
        for /d %%V in (%%F\v*) do (
            if exist "%%V\csc.exe" set CSC_PATH=%%V\csc.exe
        )
    )
)

if not exist "%CSC_PATH%" (
    echo ERROR: C# compiler not found!
    echo.
    echo Download Microsoft Build Tools:
    echo https://aka.ms/vs/17/release/vs_buildtools.exe
    echo.
    echo Or install Visual Studio Community (free^):
    echo https://visualstudio.microsoft.com/downloads/
    goto :end
)

echo Building %OUTPUT_NAME%.exe...
if not exist output mkdir output

"%CSC_PATH%" /target:winexe /out:output\%OUTPUT_NAME%.exe /win32icon:src\installer.ico /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Net.Http.dll src\Program.cs src\MainForm.cs src\DownloadManager.cs src\InstallationManager.cs src\UninstallerForm.cs Config.cs >nul 2>&1

if exist output\%OUTPUT_NAME%.exe (
    echo BUILD SUCCESS: output\%OUTPUT_NAME%.exe
    for %%A in (output\%OUTPUT_NAME%.exe) do echo Size: %%~zA bytes
) else (
    echo BUILD FAILED - Check your Config.cs file
)

:end
pause