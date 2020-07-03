#----------------------------------------
#Prerequisites
#----------------------------------------
#Install DotNet 3.1 SDK

#Install dotnet global DotnetTool
#dotnet tool install -g dotnet-reportgenerator-globaltool

#Add coverlet.collector nuget package to the unit test .csproj
#dotnet add package coverlet.collector

#----------------------------------------
#Run tests with coverage
#----------------------------------------
#Run unit tests and collect code coverage (via coverlet.collector nuget package installed in this test project)
$testResultsDir = "TestResults"
#Remove TestResults dir and any subdirectories, because each time "dotnet test" is run, a new subfolder with a GUID is created.
if (Test-Path $testResultsDir) {Remove-Item $testResultsDir -Recurse -Force}

$coverageReportDir = "$testResultsDir\coveragereport"

#Run unit tests and collect coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory "$testResultsDir"
if($LASTEXITCODE -ne 0) {return}

#Convert code coverage to human readable format
$recentCoverageFile = Get-ChildItem -File -Filter coverage*.xml -Path "$testResultsDir" -Name -Recurse | Select-Object -First 1;
reportgenerator "-reports:$testResultsDir\$recentCoverageFile" "-targetdir:$coverageReportDir" -reporttypes:TextSummary

#Display coverage in text form
Get-Content .\TestResults\coveragereport\Summary.txt
