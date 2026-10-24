param([switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$version = '1.0.1'
$release = Join-Path $root ('dist\' + $version)
if (-not $SkipBuild) { & (Join-Path $root 'build.ps1') -Test }
if (-not (Test-Path -LiteralPath (Join-Path $release 'Despert.exe'))) { throw 'Primer cal compilar la versio.' }
# Documentation can change after the tested binary was built. Keep SkipBuild
# packages consistent with the source export without recompiling that binary.
foreach ($name in @('LICENSE','README.md','REVISIO.md')) {
    Copy-Item -LiteralPath (Join-Path $root $name) -Destination (Join-Path $release $name) -Force
}
$docs = Join-Path $release 'docs'
New-Item -ItemType Directory -Path $docs -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $root 'docs') -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $docs -Force }
$sums = @(Get-ChildItem -LiteralPath $release -Recurse -File | Where-Object { $_.Name -ne 'SHA256SUMS.txt' } | Sort-Object FullName | ForEach-Object {
    $relative = $_.FullName.Substring($release.Length + 1).Replace('\','/')
    (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $relative
})
Set-Content -LiteralPath (Join-Path $release 'SHA256SUMS.txt') -Value $sums -Encoding ascii
& (Join-Path $root 'verify-build.ps1') -ReleaseDirectory $release

$stage = Join-Path $root ('build\paquet-' + [Guid]::NewGuid().ToString('N'))
$source = Join-Path $stage 'source'
$portable = Join-Path $stage 'portable'
New-Item -ItemType Directory -Path $source,$portable -Force | Out-Null
$sourceNames = @('src','tests','tools','assets','docs','.github','build.ps1','prepare-toolchain.ps1','verify-build.ps1','package.ps1','toolchain.json','README.md','REVISIO.md','CHANGELOG.md','CONTRIBUTING.md','LICENSE','AGENTS.md','.gitignore','.gitattributes')
foreach ($name in $sourceNames) {
    Copy-Item -LiteralPath (Join-Path $root $name) -Destination $source -Recurse -Force
}
Get-ChildItem -LiteralPath $release -Force | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $portable -Recurse -Force }
$instructions = @(
    ('DESPERT ' + $version),
    '',
    'Open portable\Despert.exe. It starts inactive. Click the cup to toggle.',
    'Right-click: activation, optional Windows startup, About and Exit.',
    'To keep the cup visible, drag it from the hidden tray area to the taskbar.',
    'Keep the executable in a fixed location before enabling Windows startup.',
    '',
    'Source code and the MIT license are in source\.',
    'To rebuild and compare the same executable, from PowerShell:',
    '  cd source',
    '  .\prepare-toolchain.ps1',
    '  .\verify-build.ps1 -ReleaseDirectory ..\portable',
    '',
    'Preparation downloads two official tools with pinned versions and hashes.',
    'No tools are installed system-wide. Despert runs without network access.',
    'Details: source\README.md and source\docs\VERIFYING.md.'
)
Set-Content -LiteralPath (Join-Path $stage 'README.txt') -Value $instructions -Encoding UTF8
$outputRoot = Join-Path $root 'dist'
$base = 'Despert-' + $version
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $portable -Force | ForEach-Object { $_.FullName }) -DestinationPath (Join-Path $outputRoot ($base + '-portable.zip')) -Force
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $source -Force | ForEach-Object { $_.FullName }) -DestinationPath (Join-Path $outputRoot ($base + '-source.zip')) -Force
Compress-Archive -LiteralPath $portable,$source,(Join-Path $stage 'README.txt') -DestinationPath (Join-Path $outputRoot ($base + '-complete.zip')) -Force
$archiveSums = @('portable','source','complete') | ForEach-Object {
    $name = $base + '-' + $_ + '.zip'
    (Get-FileHash -LiteralPath (Join-Path $outputRoot $name) -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $name
}
Set-Content -LiteralPath (Join-Path $outputRoot ($base + '-SHA256SUMS.txt')) -Value $archiveSums -Encoding ascii
Write-Output ('Paquets creats a: ' + $outputRoot)
