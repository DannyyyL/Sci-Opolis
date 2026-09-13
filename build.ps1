[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [switch]$NoRestore
)

$ErrorActionPreference = 'Stop'
if (-not [Environment]::Is64BitOperatingSystem -or $env:OS -ne 'Windows_NT') {
    throw 'This legacy content compiler requires 64-bit Windows.'
}

$msbuildCommand = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
if ($msbuildCommand) {
    $msbuild = $msbuildCommand.Source
} else {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Install Visual Studio 2022 or Build Tools 2022 with MSBuild and .NET desktop build tools.'
    }
    $msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { throw 'MSBuild was not found. Install the .NET desktop build tools workload.' }
}

$systemDirectory = if ([Environment]::Is64BitProcess) { 'System32' } else { 'Sysnative' }
if (-not (Test-Path -LiteralPath (Join-Path $env:WINDIR "$systemDirectory\vcomp120.dll"))) {
    throw 'Install Microsoft Visual C++ 2013 Redistributable x64 (12.0.40664): https://aka.ms/highdpimfc2013x64enu . MGCB needs VCOMP120.DLL to import images.'
}

$buildArguments = @(
    (Join-Path $PSScriptRoot 'Sci-Opolis.sln'),
    '/t:Build', "/p:Configuration=$Configuration", '/nologo', '/v:minimal'
)
if (-not $NoRestore) {
    $buildArguments += '/restore', '/p:RestorePackagesConfig=true', "/p:RestoreConfigFile=$(Join-Path $PSScriptRoot 'NuGet.Config')"
}
& $msbuild @buildArguments
if ($LASTEXITCODE -ne 0) { throw "MSBuild failed (exit $LASTEXITCODE)." }

$projectDirectory = Join-Path $PSScriptRoot 'Sci-Opolis'
$outputDirectory = Join-Path $projectDirectory "bin\$Configuration"
$requiredFiles = @(
    'Sci-Opolis.exe', 'Sci-Opolis.exe.config',
    'Helper.dll', 'Animation2D.dll', 'MonoGame.Framework.dll',
    'x86\SDL2.dll', 'x86\soft_oal.dll', 'x64\SDL2.dll', 'x64\soft_oal.dll'
)
# Validate actual pipeline output, including music sidecars needed by Song playback.
foreach ($line in Get-Content -LiteralPath (Join-Path $projectDirectory 'Content\Content.mgcb')) {
    if ($line.StartsWith('/build:')) {
        $asset = $line.Substring(7)
        $requiredFiles += 'Content\' + [IO.Path]::ChangeExtension($asset, '.xnb')
        if ($asset.EndsWith('.mp3')) {
            $requiredFiles += 'Content\' + [IO.Path]::ChangeExtension($asset, '.ogg')
        }
    }
}
foreach ($relativePath in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $outputDirectory $relativePath) -PathType Leaf)) {
        throw "Build output is incomplete: $relativePath"
    }
}
Write-Host "Build and output checks passed: $outputDirectory"
Write-Host 'Launch Sci-Opolis.exe manually to check graphics, audio and gameplay.'
