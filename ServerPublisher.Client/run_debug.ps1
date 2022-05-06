$currdir = Get-Location

Set-location bin/Debug/net5.0/

.\Publisher.Client.exe /action:publish /project_id:5f260d74-f471-4c96-a48d-11d829c4f0d7 /directory:D:\Projects\AppFox\CRM\AppFoxCMS\Release /auth_key_path:Timka_4e67fd6c-c3bf-4e14-87e4-0e617b3a2f18.pubuk /ip:127.0.0.1 /port:6583 /cipher_out_key:"!{b1HX11R**" /cipher_in_key:"!{b1HX11R**"
    
Set-Location $currdir