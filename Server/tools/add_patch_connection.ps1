Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

$ip_addr = GetValue -text "Введите ип патч сервера"
$port = GetValue -text "Введите порт патч сервера"
$input_cipher_key = GetValue -text "Введите ключ шифрования входящего трафика патч сервера"
$output_cipher_key = GetValue -text "Введите ключ шифрования исходящего трафика патч сервера"
$identity_name = GetValue -text "Введите название публичнного ключа для авторизации"

$project_id = GetValue -text "Введите идентификатор проекта"

if ($IsWindows) {
    $execFile = "./Publisher.Server.exe"
}
else {
    $execFile = "./Publisher.Server"
}

& $execFile /action:add_patch_connection /project_id:$project_id /ip_address:$ip_addr /port:$port /input_cipher_key:$input_cipher_key /output_cipher_key:$output_cipher_key /identity_name:$identity_name

Set-Location $currdir