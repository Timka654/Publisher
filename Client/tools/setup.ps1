Import-Module "./utils.psm1"

$backLocation = (Get-Location).Path

Set-Location ..

$currLocation = (Get-Location).Path

if ([System.IO.File]::Exists("Publisher.Client.deps.json") -eq $false) {
    Write-Error "Client files not found in current path $currLocation"
    exit
}

$setupPath = ""
do {
    $setupPath = GetValue -text "Install path"
    if (([System.IO.Directory]::Exists($setupPath) -eq $true) -and ([System.IO.Directory]::GetFiles($setupPath).Count -ne 0)) {
        Write-Host "Install path ""$setupPath"" must be empty"
        continue
    }
    else {
        break
    }
}
while ($true);

Copy-Item -Path $currLocation -Destination $setupPath -Recurse -Container

Set-Location $setupPath

Write-Host "Invoke Publisher.Client with args"


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
}

Set-Location $backLocation

# Set-Location $setupPath/tools/