@echo off
cd /d %~dp0

@rem dotnet CLIのテレメトリを送信しない
set DOTNET_CLI_TELEMETRY_OPTOUT=1

@rem 基本設定
set DOTNET="%ProgramFiles%\dotnet\dotnet.exe"
if "%TARGET_PLATFORM%" == "" set TARGET_PLATFORM=win
if "%TARGET_ARCH%" == "" set TARGET_ARCH=x64
set TARGET_RUNTIME=%TARGET_PLATFORM%-%TARGET_ARCH%
set TARGET_SLN=MakiMoki.sln
set TARGET_PRJ=src\wpf\MakiMoki.Wpf\MakiMoki.Wpf.csproj
set OUTPUT_ROOT=publish
set OUTPUT_DIR=%OUTPUT_ROOT%\FutaMaki
set CONF_FILE=publish.wpf.conf.json
for /f %%a in ('powershell -Command "(Get-Content %CONF_FILE% | ConvertFrom-Json).version"') do set VERSION=%%a

if "%1" == "beta" for /f %%a in ('powershell -Command "Write-Output('futamaki-{0:000}b{1:00}.zip' -f ((New-Object System.Version('%VERSION%')).Minor + 1), (New-Object System.Version('%VERSION%')).Build)"') do set OUTPUT_ZIP=%%a
if "%1" == "" set OUTPUT_ZIP=
if "%OUTPUT_ZIP%" == "" for /f %%a in ('powershell -Command "Write-Output('futamaki-{0:000}.zip' -f (New-Object System.Version('%VERSION%')).Minor)"') do set OUTPUT_ZIP=%%a

@rem 事前チェック
if not exist %DOTNET% echo dotnetコマンドが見つかりません & goto end
if not exist %TARGET_SLN% echo SLNが見つかりません & goto end
if exist %OUTPUT_ROOT%\%OUTPUT_ZIP% echo 既にアーカイブが存在します & goto end

@rem ビルド
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR% 
%DOTNET% restore
%DOTNET% clean --nologo -c Release -r %TARGET_RUNTIME%
%DOTNET% publish ^
   --nologo ^
   -c Release ^
   -r %TARGET_RUNTIME% ^
   -o %OUTPUT_DIR% ^
   %TARGET_PRJ%
if not %errorlevel%==0 goto end

xcopy /y %OUTPUT_DIR%\x86 %OUTPUT_DIR%\Lib\x86
xcopy /y %OUTPUT_DIR%\x64 %OUTPUT_DIR%\Lib\x64
rd /s /q %OUTPUT_DIR%\x86
rd /s /q %OUTPUT_DIR%\x64
rd /s /q %OUTPUT_DIR%\arm64

rd /s /q %OUTPUT_DIR%\Lib\x86
rd /s /q %OUTPUT_DIR%\libvlc\win-x86
move %OUTPUT_ROOT%\FutaMaki\futamaki.dll.config %OUTPUT_ROOT%\FutaMaki\futamaki.exe.config

@rem 公開用ZIPファイルの生成
@rem 既に存在してる場合は設定がおかしいのであえて-Forceはつけない
rem echo 圧縮してます…
powershell -Command "Compress-Archive -Path %OUTPUT_DIR% -DestinationPath %OUTPUT_ROOT%\%OUTPUT_ZIP%"

:end
pause
