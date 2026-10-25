param([string]$PackageCache)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskArchives = @(Get-ChildItem -LiteralPath (Join-Path $taskRoot 'dist') -Filter 'Despert-*-complete.zip' -File)
if ($taskArchives.Count -ne 1) {
    throw 'Expected exactly one complete archive in dist. Use a fresh checkout/output directory for this check.'
}
if (-not $PackageCache) { $PackageCache = Join-Path $taskRoot '.tools\downloads' }
$taskCache = [IO.Path]::GetFullPath($PackageCache)
if (-not (Test-Path -LiteralPath $taskCache -PathType Container)) { throw 'The verified build-tool package cache is missing.' }
$taskExport = Join-Path $taskRoot ('build\export-verification-' + [Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $taskArchives[0].FullName -DestinationPath $taskExport
$taskSource = Join-Path $taskExport 'source'
& (Join-Path $taskSource 'prepare-toolchain.ps1') -PackageCache $taskCache
& (Join-Path $taskSource 'verify-build.ps1') -ReleaseDirectory (Join-Path $taskExport 'portable')
Write-Output ('Exported source verified in: ' + $taskExport)
