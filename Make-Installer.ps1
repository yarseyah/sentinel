function Get-MsBuildCommand() {

    $findCommand = Get-Command msbuild -ErrorAction SilentlyContinue
    if ( $findCommand -ne $null ) {
        return "msbuild"
    }

    # Since VS2017, there is a bundled command called vswhere
    # - vswhere is included with the installer as of Visual Studio 2017 version 15.2 and later, 
    #   and can be found at the following location: 
    #   %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    if ( (Get-Command $vsWhere -ErrorAction SilentlyContinue) -eq $null) {
        Write-Error "Unable to find 'vswhere'"
    }
    else {
        $vspath = & $vsWhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($vspath) {
            $msbuild = join-path $vspath 'MSBuild\15.0\Bin\MSBuild.exe'

            if ( Test-Path $msbuild ) {
                return $msbuild
            } else {
                Write-Error "Should be located at $msbuild but Test-Path failed"
            }
        }
    }

    return $null;
}

function Get-NugetCommand() {
    $findCommand = Get-Command nuget -ErrorAction SilentlyContinue
    if ( $findCommand -ne $null ) {
        return "nuget"
    }

    $installedNuget = ".\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe"
    if ( Test-Path $installedNuget ) {
        return $installedNuget
    } else {
        return $null;
    }
}

function Get-SquirrelCommand() {
    $findCommand = Get-Command squirrel.exe -ErrorAction SilentlyContinue
    if ( $findCommand -ne $null ) {
        return "squirrel.exe"
    }

    $squirrel = ".\packages\squirrel.windows.1.7.7\tools\Squirrel.exe"
    if ( Test-Path $squirrel ) {
        return $squirrel
    } else {
        return $null;
    }
}

$msbuild = Get-MsBuildCommand
if ( $msbuild -eq $null ) {
    Write-Error "Unable to locate MSBuild"
    Exit
}

$nuget = Get-NugetCommand
if ( $nuget -eq $null ) {
    Write-Error "Unable to locate Nuget"
    Exit
}

$squirrel = Get-SquirrelCommand
if ( $squirrel -eq $null ) {
    Write-Error "Unable to locate Squirrel"
    Exit
}

Write-Output "MSBuild = $msbuild"
Write-Output "Nuget = $nuget"
Write-Output "Squirrel = $squirrel"

$solutionFile = "Sentinel.sln"
$nuspecFile = "Sentinel.nuspec"

$buildConfiguration = "Release"

& $msbuild /t:Clean /p:Configuration=$buildConfiguration $solutionFile
& $msbuild /t:Build /p:Configuration=$buildConfiguration $solutionFile

if ($LASTEXITCODE -eq 0) {
    $basePath = "Sentinel\Bin\$buildConfiguration"
    $application = "$basePath\Sentinel.exe"
    $version = (get-item $application).VersionInfo.ProductVersion
    Write-Output "Packaging up $application [$version]"

    & $nuget pack $nuspecFile -Version $version -BasePath $basePath

    $package = "Sentinel.$version.nupkg"
    if ( Test-Path $package ) {
        Write-Output "Packed into $package"
        $releasesLocation = "Releases"

        # Squirrel doesn't wait, so cheat by pipelining it with | Write-Output
        # See https://github.com/Squirrel/Squirrel.Windows/issues/489
        & $squirrel --releasify $package --no-msi --releaseDir=$releasesLocation | Write-Output

        if ( $LASTEXITCODE -eq 0) {
            # Give the installer a sensible name
            Rename-Item (Join-Path $releasesLocation "setup.exe") "Sentinel-Setup-$version.exe"
        }
    } else {
        Write-Error "Problem creating package"
    }
} else {
    Write-Error "Failed to build!"
}