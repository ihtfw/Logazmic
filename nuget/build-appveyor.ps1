$projDirName = "Logazmic.Integration"
$projName = "Logazmic.Integration"

$curDir = (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$slnDir = $curDir + "\..\src\"
$projDir = $slnDir + $projDirName+ "\"

$version = (Get-Date -format yyyy.M.d) + "." + $env:appveyor_build_number;

$nuspecPath = ($curDir + "\" + $projName + ".nuspec")
$nupkgPath = ($curDir + "\" + $projName + "." + $version + ".nupkg")

Set-Location $curDir

Write-Host "------------------Setting nuspec version($vesrsion)------------------"
[xml]$nuspec = Get-Content $nuspecPath
$nuspec.package.metadata.version = $version
$nuspec.Save($nuspecPath)

Write-Host "------------------Create nupkg------------------"
& ($curDir + "\bin\nuget.exe") pack $nuspecPath

Write-Host "------------------Move nupkg------------------"
$releasesDir = $curDir + "\Releases"
New-Item -ItemType Directory -Force -Path $releasesDir
Move-Item $nupkgPath -destination $releasesDir