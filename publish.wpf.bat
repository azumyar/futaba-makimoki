@echo off
cd /d %~dp0

@rem dotnet CLIのテレメトリを送信しない
set DOTNET_CLI_TELEMETRY_OPTOUT=1

@rem 基本設定
set DOTNET=%ProgramFiles%\dotnet\dotnet.exe
if "%VS_VERSION%" == "" set VS_VERSION=2022
if "%VS_EDITION%" == "" set VS_EDITION=Community
if not exist "%MSBUILD%" set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\%VS_VERSION%\%VS_EDITION%\Msbuild\Current\Bin\amd64\MSBuild.exe
if "%TARGET_PLATFORM%" == "" set TARGET_PLATFORM=win
if "%TARGET_ARCH%" == "" set TARGET_ARCH=x64
set TARGET_RUNTIME=%TARGET_PLATFORM%-%TARGET_ARCH%
set TARGET_SLN=MakiMoki.sln
set TARGET_PRJ=src\wpf\MakiMoki.Wpf\MakiMoki.Wpf.csproj
set OUTPUT_ROOT=publish
set OUTPUT_DIR=%OUTPUT_ROOT%\FutaMaki
set CONF_FILE=src\wpf\MakiMoki.Wpf\Properties\publish.conf.json
for /f %%a in ('powershell -Command "(Get-Content %CONF_FILE% | ConvertFrom-Json).version"') do set VERSION=%%a

if "%1" == "beta" for /f %%a in ('powershell -Command "Write-Output('futamaki-{0:000}b{1:00}.zip' -f ((New-Object System.Version('%VERSION%')).Minor + 1), (New-Object System.Version('%VERSION%')).Build)"') do set OUTPUT_ZIP=%%a
if "%1" == "" set OUTPUT_ZIP=
if "%OUTPUT_ZIP%" == "" for /f %%a in ('powershell -Command "Write-Output('futamaki-{0:000}.zip' -f (New-Object System.Version('%VERSION%')).Minor)"') do set OUTPUT_ZIP=%%a

@rem 事前チェック
if not exist "%DOTNET%" echo dotnetコマンドが見つかりません & goto end
if not exist %TARGET_SLN% echo SLNが見つかりません & goto end
if exist %OUTPUT_ROOT%\%OUTPUT_ZIP% echo 既にアーカイブが存在します & goto end

@rem ビルド
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR% 
"%DOTNET%" restore -p:PublishReadyToRun=true
if not %errorlevel%==0 goto end
"%DOTNET%" clean --nologo -c Release -r %TARGET_RUNTIME%
if not %errorlevel%==0 goto end
"%MSBUILD%" %TARGET_SLN% -nologo -m -t:TransformAll
if not %errorlevel%==0 goto end
"%DOTNET%" msbuild ^
   -noLogo ^
   -p:Configuration=Release ^
   -p:RuntimeIdentifier=%TARGET_RUNTIME% ^
   -m ^
   %TARGET_PRJ%
if not %errorlevel%==0 goto end
"%DOTNET%" publish ^
   --nologo ^
   -c Release ^
   -r %TARGET_RUNTIME% ^
   -o %OUTPUT_DIR% ^
   --no-restore ^
   --no-build ^
   %TARGET_PRJ%
if not %errorlevel%==0 goto end

mkdir %OUTPUT_DIR%\runtimes\libvlc
xcopy /y /s /q src\wpf\MakiMoki.Wpf\bin\Release\net6.0-windows\%TARGET_RUNTIME%\libvlc %OUTPUT_DIR%\runtimes\libvlc

powershell -Command "Get-ChildItem -Path %OUTPUT_DIR%\runtimes\libwebp\ -Exclude win-%TARGET_ARCH% | Remove-Item -Recurse -Force"
powershell -Command "Get-ChildItem -Path %OUTPUT_DIR%\runtimes\libvlc\\ -Exclude win-%TARGET_ARCH% | Remove-Item -Recurse -Force"
move %OUTPUT_ROOT%\FutaMaki\futamaki.dll.config %OUTPUT_ROOT%\FutaMaki\futamaki.exe.config

@rem 公開用ZIPファイルの生成
@rem 既に存在してる場合は設定がおかしいのであえて-Forceはつけない
echo 圧縮してます…
powershell -Command "Compress-Archive -Path %OUTPUT_DIR% -DestinationPath %OUTPUT_ROOT%\%OUTPUT_ZIP%"

:end
pause
