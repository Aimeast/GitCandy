$Env:Path = "${Env:ProgramFiles}\Git\bin;${Env:ProgramFiles(x86)}\Git\bin;C:\PortableGit\bin\;D:\PortableGit\bin\;$Env:Path"
$date = [DateTimeOffset]::Now.ToString([CultureInfo]::InvariantCulture)
$str = "Building: " + $date
$file = "Properties\Information"

if($args.Length -eq 1)
{
    Set-Location $args[0]
}

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
