﻿Import-Module "./utils.psm1"

$currLocation = [System.IO.Path]::Combine((Get-Location).Path, "..")

if ([System.IO.File]::Exists([System.IO.Path]::Combine($currLocation,"publisherserver.deps.json")) -eq $false) {
	Write-Error "Server files not found in current path $currLocation"
	exit
}

if ([System.IO.File]::Exists("$currLocation/installed") -eq $true) {
    Write-Error "Client files already installed! No need more actions"
    exit
}

if($IsLinux)
{
	$setupPath = "/etc/publisherserver";
}
elseif($IsWindows)
{
	$setupPath = "C:\Program Files\PublisherServer"
}
elseif($IsMacOS)
{
	throw "not supported platform";
}

if($args.Contains("default") -eq $false)
{
	do {
		$setupPath = GetValue -text "Install path" -defaultValue $setupPath

		if (([System.IO.Directory]::Exists($setupPath) -eq $true) -and ([System.IO.Directory]::GetFiles($setupPath).Count -ne 0)) {
			Write-Host "Install folder ""$setupPath"" must be empty"
			continue
		}
		else {
			break
		}
	}
	while ($true);
}

$serverPort = 6583;

if($args.Contains("default") -eq $false)
{
	$serverPort = GetValue -text "Publisher port" -type "int" -defaultValue "6583"
}

Copy-Item -Recurse -Filter *.* -Path $currLocation -Destination $setupPath -Force


if([System.IO.File]::Exists([System.IO.Path]::Combine($setupPath, "..", "installed")) -eq $false) {
    New-Item -Path $setupPath -Name "installed" -ItemType "file"
}


if ($IsWindows) {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("Path");
	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("Path", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}
	$execFilePath = [System.IO.Path]::Combine($setupPath, "publisherserver.exe")

	Write-Host "Invoke ./setup-service-windows for install publisher windows server with Administrator rights"
	Write-Host "or './setup-service-windows ""service_name""'"
	Write-Host "default './setup-service-windows default' equals './setup-service-windows ""Publisher Server""'"
}
else {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("PATH");

	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("PATH", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}
	$execFilePath = [System.IO.Path]::Combine($setupPath, "publisherserver")
	chmod +x $execFilePath
	ln -sf $execFilePath /bin/publs
	ln -sf $execFilePath /bin/publsrv
	ln -sf $execFilePath /bin/publishsrv

	Write-Host "Invoke 'sudo pwsh setup-service-linux.ps1' for install publisher linux server with sudo"
	Write-Host "or 'sudo pwsh setup-service-linux.ps1 ""service_name"" ""service_file_name""'"
	Write-Host "default 'sudo pwsh setup-service-linux.ps1 default' equals 'sudo pwsh setup-service-linux.ps1 ""Publisher Server"" ""publisher.service""'"
}

Write-Host "Please change path to cd $setupPath/tools/"

cd $setupPath/tools/

Remove-Item -Path "$currLocation" -Recurse -Force

& $execFilePath /action:cdefault /y
& $execFilePath /action:cset /path:"publisher/server/io/port" /value:$serverPort /y