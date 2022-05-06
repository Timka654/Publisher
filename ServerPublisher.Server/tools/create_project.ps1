Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$name = GetValue -text "Введите название проекта"

$directory = GetValue -text "Введите путь к проекту"

$fullReplace = GetValue -text "Перезаписывать при публикации(true/false)" -type "bool"

$backup = GetValue -text "Резервное копирование при публикации(true/false)" -type "bool"

if ($IsWindows) {
    $execFile = "./ServerPublisher.Server.exe"
}
else {
    $execFile = "./ServerPublisher.Server"
}

& $execFile /action:create_project /name:$name /directory:$directory /full_replace:$fullReplace /backup:$backup

Set-Location $currdir