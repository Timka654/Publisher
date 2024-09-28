Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$serviceName = "Publisher Server"
$serviceFileName = "publisher.service";

if($args.Contains("default") -eq $false)
{
    if($args.Count -ge 1)
    {
        $serviceName = args[0]
    }
    else
    {
        $serviceName = GetValue -text "Service name(default:Publisher Server)"
    }


    if($args.Count -ge 2)
    {
        $serviceFileName = args[1]
    }
    else
    {
        $serviceFileName = GetValue -text "Service file name(default:publisher.service)"
    }
}

$setupPath=Get-Location

$execFile = [System.IO.Path]::Combine($setupPath,"publisherserver")
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
ExecStart=$execFile /service
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
");

sudo systemctl enable $serviceFileName

Write-Host "Service ""$serviceFileName"" enabled, print ""systemctl start $serviceFileName"" for start now"

Set-Location $currdir