# FTF MSIX packaging script (.NET 10 full feature build)
# Usage: powershell -ExecutionPolicy Bypass -File build/msix/pack-net10.ps1
# Publisher/PackageName/DisplayName MUST match Partner Center product identity.

param(
    [string]$Version = "1.0.6.0",
    [string]$Publisher = "CN=1F52438E-D4A2-4DEE-96DD-26314B86665C",
    [string]$PublisherDisplayName = "Moonto Studio",
    [string]$PackageName = "MoontoStudio.FiletoFiles",
    [string]$DisplayName = ("File to Files" + [char]0x4E07 + [char]0x80FD + [char]0x6587 + [char]0x4EF6 + [char]0x5904 + [char]0x7406 + [char]0x5927 + [char]0x5E08),
    [string]$MakeAppx = "$env:TEMP\msixsdk26100\bin\10.0.26100.0\x64\makeappx.exe"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root

Write-Host "=== FTF MSIX packaging (.NET 10) ===" -ForegroundColor Cyan

if (-not (Test-Path $MakeAppx)) {
    Write-Error "makeappx.exe not found: $MakeAppx"
}

# 1. Check .NET 10 SDK
Write-Host "`n[1/4] Checking .NET 10 SDK..." -ForegroundColor Yellow
$sdk = dotnet --version
Write-Host "Current SDK: $sdk"
if (-not $sdk.StartsWith("10.")) { Write-Error ".NET 10 SDK required, current is $sdk" }

# 2. Self-contained publish
Write-Host "`n[2/4] Publishing app..." -ForegroundColor Yellow
dotnet publish src/Filee.App/Filee.App.csproj -c Release -r win-x64 `
    -p:FileeRelease=true --self-contained true -o AppPackages/publish -v minimal
if ($LASTEXITCODE -ne 0) { Write-Error "Publish failed" }

# 3. Stage package directory (binaries + icons + manifest)
Write-Host "`n[3/4] Staging package..." -ForegroundColor Yellow
$stage = "AppPackages/msix-temp"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path "$stage/Assets/Icons" -Force | Out-Null
Copy-Item "AppPackages/publish/*" -Destination $stage -Recurse -Force
Copy-Item "build/msix/Assets/Icons/StoreLogo.png", "build/msix/Assets/Icons/Square50x50Logo.png", "build/msix/Assets/Icons/Square150x150Logo.png" -Destination "$stage/Assets/Icons" -Force

$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
  IgnorableNamespaces="uap rescap">
  <Identity Name="$PackageName" Publisher="$Publisher" Version="$Version" ProcessorArchitecture="x64" />
  <Properties>
    <DisplayName>$DisplayName</DisplayName>
    <PublisherDisplayName>$PublisherDisplayName</PublisherDisplayName>
    <Logo>Assets\Icons\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
    <Resource Language="zh-cn" />
  </Resources>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
  <Applications>
    <Application Id="FTF" Executable="FTF.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements BackgroundColor="#1e1e1e" DisplayName="$DisplayName"
        Square150x150Logo="Assets\Icons\Square150x150Logo.png"
        Square44x44Logo="Assets\Icons\Square50x50Logo.png"
        Description="Free open-source local file format converter. 263 formats, fully offline." />
    </Application>
  </Applications>
</Package>
"@
[System.IO.File]::WriteAllText("$stage/AppxManifest.xml", $manifest, [System.Text.UTF8Encoding]::new($false))

# 4. Pack
Write-Host "`n[4/4] Generating MSIX..." -ForegroundColor Yellow
$out = "AppPackages/FTF_${Version}_x64.msix"
& $MakeAppx pack /d $stage /p $out /o | Select-Object -Last 3
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $out)) { Write-Error "Pack failed" }

$size = [math]::Round((Get-Item $out).Length / 1MB, 1)
Write-Host "`n=== Done ===" -ForegroundColor Green
Write-Host "Output: $out ($size MB)" -ForegroundColor Cyan
Write-Host "Identity: Name=$PackageName Publisher=$Publisher DisplayName=$DisplayName" -ForegroundColor Cyan
