Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$project_id = GetValue -text "Введите идентификатор проекта"

if ($IsWindows) {
    $execFile = "./Publisher.Server.exe"
}
else {
    $execFile = "./Publisher.Server"
}

& $execFile /action:check_scripts /project_id:$project_id

Set-Location $currdir