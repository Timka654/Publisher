Import-Module "./utils.psm1"

Set-Location ..

$currLocation = (Get-Location).Path

if ([System.IO.File]::Exists([System.IO.Path]::Combine($currLocation,"ServerPublisher.Server.deps.json")) -eq $false) {
	Write-Error "Server files not found in current path $currLocation"
	exit
}

if($IsLinux)
{
	$setupPath = "/opt/Publisher";
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
		$setupPath = GetValue -text "Install path"
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

$configPath = [System.IO.Path]::Combine($setupPath, "ServerSettings.json")

$serverPort = 6583;

if($args.Contains("default") -eq $false)
{
	$serverPort = GetValue -text "Publisher port (default:6583)" -type "int"
}

Move-Item -Path "$currLocation/*" -Destination $setupPath -Force

$a = "{ ""server"": { ""io.port"": $serverPort } }"

$a | set-content $configPath

if ($IsWindows) {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("Path");
	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("Path", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}

	Write-Host "Invoke ./setup-service-windows for install publisher windows server with Administrator rights"
	Write-Host "or './setup-service-windows ""service_name""'"
	Write-Host "default './setup-service-windows default' equals './setup-service-windows ""Publisher Server""'"
}
else {
	$PathEnv = [System.Environment]::GetEnvironmentVariable("PATH");

	if ($PathEnv.Contains($setupPath, [StringComparison]::OrdinalIgnoreCase) -eq $false) {
		[System.Environment]::SetEnvironmentVariable("PATH", "$setupPath;$PathEnv", [System.EnvironmentVariableTarget]::Machine)
	}
	$execFilePath = [System.IO.Path]::Combine($setupPath, "ServerPublisher.Server")
	chmod +x $execFilePath
	ln -sf $execFilePath /bin/pubs

	Write-Host "Invoke 'sudo pwsh setup-service-linux.ps1' for install publisher linux server with sudo"
	Write-Host "or 'sudo pwsh setup-service-linux.ps1 ""service_name"" ""service_file_name""'"
	Write-Host "default 'sudo pwsh setup-service-linux.ps1 default' equals 'sudo pwsh setup-service-linux.ps1 ""Publisher Server"" ""publisher.service""'"
}

Write-Host "Please change path to cd $setupPath/tools/"


cd $setupPath/tools/