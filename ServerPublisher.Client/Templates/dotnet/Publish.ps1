$currentLocation=Get-Location

# Путь к файлу проекта
$clientSolutionPath=($currentLocation).Path + "\test\test.csproj"
# Путь к файлу профиля публикации
$clientPublishProfileName=($currentLocation).Path + "\test\Properties\PublishProfiles\TestRelease.pubxml"

#внешний ип адресс сервера
$publisher_ip = "127.0.0.1"
$publisher_port = 6583

#ключ шифрования по умолчанию, должен совпадать с ключем прописанном в конфиге на сервере
$publisher_cipher_key = "!{b1HX11R**"
#путь к файлу с публичными данными пользователя, будет произведена проверка относительного пути к key_storage папке в папке с приложением Publisher.Client и абсолютного пути
$publisher_auth_key_file = "testkey.pubuk"

#путь к папке куда после публикации будут складываться файлы
$publisher_release_dir = "Publish"
#id проекта который был присвоен при создании проекта на машине
$publisher_project_id = "c52f22ff-9999-9999-f018-51586ff742ff"

# Сборка проекта
dotnet publish $clientSolutionPath -c Release -o "$publisher_release_dir" /p:PublishProfile="$clientPublishProfileName"

# Evaluate success/failure
if($LASTEXITCODE -eq 0)
{
	Write-Host "build success" -ForegroundColor Green
		
	Publisher.Client.exe /action:publish /project_id:$publisher_project_id /directory:$publisher_release_dir /auth_key_path:$publisher_auth_key_file /ip:$publisher_ip /port:$publisher_port /cipher_out_key:$publisher_cipher_key /cipher_in_key:$publisher_cipher_key /has_compression:true
		
	Write-Host "Finished" -ForegroundColor Green
}
else
{
	Write-Host "build failed" -ForegroundColor Red
	[System.Console]::ReadKey()
}