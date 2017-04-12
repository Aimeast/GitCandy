# PreBuild.ps1
#
# For generate the tracking information

Set-Location $PSScriptRoot

# For tracking information

$Env:Path = "${Env:ProgramFiles}\Git\bin;${Env:ProgramFiles(x86)}\Git\bin;C:\PortableGit\bin\;D:\PortableGit\bin\;$Env:Path"
$date = [DateTimeOffset]::Now.ToString([CultureInfo]::InvariantCulture)
$str = "Building: " + $date
$file = [System.IO.Path]::GetFullPath($PSScriptRoot + "\..\GitCandy\Properties\Information")

"Writting to " + $file

Set-Content $file $str
Add-Content $file ""
git log -1 | Add-Content $file
Add-Content $file ""
Add-Content $file "Author:"
git config user.name | Add-Content $file
git config user.email | Add-Content $file
Add-Content $file ""
Add-Content $file "Status:"
git status -b -s | Add-Content $file
Add-Content $file "End."

Get-Content $file
