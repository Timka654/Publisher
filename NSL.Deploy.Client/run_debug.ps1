$currdir = Get-Location

Set-location bin/Debug/net8.0

.\Publisher.Client.exe /action:publish /project_id:273a2073-5a66-43aa-ac08-c2e6e7838bb7 /directory:D:\Temp\publisher2\client\project1 /auth_key_path:dev_user_78d92859-cfe5-432d-9643-5c2e1f5f056e.pubuk /ip:127.0.0.1 /port:6583 /cipher_out_key:"!{b1HX11R**" /cipher_in_key:"!{b1HX11R**"
    
Set-Location $currdir