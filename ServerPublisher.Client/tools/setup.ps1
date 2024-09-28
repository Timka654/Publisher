#Requires -RunAsAdministrator
Import-Module "./utils.psm1"

$backLocation = (Get-Location).Path

Set-Location ..

$currLocation = (Get-Location).Path

Set-Location $backLocation

if ([System.IO.File]::Exists("$currLocation/publisherclient.pdb") -eq $false) {
    Write-Error "Client files not found in current path $currLocation"
    exit
}

if ([System.IO.File]::Exists("$currLocation/installed") -eq $true) {
    Write-Error "Client files already installed! No need more actions"
    exit
}

$setupPath = ""
do {
    $setupPath = GetValue -text "Install path" -defaultValue "C:\Program Files\Publisher.Client"
    if (([System.IO.Directory]::Exists($setupPath) -eq $true) -and ([System.IO.Directory]::GetFiles($setupPath).Count -ne 0) -and (([System.IO.File]::Exists("$setupPath/publisherclient.pdb") -eq $false))) {
        Write-Host "Install path ""$setupPath"" must be empty"
        continue
    }
    else {
        break
    }
}
while ($true);

if (-Not (Test-Path $setupPath))
{
    New-Item -Path $setupPath -ItemType "directory"
}

Copy-Item -Path "$currLocation/*" -Destination $setupPath -Recurse -Container -Force

if([System.IO.Directory]::Exists([System.IO.Path]::Combine($setupPath, "key_storage")) -eq $false) {
    New-Item -Path $setupPath -Name "key_storage" -ItemType "directory"
}

if([System.IO.File]::Exists([System.IO.Path]::Combine($setupPath, "..", "installed")) -eq $false) {
    New-Item -Path $setupPath -Name "installed" -ItemType "file"
}

Set-Location $setupPath

Write-Host "Invoke ""publisherclient"" with args for produce project file to remote server"

if ($IsWindows) {

    $PathEnv = [System.Environment]::GetEnvironmentVariable("Path");

    if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
        [System.Environment]::SetEnvironmentVariable("Path", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
    }
}
else {

    $PathEnv = [System.Environment]::GetEnvironmentVariable("PATH");

    if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
        [System.Environment]::SetEnvironmentVariable("PATH", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
    }
    
	$execFilePath = [System.IO.Path]::Combine($setupPath, "Publisher.Client")

	chmod +x $execFilePath

    ln -s "$setupPath/Publisher.Client" /bin/publc
}

Set-Location $backLocation

# Set-Location $setupPath/tools/