Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$project_id = GetValue -text "Введите идентификатор проекта"

if ($IsWindows) {
    $execFile = "./ServerPublisher.Server.exe"
}
else {
    $execFile = "./ServerPublisher.Server"
}

& $execFile /action:reindexing /project_id:$project_id

Set-Location $currdir