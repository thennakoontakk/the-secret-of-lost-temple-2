param(
    [string]$Adb = "adb",
    [string]$Serial = "",
    [string]$Package = "com.UnityTechnologies.com.unity.template.urpblank",
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "research-backups")
)
$ErrorActionPreference = "Stop"
if ($Package -notmatch '^[A-Za-z][A-Za-z0-9_.]+$') { throw "Invalid Android package ID." }
$deviceArgs = @()
if ($Serial) { $deviceArgs = @("-s", $Serial) }
& $Adb @deviceArgs get-state
if ($LASTEXITCODE -ne 0) { throw "Connect and authorize one phone, or specify -Serial." }
$backupName = (Get-Date -Format "yyyyMMdd-HHmmss") + "-" + [Guid]::NewGuid().ToString("N").Substring(0,8)
$destination = Join-Path $OutputDirectory $backupName
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$remote = "/sdcard/Android/data/$Package/files/research_v1"
& $Adb @deviceArgs pull $remote $destination
if ($LASTEXITCODE -ne 0) { throw "Copy failed. Phone data was not changed. Verify the app has saved data and check the displayed export path." }
$records = Join-Path $destination "research_v1/records.json"
if (-not (Test-Path -LiteralPath $records)) { throw "No records.json found in the copy. Verify the source path before using this backup." }
$data = Get-Content -LiteralPath $records -Raw | ConvertFrom-Json
Write-Host ("Backup complete: {0}; saved sessions: {1}" -f $destination, @($data.sessions).Count)
Write-Host "Verify the exported CSV files, then keep a second backup. Nothing was removed from the phone."
