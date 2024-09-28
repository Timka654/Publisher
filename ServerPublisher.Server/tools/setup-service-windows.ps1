Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$serviceName = "Publisher Server"

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
}

$setupPath=Get-Location

$execFile = [System.IO.Path]::Combine($setupPath,"publisherserver.exe")

if((Test-Path -Path $execFile) -eq $false)
{
    Write-Error "Publisher server exec file ""$execFile"" not found"
    exit
}

sc.exe create $serviceName binPath="""$execFile /service""" start=auto

Set-Location $currdir