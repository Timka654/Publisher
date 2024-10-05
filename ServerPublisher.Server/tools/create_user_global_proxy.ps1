Import-Module "./utils.psm1"

$currdir = Get-Location

Set-Location ..

if($args.Count -ge 1)
{
    $name = args[0]
}
else
{
    $name = GetValue -text "User name" -defaultValue "Publisher Server"
}

if ($IsWindows) {
    $execFile = "./publisherserver.exe"
}
else {
    $execFile = "./publisherserver"
}

& $execFile /action:create_user /name:$name /global /proxy

Set-Location $currdir