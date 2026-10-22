@echo off
setlocal
cd /d "%~dp0"

echo ================================================
echo  Publicacao do Gerador de Materiais Didaticos
echo ================================================
echo.
echo Compilando para Windows 64 bits em arquivo unico...

dotnet publish "GeradorMateriaisDesktop.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:PublishTrimmed=false -o "publicado"
if errorlevel 1 (
    echo.
    echo ERRO: a compilacao falhou.
    echo Confirme que o SDK .NET 10 esta instalado.
    pause
    exit /b 1
)
echo.
echo CONCLUIDO!
echo Executavel: "%~dp0publicado\GeradorMateriaisDesktop.exe"
echo.
pause
