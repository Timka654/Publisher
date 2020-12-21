$currdir = Get-Location

Set-location bin/Debug/net5.0/

.\Publisher.Client.exe /action:publish /project_id:cb188195-564c-4b04-abe8-0e43101b018d /directory:D:\Projects\AppFox\CRM\AppFoxCMS\Release /auth_key_path:D:\Temp\testPublisher\Publisher\users\publ\Timur_c6a1031e-5bb8-4383-a03d-f4b2fd7ab873.pubuk /ip:127.0.0.1 /port:6583 /cipher_out_key:"!{b1HX11R**" /cipher_in_key:"!{b1HX11R**" /success_args:"/newVersion:0.0.0.0 /os:Windows"
    
Set-Location $currdir