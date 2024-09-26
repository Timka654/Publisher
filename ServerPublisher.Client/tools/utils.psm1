function GetValue {
	param([string] $text, [string] $type = "string", [string] $defaultValue = $null)
	$answer = ""
	$value = ""
	$tryValue = $null;
	while($true)
	{
		$value = Read-Host $text

		if(([string]::IsNullOrEmpty($value) -eq $true) -and ($defaultValue -ne $null))
		{
			$value = $defaultValue
		}

		if(($type -eq "int") -and ([int]::TryParse($value, [ref]$tryValue) -eq $false))
		{
			Write-Host "Error, cannot converted ""$value"" to $type"
			continue
		}
		elseif(($type -eq "double") -and ([double]::TryParse($value, [ref]$tryValue) -eq $false))
		{
			Write-Host "Error, cannot converted ""$value"" to $type"
			continue
		}
		elseif(($type -eq "bool") -and ([bool]::TryParse($value, [ref]$tryValue) -eq $false))
		{
			Write-Host "Error, cannot converted ""$value"" to $type"
			continue
		}

		$answer = Read-Host "Value setted to ""$value"" (y - continue, n - cancel, c - close)"

		if($answer -eq "y")
		{
			return $value
		}
		elseif($answer -eq "c")
		{
			exit
		}
	}
}