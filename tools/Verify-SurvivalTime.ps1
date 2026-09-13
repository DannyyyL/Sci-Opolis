[CmdletBinding()]
param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug')

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$output = Join-Path $repositoryRoot "Sci-Opolis\bin\$Configuration"
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
$ownedName = 'SciOpolis-survival-' + [Guid]::NewGuid().ToString('N')
$testDirectory = Join-Path $tempRoot $ownedName
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$owned = $false
try {
    New-Item -ItemType Directory -Path $testDirectory -ErrorAction Stop | Out-Null
    $owned = $true
    foreach ($file in @('Sci-Opolis.exe', 'Sci-Opolis.exe.config', 'MonoGame.Framework.dll', 'Helper.dll', 'Animation2D.dll')) {
        Copy-Item -LiteralPath (Join-Path $output $file) -Destination $testDirectory
    }
    $runner = Join-Path $testDirectory 'SurvivalTimeChecks.exe'
    $references = @('Sci-Opolis.exe', 'MonoGame.Framework.dll', 'Helper.dll') | ForEach-Object { '/reference:' + (Join-Path $testDirectory $_) }
    & $csc /nologo /target:exe "/out:$runner" @references (Join-Path $PSScriptRoot 'SurvivalTimeChecks.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Could not compile survival-time checks.' }
    & $runner
    if ($LASTEXITCODE -ne 0) { throw 'Survival-time regression checks failed.' }
    & $runner --reload
    if ($LASTEXITCODE -ne 0) { throw 'Survival-time restart checks failed.' }
}
finally {
    if ($owned) {
        $resolved = (Resolve-Path -LiteralPath $testDirectory).ProviderPath
        if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($resolved) -cne $ownedName) {
            throw 'Refusing cleanup outside the owned test directory.'
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
