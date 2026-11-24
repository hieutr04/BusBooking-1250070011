param([switch]$Database,[switch]$FiveMinutes,[string]$BaseUrl="http://localhost:5190")
$ErrorActionPreference="Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)
New-Item -ItemType Directory -Force TestResults | Out-Null
Start-Transcript -Path TestResults/verification-transcript.txt
try {
 function Run-Dotnet([string[]]$Arguments) { & dotnet @Arguments; if($LASTEXITCODE -ne 0){throw "dotnet failed: $Arguments"} }
 Run-Dotnet @("--info")
 Run-Dotnet @("tool","restore")
 Run-Dotnet @("restore","BusBooking.sln")
 Run-Dotnet @("build","BusBooking.sln","--no-restore")
 Run-Dotnet @("ef","migrations","has-pending-model-changes","--project","BusBooking")
 Run-Dotnet @("build","Tests/BusBooking.SqlTests/BusBooking.SqlTests.csproj")
 Run-Dotnet @("run","--project","Tests/BusBooking.RedirectTests")
 if($Database) {
  $testArgs=@("run","--project","Tests/BusBooking.SqlTests","--","--ack-test-db","--http",$BaseUrl)
  if($FiveMinutes){$testArgs+="--five-minutes"}
  Run-Dotnet $testArgs
 }
} finally {Stop-Transcript}
