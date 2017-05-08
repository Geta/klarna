@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack Klarna.Payments\Klarna.Payments.csproj -IncludeReferencedProjects
nuget pack Klarna.Payments.CommerceManager\Klarna.Payments.CommerceManager.csproj -IncludeReferencedProjects
nuget pack Klarna.OrderManagement\Klarna.OrderManagement.csproj -IncludeReferencedProjects
nuget pack Klarna.OrderManagement.CommerceManager\Klarna.OrderManagement.CommerceManager.csproj -IncludeReferencedProjects
nuget pack Klarna.Checkout\Klarna.Checkout.csproj -IncludeReferencedProjects
nuget pack Klarna.Checkout.CommerceManager\Klarna.Checkout.CommerceManager.csproj -IncludeReferencedProjects

copy EPiServer.Klarna.*.nupkg ..\demo\lib /Y

@echo on