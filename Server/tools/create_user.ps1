Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$name = GetValue -text "Введите имя пользователя"

$project_id = GetValue -text "Введите идентификатор проекта(или пропустите и введите название проекта)"

$project_name = GetValue -text "Введите название проекта"

if ($IsWindows) {
    $execFile = "./Publisher.Server.exe"
}
else {
    $execFile = "./Publisher.Server"
}

& $execFile /action:create_user /name:$name /project_id:$project_id /project_name:$project_name

Set-Location $currdir