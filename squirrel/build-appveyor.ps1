# for debugging 
# $env:appveyor_build_number = "1"

$projDirName = "Logazmic"
$projName = "Logazmic"

$curDir = (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$slnDir = $curDir + "\..\src\"
$projDir = $slnDir + $projDirName + "\"
$setupIconPath = $curDir + "\app.ico"  

$version = (Get-Date -format yyMM.d) + "." + $env:appveyor_build_number;

$nuspecPath = ($curDir + "\" + $projName + ".nuspec")
$nupkgPath = ($curDir + "\" + $projName + "." + $version + ".nupkg")

Set-Location $curDir

Write-Host "------------------Setting nuspec version($version)------------------"
[xml]$nuspec = Get-Content $nuspecPath
$nuspec.package.metadata.version = $version
$nuspec.Save($nuspecPath)

Write-Host "------------------Create nupkg------------------"
& ($curDir + "\bin\nuget.exe") pack $nuspecPath

Write-Host "------------------Release------------------"
Write-Host $nupkgPath
& ($curDir + "\bin\squirrel.exe") --releasify $nupkgPath --no-msi --framework-version=net472 --setupIcon $setupIconPath | Out-Null
Write-Host "------------------Completed------------------"


