$currentLocation=Get-Location

# dotnet Project file path
$clientSolutionPath=($currentLocation).Path + "\test\test.csproj"
# dotnet Publish profile file path
$clientPublishProfileName=($currentLocation).Path + "\test\Properties\PublishProfiles\TestRelease.pubxml"

#path to file with user information for authorize to deployed project on publisher.server, this file can located in "key_storage" with publisher.client executable folder, or relative/absolute path to current directory
$publisher_auth_key_file = "__publisher_auth_key_file__"
#project id for identity on publisher.server
$publisher_project_id = "__publisher_project_id__"

#remote ipaddr/port for connect to publisher.server project
$publisher_ip = "__publisher_ip__"
$publisher_port = 6583

#cipher key for initialize connection to publisher.server project
$publisher_out_cipher_key = "!{b1HX11R**"
$publisher_in_cipher_key = "!{b1HX11R**"

#dotnet publish output folder path
$publisher_release_dir = "Publish"

#path for upload to server, relative to project root
$project_relative_output_path = ""

# dotnet project build
dotnet publish $clientSolutionPath -c Release -o "$publisher_release_dir" /p:PublishProfile="$clientPublishProfileName"

# Evaluate success/failure
if($LASTEXITCODE -eq 0)
{
	Write-Host "build success" -ForegroundColor Green
		
	deployclient publish /project_id:$publisher_project_id /directory:$publisher_release_dir /auth_key_path:$publisher_auth_key_file /ip:$publisher_ip /port:$publisher_port /cipher_out_key:$publisher_out_cipher_key /cipher_in_key:$publisher_in_cipher_key /has_compression:true/output_relative_path:"$project_relative_output_path"
		
	Write-Host "Finished" -ForegroundColor Green
}
else
{
	Write-Host "build failed" -ForegroundColor Red
	[System.Console]::ReadKey()
}