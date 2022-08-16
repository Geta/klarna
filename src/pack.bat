@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)

dotnet pack Klarna.Common\Klarna.Common.v3.csproj
dotnet pack Klarna.OrderManagement\Klarna.OrderManagement.v3.csproj
dotnet pack Klarna.Payments\Klarna.Payments.v3.csproj
dotnet pack Klarna.Checkout\Klarna.Checkout.v3.csproj

copy Klarna.*.nupkg ..\demo\lib /Y

@echo on