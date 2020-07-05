Import-Module "./utils.psm1"

Set-Location ..

$currLocation = (Get-Location).Path

if ([System.IO.File]::Exists("Publisher.Server.deps.json") -eq $false) {
	Write-Error "Server files not found in current path $currLocation"
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

$configPath = [System.IO.Path]::Combine($setupPath, "ServerSettings.json")

$serverPort = GetValue -text "Publisher port (default:6583)" -type "int"

Copy-Item -Path $currLocation -Destination $setupPath -Recurse -Container

$a = Get-Content $configPath -raw | ConvertFrom-Json -Depth 10
$a.server."io.port" = $serverPort
$a | ConvertTo-Json -depth 32 | set-content $configPath

Set-Location $setupPath
if ($IsWindows) {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("Path");
	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("Path", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}

	Write-Host "Invoke ./setup-service-windows for install publisher windows server with Administrator"
}
else {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("PATH");

	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("PATH", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}
	$execFilePath = [System.IO.Path]::Combine($setupPath, "Publisher.Server")
	chmod +x $execFilePath
	Write-Host "Invoke ./setup-service-linux for install publisher linux server with sudo"
}
Set-Location $setupPath/tools/