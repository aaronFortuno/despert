# Reproduce the canonical QR module data from the author's existing SVG.
# Supports only this SVG's square-cell path format. No network or QR service.
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskSettings = New-Object Xml.XmlReaderSettings
$taskSettings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
$taskSettings.XmlResolver = $null
$taskReader = [Xml.XmlReader]::Create((Join-Path $taskRoot 'assets\lightning.svg'), $taskSettings)
$taskDocument = New-Object Xml.XmlDocument
$taskDocument.XmlResolver = $null
try { $taskDocument.Load($taskReader) } finally { $taskReader.Dispose() }
if ($taskDocument.DocumentElement.GetAttribute('viewBox') -ne '0 0 45 45') { throw 'Unexpected QR dimensions.' }
$taskPath = $taskDocument.DocumentElement.GetElementsByTagName('path')
if ($taskPath.Count -ne 1) { throw 'Expected one QR cell path.' }
$taskCells = $taskPath[0].GetAttribute('d')
$taskPattern = 'M([0-9]+) ([0-9]+)h1v1h-1z'
if ([regex]::Replace($taskCells, $taskPattern, '').Length -ne 0) { throw 'Unsupported QR path commands.' }
$taskRows = @()
for ($taskY = 0; $taskY -lt 45; $taskY++) { $taskRows += ,('0' * 45).ToCharArray() }
foreach ($taskMatch in [regex]::Matches($taskCells, $taskPattern)) {
    $taskX = [int]$taskMatch.Groups[1].Value
    $taskY = [int]$taskMatch.Groups[2].Value
    if ($taskX -lt 4 -or $taskY -lt 4 -or $taskX -gt 40 -or $taskY -gt 40) { throw 'QR quiet zone is invalid.' }
    $taskRows[$taskY][$taskX] = '1'
}
$taskOutput = ($taskRows | ForEach-Object { -join $_ }) -join "`n"
[IO.File]::WriteAllText((Join-Path $taskRoot 'assets\lightning.modules'), $taskOutput + "`n", [Text.Encoding]::ASCII)
Write-Output 'QR modules reproduced from the original local SVG.'
