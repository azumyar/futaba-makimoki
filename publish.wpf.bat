@echo off
cd /d %~dp0

@rem dotnet CLI�̃e�����g���𑗐M���Ȃ�
set DOTNET_CLI_TELEMETRY_OPTOUT=1

@rem ��{�ݒ�
set DOTNET=%ProgramFiles%\dotnet\dotnet.exe
if "%VS_VERSION%" == "" set VS_VERSION=2019
if "%VS_EDITION%" == "" set VS_EDITION=Community
if not exist "%MSBUILD%" set MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\%VS_VERSION%\%VS_EDITION%\MSBuild\Current\Bin\MSBuild.exe
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

@rem ���O�`�F�b�N
if not exist "%DOTNET%" echo dotnet�R�}���h��������܂��� & goto end
if not exist %TARGET_SLN% echo SLN��������܂��� & goto end
if exist %OUTPUT_ROOT%\%OUTPUT_ZIP% echo ���ɃA�[�J�C�u�����݂��܂� & goto end

@rem �r���h
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR% 
"%DOTNET%" restore
if not %errorlevel%==0 goto end
"%DOTNET%" clean --nologo -c Release -r %TARGET_RUNTIME%
if not %errorlevel%==0 goto end
"%MSBUILD%" %TARGET_SLN% -nologo -m -t:TransformAll
if not %errorlevel%==0 goto end
"%DOTNET%" publish ^
   --nologo ^
   -c Release ^
   -r %TARGET_RUNTIME% ^
   -o %OUTPUT_DIR% ^
   %TARGET_PRJ%
if not %errorlevel%==0 goto end

xcopy /y %OUTPUT_DIR%\x86 %OUTPUT_DIR%\Lib\x86
xcopy /y %OUTPUT_DIR%\x64 %OUTPUT_DIR%\Lib\x64
xcopy /y %OUTPUT_DIR%\arm64 %OUTPUT_DIR%\Lib\arm64
rd /s /q %OUTPUT_DIR%\x86
rd /s /q %OUTPUT_DIR%\x64
rd /s /q %OUTPUT_DIR%\arm64

powershell -Command "Get-ChildItem -Path %OUTPUT_DIR%\Lib\ -Exclude %TARGET_ARCH% | Remove-Item -Recurse -Force"
powershell -Command "Get-ChildItem -Path %OUTPUT_DIR%\libvlc\\ -Exclude %TARGET_ARCH% | Remove-Item -Recurse -Force"
move %OUTPUT_ROOT%\FutaMaki\futamaki.dll.config %OUTPUT_ROOT%\FutaMaki\futamaki.exe.config

@rem ���J�pZIP�t�@�C���̐���
@rem ���ɑ��݂��Ă�ꍇ�͐ݒ肪���������̂ł�����-Force�͂��Ȃ�
echo ���k���Ă܂��c
powershell -Command "Compress-Archive -Path %OUTPUT_DIR% -DestinationPath %OUTPUT_ROOT%\%OUTPUT_ZIP%"

:end
pause