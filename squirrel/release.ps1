$projDirName = "Logazmic"
$projName = "Logazmic"

$curDir = (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$slnDir = $curDir + "\..\src\"
$projDir = $slnDir + $projDirName+ "\"

$nuspecPath = ($curDir + "\" + $projName + ".nuspec")
$version = GetVersion $nuspecPath
$nupkgPath = ($curDir + "\" + $projName + "." + $version + ".nupkg")

Write-Host "------------------Clean bin obj directories------------------"
ClearBinObj $projDir

Write-Host "------------------Build------------------"
Build ($projDir + $projName + ".csproj")

Write-Host "------------------Create nupkg------------------"
& ($curDir + "\bin\nuget.exe") pack $nuspecPath

Write-Host "------------------Release------------------"
& ($curDir + "\bin\squirrel.exe") --releasify $nupkgPath

Write-Host "------------------Completed------------------"

function ClearBinObj($dirPath)
{
    if (Test-Path ($dirPath + "bin"))
    {
        Remove-Item ($dirPath + "bin") -Force -Recurse
    }

    if (Test-Path ($dirPath + "obj"))
    {
        Remove-Item ($dirPath + "obj") -Force -Recurse
    }
}

function Build($projPath)
{    
    $msBuild = GetMSBuildPath
    $cpuCount = '/maxcpucount:4'
    $configuration = '/p:Configuration=Release'
    $platform = '/p:Platform="AnyCPU"'
    $outputPath = '/p:OutputPath="bin\Release"'
    $restorePackages = '/p:RestorePackages=true'
    $verbosity = '/v:q'

    & $msBuild $projPath $cpuCount $configuration $platform $outputPath $restorePackages $verbosity
}

function GetMSBuildPath()
{
    $msBuild = $ENV:ProgramFiles + "\MSBuild\14.0\bin\MSBuild.exe"
    if (-not (Test-Path ($msBuild)))
    {
        $msBuild = $ENV:ProgramFiles + " (x86)\MSBuild\14.0\bin\MSBuild.exe"
        if (-not (Test-Path ($msBuild)))
        {
            throw [System.IO.FileNotFoundException] "MSBuild.exe not found."
        }
    }

    return $msBuild
}

function GetVersion($nuspecPath)
{
    [xml]$nuspec = Get-Content $nuspecPath
    return $nuspec.package.metadata.version
}
