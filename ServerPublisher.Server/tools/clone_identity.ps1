Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$source_project_id = GetValue -text "Введите идентификатор проекта (источник)"

$dest_project_id = GetValue -text "Введите идентификатор проекта (конечный)"

if ($IsWindows) {
    $execFile = "./publisherserver.exe"
}
else {
    $execFile = "./publisherserver"
}

& $execFile /action:clone_identity /source_project_id:$source_project_id /destination_project_id:$dest_project_id

Set-Location $currdir