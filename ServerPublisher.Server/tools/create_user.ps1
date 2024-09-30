Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$name = GetValue -text "Введите имя пользователя"

$project_id = GetValue -text "Введите идентификатор проекта(или пропустите и введите название проекта)"

if ($IsWindows) {
    $execFile = "./publisherserver.exe"
}
else {
    $execFile = "./publisherserver"
}

& $execFile /action:create_user /name:$name /project_id:$project_id

Set-Location $currdir