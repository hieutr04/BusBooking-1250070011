$ErrorActionPreference="Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)
# Schema changes remain explicit; this script never drops or resets a database.
& dotnet tool restore
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
& dotnet ef database update --project BusBooking
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
& dotnet run --project BusBooking --launch-profile SqlServer -- --seed
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
& dotnet run --project BusBooking --launch-profile SqlServer
