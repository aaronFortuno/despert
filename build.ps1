param(
    [switch]$Test,
    [string]$CompilerPath,
    [string]$OutputDirectory,
    [switch]$Legacy,
    [switch]$RegenerateIcons
)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$version = '1.0.1'
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot ('dist\' + $version) }
$destination = [IO.Path]::GetFullPath($OutputDirectory)
$buildOutput = Join-Path $projectRoot 'build'
New-Item -ItemType Directory -Path $destination,$buildOutput -Force | Out-Null

if (-not $CompilerPath -and -not $Legacy) {
    $CompilerPath = Join-Path $projectRoot '.tools\compiler-4.14.0\tasks\net472\csc.exe'
}
if ($Legacy -and -not $CompilerPath) {
    $CompilerPath = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not $CompilerPath -or -not (Test-Path -LiteralPath $CompilerPath)) {
    throw 'Executa primer prepare-toolchain.ps1 per obtenir el compilador oficial amb versio fixa. Vegeu docs/VERIFICACIO.md. -Legacy permet compilar sense garantia de reproduccio binaria.'
}
$compiler = [IO.Path]::GetFullPath($CompilerPath)
$framework = if ($Legacy) { Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319' } else { Join-Path $projectRoot '.tools\references-1.0.3\build\.NETFramework\v4.8' }
$referenceNames = @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')
$referencePaths = @($referenceNames | ForEach-Object { Join-Path $framework $_ })
foreach ($reference in $referencePaths) {
    if (-not (Test-Path -LiteralPath $reference)) { throw ('Falta la biblioteca .NET Framework: ' + $reference) }
}
$common = @('/nologo','/noconfig','/nostdlib+','/utf8output','/codepage:65001','/langversion:5','/warn:4','/warnaserror+','/optimize+','/debug-')
foreach ($reference in $referencePaths) { $common += '/reference:' + $reference }
if (-not $Legacy) { $common += @('/deterministic+', ('/pathmap:' + $projectRoot + '=/_/Despert')) }

if ($RegenerateIcons) {
    $iconBuilder = Join-Path $buildOutput 'BuildIcons.exe'
    & $compiler @common /target:exe ('/out:' + $iconBuilder) (Join-Path $projectRoot 'tools\BuildIcons.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la preparacio de les icones.' }
    & $iconBuilder (Join-Path $projectRoot 'assets')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la generacio de les icones.' }
}
# ICOs are canonical source assets. Normal builds do not rerender them with
# environment-dependent GDI+/PNG encoders.
[string[]]$sources = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })
[Array]::Sort($sources, [StringComparer]::Ordinal)
if (-not $sources.Count) { throw 'No hi ha fonts C#.' }
$iconPath = Join-Path $projectRoot 'assets'
$resourceArgs = @()
$resourceArgs += '/resource:' + (Join-Path $iconPath 'lightning.modules') + ',Despert.Lightning.modules'
foreach ($state in @('Inactive','Active')) {
    foreach ($theme in @('Light','Dark')) {
        $file = Join-Path $iconPath ($state.ToLowerInvariant() + '-' + $theme.ToLowerInvariant() + '.ico')
        $resourceArgs += '/resource:' + $file + ',Despert.' + $state + '.' + $theme + '.ico'
    }
}
& $compiler @common /platform:anycpu /target:winexe ('/win32manifest:' + (Join-Path $iconPath 'app.manifest')) ('/win32icon:' + (Join-Path $iconPath 'app.ico')) @resourceArgs ('/out:' + (Join-Path $destination 'Despert.exe')) @sources
if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de Despert.' }

