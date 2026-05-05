param(
    [Parameter(Mandatory=$false)]
    [string]$UltrakillPath = "C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL",

    [Parameter(Mandatory=$false)]
    [string]$R2ModmanProfilePath = "$env:APPDATA\r2modmanPlus-local\ULTRAKILL\profiles\Default"
)

Set-StrictMode -Version Latest

$root = $PSScriptRoot
$targetDir = Join-Path $root "libs\Managed"

Write-Host "--- Library Setup ---"
Write-Host "Target directory: $targetDir"

# Ensure the target directory exists
if (-not (Test-Path $targetDir)) {
    Write-Host "Creating target directory..."
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

# 1) Copy Managed DLLs from ULTRAKILL installation
$ukManaged = Join-Path $UltrakillPath "ULTRAKILL_Data\Managed"
if (-not (Test-Path $ukManaged)) {
    Write-Error "ULTRAKILL Managed directory not found at: $ukManaged"
    Write-Host "Please ensure -UltrakillPath points to the root of your ULTRAKILL installation" -ForegroundColor Yellow
    return
}
$ukDlls = @(
    "Assembly-CSharp.dll"
    "UnityEngine.dll"
    "UnityEngine.CoreModule.dll"
)

foreach ($dll in $ukDlls) {
    $source = Join-Path $ukManaged $dll
    if (Test-Path $source) {
        Copy-Item -Path $source -Destination $targetDir -Force
        Write-Host "Successfully copied: $dll"
    } else {
        Write-Error "Source file not found: $source"
    }
}

# 2) Copy Plugin DLLs from r2modman profile
$R2ModmanProfilePath
if (-not (Test-Path $R2ModmanProfilePath)) {
    Write-Warning "BepInEx plugins directory not found in the profile. Ensure your plugins are installed in r2modman and/or make sure -R2ModmanProfilePath points to your R2Modman profile"
}

$r2Dlls = @(
    "BepInEx\core\BepInEx.dll"
    "BepInEx\plugins\EternalsTeam-PluginConfigurator\PluginConfigurator\PluginConfiguratorComponents.dll"
    "BepInEx\plugins\EternalsTeam-PluginConfigurator\PluginConfigurator\PluginConfigurator.dll"
)

foreach ($dll in $r2Dlls) {
    $source = Join-Path $R2ModmanProfilePath $dll
    if (Test-Path $source) {
        Copy-Item -Path $source -Destination $targetDir -Force
        Write-Host "Successfully copied: $(Split-Path $dll -Leaf)"
    } else {
        Write-Error "Source file not found: $source"
    }
}

Write-Host "`nSetup finished. If any errors occurred (e.g. files not found), please specify your paths manually using the -UltrakillPath and -R2ModmanProfilePath parameters."
Write-Host "Otherwise, you can now build the mod using make-package.ps1."
