@echo off

SETLOCAL

IF [%1]==[] SET SOLUTION_NAME=./src/Microsoft.Extensions.Logging.Log4net.sln
IF NOT [%1]==[] SET SOLUTION_NAME=%1

IF [%2]==[] SET TEST_PROJECT=./tests/Unit.Tests.Target.Netcore31/Unit.Tests.Target.Netcore31.csproj
IF NOT [%2]==[] SET TEST_PROJECT=%2

dotnet build-server shutdown

DEL .\TestResults /S /Q > NUL

ECHO Building %SOLUTION_NAME%...
dotnet build %SOLUTION_NAME%

ECHO Runing tests...
dotnet test /p:Language=cs ^
    --no-build ^
    /p:CollectCoverage=true ^
    /p:CoverletOutputFormat=opencover ^
    /p:CoverletOutput="%CD%/TestResults/" ^
    %TEST_PROJECT% 

ECHO Generating reports...
dotnet tool install -g dotnet-reportgenerator-globaltool > NUL
reportgenerator -reports:"%CD%/TestResults/*.xml" -targetdir:"%CD%/TestResults/reports" -reporttypes:HTML;HTMLSummary

TestResults\reports\index.htm

ENDLOCAL