function FileRecord([string]$file, [string]$label) {
    [ordered]@{ file = $label; sha256 = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$inputFiles = @($sources)
$inputFiles += Join-Path $iconPath 'app.manifest'
$inputFiles += Join-Path $iconPath 'lightning.modules'
$inputFiles += @(Get-ChildItem -LiteralPath $iconPath -Filter '*.ico' | Where-Object { $_.Name -notin @('active.ico','inactive.ico') } | ForEach-Object { $_.FullName })
[string[]]$inputFiles = $inputFiles
[Array]::Sort($inputFiles, [StringComparer]::Ordinal)
$inputs = @($inputFiles | ForEach-Object { FileRecord $_ ($_.Substring($projectRoot.Length + 1).Replace('\','/')) })
$dependencies = @()
foreach ($name in @('Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll','System.Collections.Immutable.dll','System.Reflection.Metadata.dll')) {
    $file = Join-Path (Split-Path -Parent $compiler) $name
    if (Test-Path -LiteralPath $file) { $dependencies += FileRecord $file $name }
}
$compilerVersion = if ($Legacy) { (Get-Item -LiteralPath $compiler).VersionInfo.FileVersion } else { (& $compiler -version | Out-String).Trim() }
$info = [ordered]@{
    format = 1; version = $version; deterministic = (-not $Legacy)
    compiler = [ordered]@{ version = $compilerVersion; sha256 = (Get-FileHash -LiteralPath $compiler).Hash.ToLowerInvariant(); dependencies = $dependencies }
    references = @($referencePaths | ForEach-Object { FileRecord $_ ([IO.Path]::GetFileName($_)) })
    toolchain = (Get-Content -LiteralPath (Join-Path $projectRoot 'toolchain.json') -Raw | ConvertFrom-Json)
    inputs = $inputs
    output = FileRecord (Join-Path $destination 'Despert.exe') 'Despert.exe'
    notes = 'Byte identity requires identical effective compiler inputs. No timestamps or machine paths are used as build identifiers. Archive bytes are not claimed reproducible.'
}
$info | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $destination 'BUILDINFO.json') -Encoding UTF8
foreach ($name in @('LICENSE','README.md','REVISIO.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination (Join-Path $destination $name)
}
New-Item -ItemType Directory -Path (Join-Path $destination 'assets') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $iconPath 'preview.png') -Destination (Join-Path $destination 'assets\preview.png')

if ($Test) {
    $logicExe = Join-Path $buildOutput 'LogicTests.exe'
    & $compiler @common /target:exe ('/out:' + $logicExe) (Join-Path $projectRoot 'src\Logic.cs') (Join-Path $projectRoot 'tests\LogicTests.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de les proves.' }
    & $logicExe
    if ($LASTEXITCODE -ne 0) { throw 'Han fallat les proves.' }
    $startupChecks = Join-Path $buildOutput 'StartupTests.exe'
    & $compiler @common /target:exe ('/out:' + $startupChecks) (Join-Path $projectRoot 'tests\StartupTests.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de les proves de ruta dinici.' }
    & $startupChecks (Join-Path $destination 'Despert.exe')
    if ($LASTEXITCODE -ne 0) { throw 'Han fallat les proves de ruta dinici.' }
    $architectures = if ([Environment]::Is64BitOperatingSystem) { @('x64','x86') } else { @('x86') }
    foreach ($architecture in $architectures) {
        $interopExe = Join-Path $buildOutput ('InteropTests-' + $architecture + '.exe')
        & $compiler @common /target:exe ('/platform:' + $architecture) ('/out:' + $interopExe) (Join-Path $projectRoot 'src\Logic.cs') (Join-Path $projectRoot 'src\Native.cs') (Join-Path $projectRoot 'src\NativeMenu.cs') (Join-Path $projectRoot 'src\Strings.cs') (Join-Path $projectRoot 'src\Translations.cs') (Join-Path $projectRoot 'tests\InteropTests.cs')
        if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de les proves Win32.' }
        & $interopExe
        if ($LASTEXITCODE -ne 0) { throw 'Ha fallat el contracte binari Win32.' }
    }
    $releaseChecks = Join-Path $buildOutput 'ReleaseChecks.exe'
    & $compiler @common /target:exe ('/win32manifest:' + (Join-Path $iconPath 'app.manifest')) ('/out:' + $releaseChecks) (Join-Path $projectRoot 'tests\ReleaseChecks.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de les comprovacions de recursos.' }
    & $releaseChecks (Join-Path $destination 'Despert.exe') $buildOutput
    if ($LASTEXITCODE -ne 0) { throw 'Han fallat les comprovacions de recursos i avisos de sessio.' }
    $languageChecks = Join-Path $buildOutput 'LanguageChecks.exe'
    & $compiler @common /target:exe ('/out:' + $languageChecks) (Join-Path $projectRoot 'tests\LanguageChecks.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Ha fallat la compilacio de les comprovacions didioma.' }
    & $languageChecks (Join-Path $destination 'Despert.exe') $buildOutput
    if ($LASTEXITCODE -ne 0) { throw 'Han fallat les comprovacions didioma i dialeg.' }
}
Write-Output ('Preparat: ' + (Join-Path $destination 'Despert.exe'))
