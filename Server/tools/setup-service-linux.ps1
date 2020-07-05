Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$serviceName = GetValue -text "Service name(default:Publisher Server)"
$serviceFileName = GetValue -text "Service file name(default:publisher.service)"
$setupPath=Get-Location


$execFile = [System.IO.Path]::Combine($setupPath,"Publisher.Server")
$dir = Split-Path -Path $execFile
if((Test-Path -Path $execFile) -eq $false)
{
    Write-Error "Publisher server exec file ""$execFile"" not found"
    exit
}

[System.IO.File]::WriteAllText("/etc/systemd/system/$serviceFileName", "
[Unit]
Description=$serviceName

[Service]
WorkingDirectory=$dir
ExecStart=$execFile
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
");

sudo systemctl enable $serviceFileName

Write-Host "Enabled service $serviceFileName, print ""systemctl start $serviceFileName"" for start now"

Set-Location $currdir