$projDirName = "Logazmic"
$projName = "Logazmic"

$curDir = (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$slnDir = $curDir + "\..\src\"
$projDir = $slnDir + $projDirName+ "\"

$version = (Get-Date -format yyMM.d) + "." + $env:appveyor_build_number;

$nuspecPath = ($curDir + "\" + $projName + ".nuspec")
$nupkgPath = ($curDir + "\" + $projName + "." + $version + ".nupkg")

Set-Location $curDir

Write-Host "------------------Setting nuspec version($vesrsion)------------------"
[xml]$nuspec = Get-Content $nuspecPath
$nuspec.package.metadata.version = $version
$nuspec.Save($nuspecPath)

Write-Host "------------------Create nupkg------------------"
& ($curDir + "\bin\nuget.exe") pack $nuspecPath

Write-Host "------------------Release------------------"
Write-Host $nupkgPath
& ($curDir + "\bin\squirrel.exe") --releasify $nupkgPath --no-msi | Out-Null
Write-Host "------------------Completed------------------"


