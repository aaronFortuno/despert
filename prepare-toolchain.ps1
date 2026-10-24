param([string]$PackageCache)
$ErrorActionPreference = 'Stop'
$settings = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'toolchain.json') -Raw | ConvertFrom-Json
$toolRoot = Join-Path $PSScriptRoot '.tools'
if (-not $PackageCache) { $PackageCache = Join-Path $toolRoot 'downloads' }
New-Item -ItemType Directory -Path $toolRoot,$PackageCache -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
# Changes TLS settings only for this PowerShell process, not machine policy.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
foreach ($package in $settings.packages) {
    $destination = [IO.Path]::GetFullPath((Join-Path $toolRoot $package.directory))
    if (-not $destination.StartsWith($toolRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Ruta de paquet invalida.' }
    $archive = Join-Path $PackageCache $package.archive
    if (-not (Test-Path -LiteralPath $archive)) {
        Write-Output ('Descarregant ' + $package.id + ' ' + $package.version + ' des de NuGet oficial...')
        Invoke-WebRequest -UseBasicParsing -Uri $package.url -OutFile $archive
    }
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $package.sha256) { throw ('SHA-256 incorrecte: ' + $package.id) }
    $marker = Join-Path $destination 'package-sha256.txt'
    if (-not (Test-Path -LiteralPath $marker)) {
        if (Test-Path -LiteralPath $destination) { throw ('Extraccio parcial: conserva o canvia el nom de la carpeta abans de repetir: ' + $destination) }
        [IO.Compression.ZipFile]::ExtractToDirectory([IO.Path]::GetFullPath($archive), $destination)
        Set-Content -LiteralPath $marker -Value $package.sha256 -Encoding ascii
    }
    if ((Get-Content -LiteralPath $marker -Raw).Trim() -ne $package.sha256 -or -not (Test-Path -LiteralPath (Join-Path $destination $package.entry))) { throw ('Paquet local invalid: ' + $package.id) }
    Write-Output ('Preparat i verificat: ' + $package.id + ' ' + $package.version)
}
Write-Output 'Eines locals preparades. No s''ha instal.lat res al sistema. Ja pots executar build.ps1.'
