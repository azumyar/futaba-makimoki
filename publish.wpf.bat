@echo off
cd /d %~dp0

@rem dotnet CLIのテレメトリを送信しない
set DOTNET_CLI_TELEMETRY_OPTOUT=1

@rem 基本設定
set DOTNET="%ProgramFiles%\dotnet\dotnet.exe"
set CONF_FILE=publish.wpf.conf.json
set TARGET_SLN=MakiMoki.sln
set TARGET_PRJ=MakiMoki\MakiMoki.Wpf\MakiMoki.Wpf.csproj
set OUTPUT_ROOT=publish
set OUTPUT_DIR=%OUTPUT_ROOT%\FutaMaki
for /f %%a in ('powershell -Command "(Get-Content %CONF_FILE% | ConvertFrom-Json).version"') do set VERSION=%%a
for /f %%a in ('powershell -Command "Write-Output('futamaki-{0:000}.zip' -f (New-Object System.Version('%VERSION%')).Minor)"') do set OUTPUT_ZIP=%%a

@rem 事前チェック
if not exist %DOTNET% echo dotnetコマンドが見つかりません & goto end
if not exist %TARGET_SLN% echo SLNが見つかりません & goto end
if exist %OUTPUT_ROOT%\%OUTPUT_ZIP% echo 既にアーカイブが存在します & goto end

@rem ビルド
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR% 
%DOTNET% restore
%DOTNET% clean --nologo -c Release
%DOTNET% publish ^
   --nologo ^
   -r win-x64 ^
   -c Release ^
   -o %OUTPUT_DIR% ^
   %TARGET_PRJ%
if not %errorlevel%==0 goto end

rd /s /q %OUTPUT_DIR%\Lib\x86
rd /s /q %OUTPUT_DIR%\libvlc\win-x86

@rem 公開用ZIPファイルの生成
@rem 既に存在してる場合は設定がおかしいのであえて-Forceはつけない
echo 圧縮してます…
powershell -Command "Compress-Archive -Path %OUTPUT_DIR% -DestinationPath %OUTPUT_ROOT%\%OUTPUT_ZIP%"

:end
set /p TMP="続行するには何か入力してください…"