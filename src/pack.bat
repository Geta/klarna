@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack Klarna.Payments\Klarna.Payments.v3.csproj -IncludeReferencedProjects
nuget pack Klarna.OrderManagement\Klarna.OrderManagement.v3.csproj -IncludeReferencedProjects
nuget pack Klarna.Checkout\Klarna.Checkout.v3.csproj -IncludeReferencedProjects

copy Klarna.*.nupkg ..\demo\lib /Y

@echo on