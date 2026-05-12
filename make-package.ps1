<#
make-package.ps1

Builds the mod, collects package files and assets, and creates a zip package.
#>

param(
    [string]$OutputDir = $PSScriptRoot,
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$packageFolder = Join-Path $root 'package'
$tmpPackageBuildDir = 'package_build'
$modName = 'Notiffy'
Write-Host "Repository root: $root"

# 1) Prepare staging directory
$staging = Join-Path $root "$tmpPackageBuildDir"
if (Test-Path $staging) {
    Write-Host "Removing existing staging folder: $staging"
    Remove-Item $staging -Recurse -Force
}
New-Item -ItemType Directory -Path $staging | Out-Null

# Define the internal plugin path: plugins/ModName
$pluginDest = Join-Path $staging "plugins/$modName"
New-Item -ItemType Directory -Path $pluginDest -Force | Out-Null

# 2) Build the mod and copy it to plugins/$modName
$modFolder = $root
$assemblyPath = Join-Path $modFolder "bin/$Configuration/netstandard2.1/$modName.dll"
$xmlPath = Join-Path $modFolder "bin/$Configuration/netstandard2.1/$modName.xml"

Push-Location ($modFolder)
Write-Host "Building mod in 'mod' using configuration: $Configuration"
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

Write-Host "Copying binaries to: $pluginDest"
Copy-Item -Path ($assemblyPath) -Destination $pluginDest -Force
if (Test-Path $xmlPath) {
    Write-Host "Copying documentation: $(Split-Path $xmlPath -Leaf)"
    Copy-Item -Path $xmlPath -Destination $pluginDest -Force
}
Pop-Location

$iconSrc = Join-Path $packageFolder 'icon.png'
if (Test-Path $iconSrc) {
    Write-Host "Copying icon.png to plugin folder for internal use"
    Copy-Item -Path $iconSrc -Destination $pluginDest -Force
}

# 3) Copy manifest, icon, and readme to root of staging
if (-not (Test-Path $packageFolder)) { throw "package folder not found at $packageFolder" }
Write-Host "Copying package files (manifest/icon) from '$packageFolder' to staging root"
Copy-Item -Path (Join-Path $packageFolder '*') -Destination $staging -Recurse -Force

# Read name/version for zip naming
$manifestPath = Join-Path $packageFolder 'manifest.json'
$pkgName = 'package'
$pkgVer = (Get-Date -Format yyyyMMddHHmmss)
if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.name) { $pkgName = $manifest.name }
        if ($manifest.version_number) { $pkgVer = $manifest.version_number }
    } catch {
        Write-Warning "Could not parse manifest.json for name/version."
    }
}

# 4) Copy assets into plugins/$modName/assets
$assetsSrc = Join-Path $root 'assets'
$assetsDest = Join-Path $pluginDest 'assets'

if (Test-Path $assetsSrc) {
    Write-Host "Copying assets from '$assetsSrc' to '$assetsDest'"
    New-Item -ItemType Directory -Path $assetsDest -Force | Out-Null
    Copy-Item -Path (Join-Path $assetsSrc '*') -Destination $assetsDest -Recurse -Force
} else {
    Write-Warning "Assets folder not found at: $assetsSrc"
}

# 5) Create zip package
$zipName = "$pkgName-$pkgVer.zip"
$zipPath = Join-Path $OutputDir $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Write-Host "Creating zip: $zipPath"

# Compress everything inside the staging folder
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -Force

Write-Host "Package created at: $zipPath"
Write-Output $zipPath
