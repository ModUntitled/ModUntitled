@echo off
setlocal EnableDelayedExpansion 


:: Requirements
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
	set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
	echo MSBuild 15.0 
  goto _zip
) 
     
if exist "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" (
	set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
	echo MSBuild 14.0 
  goto _zip
) 

echo ERROR: Microsoft.NET framework 3.5 not found. Make sure you have Visual Studio installed.
goto _exit
          
:_zip
set sevenz=7z
if exist "C:\Program Files\7-Zip\7z.exe" (
    set sevenz="C:\Program Files\7-Zip\7z.exe"
    goto prep
)

if exist "C:\Program Files(x86)\7-Zip\7z.exe" (
    set sevenz="C:\Program Files (x86)\7-Zip\7z.exe"
    goto prep
)  

where 7z >nul 2>nul
if not %errorlevel%==0 (
    echo ERROR: 7zip was not found. Make sure that you have it installed and in the PATH.
    goto _exit
)


:prep
set target=Debug
set target_unsigned=Debug
if %1.==release. goto _if_setrelease
goto _ifj_setrelease
:_if_setrelease
set target=Release
set target_unsigned=Release-Unsigned
:_ifj_setrelease

:: Prepare the build directory
set build_base=build
set build_mod=MOD-UNTITLED
set "build=%build_base%\%build_mod%"
set "build_zip=%build_base%\%build_mod%.zip"

if exist "%build_base%" rmdir /q /s "%build_base%"
mkdir "%build_base%" 2>nul
mkdir "%build%" 2>nul

:: Build
where xbuild >nul 2>nul
if %errorlevel%==0 (
  ::call xbuild
  rem
) else (
  call %msbuild%
)

for /f "tokens=*" %%L in (build-files) do (
  set "line=%%L"
  set "line=!line:/=\!"
  if not "!line:~0,1!"=="#" (
    set "file_ex=!line:{TARGET}=%target%!"
    set "file=!file_ex:{TARGET-UNSIGNED}=%target_unsigned%!"
    for %%f in (!file!) do set target=%%~nxf

    echo !line!
    set "file_ex=!line:{TARGET}=A!"
    echo !file_ex!
    call echo %%line%%
    echo Copying '!file!' to '%build%/!target!'

    for %%i in (!file!) do (
      if exist %%~si\nul (
        robocopy "!file!" "%build%/!target!" /s /e
      ) else (
        copy "!file!" "%build%/!target!"
      )
    )
  )
)

:: Zipping it all up
pushd "%build%"
%sevenz% a MOD-UNTITLED.zip *
popd
move "%build%\MOD-UNTITLED.zip" "%build_zip%"

:: The End
:_exit

