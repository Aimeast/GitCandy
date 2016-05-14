# PreBuild.ps1
#
# For gerenrate the tracking information and make a lazy cache

if($args.Length -eq 1)
{
    Set-Location $args[0]
}

# For lazy cache

$str = ""
$files = "ArchiverAccessor.cs",
    "BlameAccessor.cs",
    "CommitsAccessor.cs",
    "ContributorsAccessor.cs",
    "HistoryDivergenceAccessor.cs",
    "LastCommitAccessor.cs",
    "RepositorySizeAccessor.cs",
    "ScopeAccessor.cs",
    "SummaryAccessor.cs"
foreach($file in $files)
{
    $path = "Git\" + $file
    $sha = Get-FileHash -Path $path -Algorithm SHA1
    $str += $sha.Hash + [Environment]::NewLine
}
Set-Content "Properties\CacheVersion" $str.TrimEnd()

# For tracking information

$Env:Path = "${Env:ProgramFiles}\Git\bin;${Env:ProgramFiles(x86)}\Git\bin;C:\PortableGit\bin\;D:\PortableGit\bin\;$Env:Path"
$date = [DateTimeOffset]::Now.ToString([CultureInfo]::InvariantCulture)
$str = "Building: " + $date
$file = "Properties\Information"

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
