param(
    [string]$ReleaseDirectory,
    [string]$CompilerPath
)
$ErrorActionPreference = 'Stop'
if (-not $ReleaseDirectory) { $ReleaseDirectory = Join-Path $PSScriptRoot 'dist\1.0.1' }
$releaseRoot = [IO.Path]::GetFullPath($ReleaseDirectory)
$manifest = Get-Content -LiteralPath (Join-Path $releaseRoot 'BUILDINFO.json') -Raw | ConvertFrom-Json
if (-not $manifest.deterministic) { throw 'La versio publicada no es va compilar en mode reproduible.' }
$published = Join-Path $releaseRoot 'Despert.exe'
$publishedHash = (Get-FileHash -LiteralPath $published -Algorithm SHA256).Hash.ToLowerInvariant()
if ($publishedHash -ne $manifest.output.sha256) { throw 'El SHA-256 publicat no coincideix amb BUILDINFO.json.' }
$sumFile = Join-Path $releaseRoot 'SHA256SUMS.txt'
if (Test-Path -LiteralPath $sumFile) {
    foreach ($line in Get-Content -LiteralPath $sumFile) {
        if (-not $line.Trim()) { continue }
        if ($line -notmatch '^([a-fA-F0-9]{64})  (.+)$') { throw 'Format SHA256SUMS invalid.' }
        $expected = $Matches[1]
        $relative = $Matches[2]
        $target = [IO.Path]::GetFullPath((Join-Path $releaseRoot $relative))
        if (-not $target.StartsWith($releaseRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Ruta fora del paquet al manifest de hashes.'
        }
        if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $expected) { throw ('Fitxer alterat: ' + $relative) }
    }
}
foreach ($buildInput in $manifest.inputs) {
    $source = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot $buildInput.file))
    if (-not $source.StartsWith($PSScriptRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Ruta de font invalida.' }
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $buildInput.sha256) { throw ('La font no coincideix amb la versio: ' + $buildInput.file) }
}
$output = Join-Path $PSScriptRoot ('build\verificacio-' + [Guid]::NewGuid().ToString('N'))
$buildArgs = @{ OutputDirectory = $output }
if ($CompilerPath) { $buildArgs.CompilerPath = $CompilerPath }
& (Join-Path $PSScriptRoot 'build.ps1') @buildArgs
$rebuiltHash = (Get-FileHash -LiteralPath (Join-Path $output 'Despert.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($rebuiltHash -ne $publishedHash) {
    throw ('Els binaris difereixen. Comprova la versio i els SHA-256 del compilador i les biblioteques a BUILDINFO.json. Publicat: ' + $publishedHash + '; compilat: ' + $rebuiltHash)
}
Write-Output ('VERIFICAT: executable identic byte per byte. SHA-256: ' + $publishedHash)
Write-Output ('Compilacio conservada a: ' + $output)
