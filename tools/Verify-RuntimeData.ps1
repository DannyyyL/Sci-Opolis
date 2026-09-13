[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

# The game targets .NET Framework, so reflection must run in Windows PowerShell.
if ($PSVersionTable.PSVersion.Major -gt 5) {
    $windowsPowerShell = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    & $windowsPowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -Configuration $Configuration
    exit $LASTEXITCODE
}

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) {
        throw "Runtime data verification failed: $Message"
    }
}

function Get-FileBytesKey {
    param([string] $Path)
    return [Convert]::ToBase64String([IO.File]::ReadAllBytes($Path))
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$executable = Join-Path $repositoryRoot "Sci-Opolis\bin\$Configuration\Sci-Opolis.exe"
Assert-True (Test-Path -LiteralPath $executable -PathType Leaf) "Build $Configuration before running this script: $executable"
$assembly = [Reflection.Assembly]::LoadFrom($executable)
$runtimeData = $assembly.GetType('SciOpolis.RuntimeData', $true)
$staticFlags = [Reflection.BindingFlags]'NonPublic, Static'
$ensureDefaults = $runtimeData.GetMethod('EnsureDefaults', $staticFlags)
$resolvePath = $runtimeData.GetMethod('ResolvePath', $staticFlags)
$fileManagerType = $assembly.GetType('SciOpolis.FileManager', $true)
# Do not run the constructor: it would create defaults in the host's BaseDirectory.
$fileManager = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($fileManagerType)
$readStage = $fileManagerType.GetMethod('ReadStageLayout')
$readStats = $fileManagerType.GetMethod('ReadStatsAndInventory')
$saveStats = $fileManagerType.GetMethod('SaveStatsAndInventory')
$readUpgrades = $fileManagerType.GetMethod('ReadUpgrades')
$saveUpgrades = $fileManagerType.GetMethod('SaveUpgrades')

$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
$ownedDirectoryName = 'SciOpolis-runtime-' + [Guid]::NewGuid().ToString('N')
$verificationDirectory = [IO.Path]::GetFullPath((Join-Path $tempRoot $ownedDirectoryName))
$ownsVerificationDirectory = $false
$originalDirectory = [Environment]::CurrentDirectory
$originalLocation = Get-Location

try {
    Assert-True (-not (Test-Path -LiteralPath $verificationDirectory)) 'The unique test directory must not already exist.'
    [void] (New-Item -ItemType Directory -Path $verificationDirectory)
    $ownsVerificationDirectory = $true
    [void] $ensureDefaults.Invoke($null, [object[]] @($verificationDirectory))

    $stagePath = Join-Path $verificationDirectory 'Stage.txt'
    $statsPath = Join-Path $verificationDirectory 'Statistics.txt'
    $upgradesPath = Join-Path $verificationDirectory 'Upgrades.txt'
    $paths = @($stagePath, $statsPath, $upgradesPath)
    foreach ($path in $paths) {
        Assert-True (Test-Path -LiteralPath $path -PathType Leaf) "Default was not created: $path"
    }

    $stageLines = [IO.File]::ReadAllLines($stagePath)
    Assert-True ($stageLines.Length -eq 25) 'Stage must contain three headers and exactly 22 tile rows.'
    $stage = $readStage.Invoke($fileManager, [object[]] @([string] $stagePath, 38, 22))
    Assert-True ($null -ne $stage) 'FileManager must parse the stage.'
    Assert-True ($stage.GetLength(0) -eq 22 -and $stage.GetLength(1) -eq 38) 'Stage dimensions must be 22 by 38.'
    for ($row = 0; $row -lt 22; $row++) {
        $tiles = $stageLines[$row + 3].Split(',')
        Assert-True ($tiles.Length -eq 38) "Stage row $row must contain exactly 38 tiles."
        for ($column = 0; $column -lt 38; $column++) {
            Assert-True ($tiles[$column] -cmatch '^[0-5]$') "Invalid tile at row $row, column $column."
            Assert-True ($stage[$row, $column] -eq [int] $tiles[$column]) 'FileManager stage parsing must preserve each tile.'
            if ($row -ge 16) {
                Assert-True ($stage[$row, $column] -ne 0) "Ground must be nonempty at row $row, column $column."
            }
        }
    }

    $labels = @('Best Survival Time', 'Coins', 'Coins Spent', 'Times Logged In', 'Waves Survived', 'Enemies Killed')
    $statsLines = [IO.File]::ReadAllLines($statsPath)
    Assert-True ($statsLines.Length -eq 6) 'Statistics must contain exactly six rows.'
    $stats = $readStats.Invoke($fileManager, [object[]] @([string] $statsPath, 6))
    Assert-True ($null -ne $stats) 'FileManager must parse statistics.'
    for ($index = 0; $index -lt $labels.Length; $index++) {
        Assert-True ($statsLines[$index] -ceq ($labels[$index] + ',0')) "Default statistic $index must have the expected label and zero value."
        Assert-True ($stats[$index, 0] -ceq ($labels[$index] + ': ') -and $stats[$index, 1] -ceq '0') "FileManager must parse statistic $index."
    }

    $upgradeLines = [IO.File]::ReadAllLines($upgradesPath)
    Assert-True ($upgradeLines.Length -eq 3 -and $upgradeLines[2] -ceq '1,1,1,1,1') 'Defaults must contain exactly five locked upgrades.'
    $upgrades = $readUpgrades.Invoke($fileManager, [object[]] @([string] $upgradesPath, 5))
    Assert-True ($null -ne $upgrades -and $upgrades.Length -eq 5) 'FileManager must parse five upgrades.'
    foreach ($upgrade in $upgrades) {
        Assert-True (-not $upgrade) 'Every default upgrade must be locked.'
    }
    $defaultUpgradeBytes = Get-FileBytesKey $upgradesPath

    # Exercise the existing save formats with rooted paths, then read them back.
    $stats[1, 1] = '17'
    $stats[3, 1] = '1'
    [void] $saveStats.Invoke($fileManager, [object[]] @([string] $statsPath, $stats))
    $savedStats = $readStats.Invoke($fileManager, [object[]] @([string] $statsPath, 6))
    Assert-True ($null -ne $savedStats) 'Saved statistics must remain readable.'
    for ($row = 0; $row -lt 6; $row++) {
        for ($column = 0; $column -lt 2; $column++) {
            Assert-True ($savedStats[$row, $column] -ceq $stats[$row, $column]) "Statistic $row must survive a save/read round trip."
        }
    }
    [bool[]] $updatedUpgrades = @($true, $false, $true, $false, $true)
    [void] $saveUpgrades.Invoke($fileManager, [object[]] @([string] $upgradesPath, $updatedUpgrades))
    $savedUpgrades = $readUpgrades.Invoke($fileManager, [object[]] @([string] $upgradesPath, 5))
    Assert-True ($null -ne $savedUpgrades) 'Saved upgrades must remain readable.'
    for ($index = 0; $index -lt 5; $index++) {
        Assert-True ($savedUpgrades[$index] -eq $updatedUpgrades[$index]) "Upgrade $index must survive a save/read round trip."
    }

    # Existing files, including invalid saves, belong to the player and must survive unchanged.
    [IO.File]::WriteAllText($stagePath, "custom stage sentinel`r`n")
    [IO.File]::WriteAllText($statsPath, 'malformed statistics sentinel')
    [IO.File]::WriteAllText($upgradesPath, 'malformed upgrades sentinel')
    $existingBytes = @{}
    foreach ($path in $paths) { $existingBytes[$path] = Get-FileBytesKey $path }
    [void] $ensureDefaults.Invoke($null, [object[]] @($verificationDirectory))
    [void] $ensureDefaults.Invoke($null, [object[]] @($verificationDirectory))
    foreach ($path in $paths) {
        Assert-True ((Get-FileBytesKey $path) -ceq $existingBytes[$path]) "Initialization must preserve existing bytes: $path"
    }
    Remove-Item -LiteralPath $upgradesPath
    [void] $ensureDefaults.Invoke($null, [object[]] @($verificationDirectory))
    Assert-True ((Get-FileBytesKey $upgradesPath) -ceq $defaultUpgradeBytes) 'Only the missing upgrade file must be restored from embedded defaults.'
    foreach ($path in @($stagePath, $statsPath)) {
        Assert-True ((Get-FileBytesKey $path) -ceq $existingBytes[$path]) "Restoring a missing file must preserve $path."
    }

    [Environment]::CurrentDirectory = $verificationDirectory
    Set-Location -LiteralPath $verificationDirectory
    $relativePath = $resolvePath.Invoke($null, [object[]] @('Statistics.txt'))
    $expectedPath = Join-Path ([AppDomain]::CurrentDomain.BaseDirectory) 'Statistics.txt'
    Assert-True ($relativePath -ceq $expectedPath) 'Relative paths must use AppDomain BaseDirectory, independent of launch CWD.'
    $rootedPath = $resolvePath.Invoke($null, [object[]] @([string] $statsPath))
    Assert-True ($rootedPath -ceq $statsPath) 'Rooted file paths must remain unchanged.'

    Write-Output "PASS: $Configuration embedded defaults, stage, stats/upgrades round trips, existing-file preservation, missing-file restoration, and CWD-independent paths."
}
finally {
    [Environment]::CurrentDirectory = $originalDirectory
    Set-Location -LiteralPath $originalLocation.Path
    if ($ownsVerificationDirectory -and (Test-Path -LiteralPath $verificationDirectory)) {
        $resolvedDirectory = (Resolve-Path -LiteralPath $verificationDirectory).ProviderPath
        Assert-True ($resolvedDirectory.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
            [IO.Path]::GetFileName($resolvedDirectory) -ceq $ownedDirectoryName) 'Cleanup target must be the uniquely owned directory within the temp root.'
        Remove-Item -LiteralPath $resolvedDirectory -Recurse -Force
    }
}
