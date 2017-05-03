@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack Klarna.Payments\Klarna.Payments.csproj -IncludeReferencedProjects
nuget pack Klarna.Payments.CommerceManager\Klarna.Payments.CommerceManager.csproj -IncludeReferencedProjects
nuget pack Klarna.OrderManagement\Klarna.OrderManagement.csproj -IncludeReferencedProjects

@echo on