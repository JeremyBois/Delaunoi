:: This will create a symbolic link to enable localization inside UNITY
:: This script must be run as administrator
@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
SET parent=%~dp0

:: Define here relative folder path
SET "localizationTarget=../DelaunoiApplication\Assets\Plugins\Delaunoi\"
SET "localizationOrigin=Delaunoi\bin\Debug\"

:: Display first folders used
ECHO %me%: TARGET = %parent%%localizationTarget%
ECHO %me%: ORIGIN = %parent%%localizationOrigin%

:: Create symbolic link
mklink /D "%parent%%localizationTarget%" "%parent%%localizationOrigin%"
IF %ERRORLEVEL% NEQ 0 (
    ECHO %me%: Symbolic link not created.
) ELSE (
    ECHO %me%: Symbolic link has been created.
    ECHO %me%: %localizationTarget% now mirrors %localizationOrigin% changes.
)

ENDLOCAL
PAUSE
