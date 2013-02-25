REM PARAMS taskid & revision number
::TaskId:[Compile | UnitTests | Pack | AcceptanceTests] 
REM BUILD PARAMS
cd /d %~dp0
set buildType=%CI_CPU%

set frameworkVersion=%CI_Framework%
set nunitFramework=net-%frameworkVersion%
set frameworkFolder=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set configuration=%CI_Configuration%

REM COMMON TO ALL BUILDS
set artifactsFolder=%CD%\..\BuildArtifacts
set outputBinFolder=%artifactsfolder%\Binaries\%frameworkVersion%\%buildType%\%configuration%
set mergedFolder=%artifactsfolder%\Merged\v%frameworkVersion%\%buildType%
set outputReportsFolder=%artifactsfolder%\Reports\%frameworkVersion%\%buildType%
set packagesFolder=%CD%\..\Packages
set nunitExePath=%packagesFolder%\nUnit\NUnit-2.5.6.10205\nunitbin
set taskId=%1
set msBuildProperties=Configuration=%configuration%;OutputPath="%outputBinFolder%"\;Platform=%buildType%;Optimize=true;BuildReportsFolder="%outputReportsFolder%";TaskIdName=%taskId%;nUnitPath="%nunitExePath%";nUnitFramework=%nunitFramework%;MergedFolder="%mergedFolder%"

IF NOT "%2"=="" GOTO cont
	set msBuildProperties=%msBuildProperties%;BMinorNumber=0
:cont
IF "%2"=="" GOTO cont1
	set msBuildProperties=%msBuildProperties%;BMinorNumber=%2

:cont1

IF NOT "%3"=="" GOTO cont2
	set msBuildProperties=%msBuildProperties%;BRevisionNumber=0
:cont2
IF "%3"=="" GOTO cont3
	set msBuildProperties=%msBuildProperties%;BRevisionNumber=%3

:cont3

REM Invoking MSBuild
::%frameworkFolder%\msbuild /verbosity:diagnostic /fileLogger "%CD%"\RunBuild.xml /property:%msBuildProperties%
%frameworkFolder%\msbuild /verbosity:normal "%CD%"\RunBuild.xml /property:%msBuildProperties%
if %errorlevel% neq 0 exit /b %errorlevel%
