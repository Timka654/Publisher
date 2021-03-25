Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$file_src = GetValue -text "Введите путь к приватному файлу"

$project_id = GetValue -text "Введите идентификатор проекта"

if ($IsWindows) {
    $execFile = "./Publisher.Server.exe"
}
else {
    $execFile = "./Publisher.Server"
}

& $execFile /action:add_user /path:$file_src /project_id:$project_id

Set-Location $currdir