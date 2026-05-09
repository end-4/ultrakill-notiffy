<#
make-package.ps1

Builds the mod, collects package files and assets, and creates a zip package.

Usage: run this script from PowerShell. By default the zip is created in the repo root.
Parameters:
  -OutputDir <path>   Directory where the final zip will be written (default: script folder)
  -Configuration <cfg> Build configuration (default: Release)
#>

param(
    [string]$OutputDir = $PSScriptRoot,
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
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

# 2) Build the mod and copy it
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
Copy-Item -Path ($assemblyPath) -Destination $staging -Recurse -Force
if (Test-Path $xmlPath) {
    Write-Host "Copying documentation: $(Split-Path $xmlPath -Leaf)"
    Copy-Item -Path $xmlPath -Destination $staging -Force
}
Pop-Location

# 3) Copy all files from package folder into staging
$packageFolder = Join-Path $root 'package'
if (-not (Test-Path $packageFolder)) { throw "package folder not found at $packageFolder" }
Write-Host "Copying package files from '$packageFolder' to staging"
Copy-Item -Path (Join-Path $packageFolder '*') -Destination $staging -Recurse -Force

# Try to read name/version from manifest.json for zip naming
$manifestPath = Join-Path $packageFolder 'manifest.json'
$pkgName = 'package'
$pkgVer = (Get-Date -Format yyyyMMddHHmmss)
if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.name) { $pkgName = $manifest.name }
        if ($manifest.version_number) { $pkgVer = $manifest.version_number }
    } catch {
        Write-Warning "Could not parse manifest.json for name/version. Falling back to timestamped name."
    }
}

# 4) Create BepInEx/plugins/assets and copy assets
$assetsSrc = Join-Path $root 'assets'
$assetsDest = Join-Path $staging 'BepInEx/plugins/assets'
Write-Host "Creating assets destination: $assetsDest"
New-Item -ItemType Directory -Path $assetsDest -Force | Out-Null
if (Test-Path $assetsSrc) {
    Write-Host "Copying assets from '$assetsSrc' to '$assetsDest'"
    Copy-Item -Path (Join-Path $assetsSrc '*') -Destination $assetsDest -Recurse -Force
} else {
    Write-Warning "Assets folder not found at: $assetsSrc"
}

# 5) Create zip package
$zipName = "$pkgName-$pkgVer.zip"
$zipPath = Join-Path $OutputDir $zipName
if (Test-Path $zipPath) { Write-Host "Removing existing zip: $zipPath"; Remove-Item $zipPath -Force }
Write-Host "Creating zip: $zipPath"

# # 6) Cleanup
# Remove-Item -Path "$tmpPackageBuildDir" -Recurse -Force

# Compress everything inside the staging folder so package root contains package files and BepInEx/...
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -Force

Write-Host "Package created at: $zipPath"
Write-Host "Staging folder retained at: $staging (remove if not needed)"

# Return path for scripts / automation
Write-Output $zipPath
