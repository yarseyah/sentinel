<#
.SYNOPSIS
    New Installer generator
.DESCRIPTION
    Makes a new installer for the current code
.EXAMPLE
    New-Installer
.EXAMPLE
    New-Installer --NoCompile --Version 1.2.3
.INPUTS
    None
.OUTPUTS
    Installers should be generated in the Releases folder
.NOTES

.COMPONENT
    NCCIS Checker
.ROLE
    Installer generator
.FUNCTIONALITY
    Installer generator
#>
[CmdletBinding()]
param (
    [Alias("b")]
    [Switch]
    $NoBuild = $false,

    [Alias("v")]
    [Switch]
    [string] $Version = $null
)

function Get-MsBuildCommand() {

    Write-Output "Get-MsBuildCommand()"

    $findCommand = Get-Command msbuild -ErrorAction SilentlyContinue
    if ( $null -eq $findCommand ) {
        Write-Output "Get-MsBuildCommand() - returning 'msbuild' due to Get-Command finding something"
        return "msbuild"
    }

    # Since VS2017, there is a bundled command called vswhere
    # - vswhere is included with the installer as of Visual Studio 2017 version 15.2 and later,
    #   and can be found at the following location:
    #   %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe

    Write-Output "Get-MsBuildCommand() - using vswhere to find"
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    Write-Output "Get-MsBuildCommand() - looking for vswhere at $vsWhere"

    if ( $null -eq (Get-Command $vsWhere -ErrorAction SilentlyContinue) ) {
        Write-Error "Unable to find 'vswhere'"
    }
    else {
        $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

        if ( Test-Path $msbuild ) {
            Write-Output "Get-MsBuildCommand() - found at $msbuild"
            return $msbuild
        }
        else {
            Write-Error "Should be located at $msbuild but Test-Path failed"
        }
    }

    Write-Output "Get-MsBuildCommand() - giving up and returning $null"
    return $null;
}

function Get-NugetCommand() {
    $findCommand = Get-Command nuget -ErrorAction SilentlyContinue
    if ( $null -ne $findCommand ) {
        return "nuget"
    }

    $installedNuget = (Get-ChildItem -Path "~\.nuget\packages\" -Recurse -Include NuGet.exe -ErrorAction SilentlyContinue | Select-Object -First 1)
    if ( Test-Path $installedNuget -ErrorAction SilentlyContinue) {
        return $installedNuget
    }
    
    return "dotnet nuget";
}

function Get-SquirrelCommand() {
    $findCommand = Get-Command squirrel.exe -ErrorAction SilentlyContinue
    if ( $null -ne $findCommand ) {
        return "squirrel.exe"
    }

    $squirrel = (Get-ChildItem -Path "~\.nuget\packages\" -Recurse -Include squirrel.exe | Select-Object -First 1)
    if ( Test-Path $squirrel ) {
        return $squirrel
    }
    else {
        return $null;
    }
}

function Get-GitVerionsCommand() {
    $findCommand = Get-Command nuget -ErrorAction SilentlyContinue
    if ( $null -ne $findCommand ) {
        return "gitversion"
    }

    $installedGitVersion = (Get-ChildItem -Path "~\.nuget\packages\" -Recurse -Include GitVersion.exe | Select-Object -First 1)
    if ( Test-Path $installedGitVersion ) {
        return $installedGitVersion
    }
    else {
        return $null;
    }
}

$msbuild = Get-MsBuildCommand
if ( $null -eq $msbuild ) {
    Write-Error "Unable to locate MSBuild"
    Exit
}

$nuget = Get-NugetCommand
if ( $null -eq $nuget ) {
    Write-Error "Unable to locate Nuget"
    Exit
}

Write-Output "MSBuild = $msbuild"
Write-Output "Nuget = $nuget"

# Restore any packages
& $nuget restore

# Look for locations of things that may have required nuget restore to download
$squirrel = Get-SquirrelCommand
if ( $null -eq $squirrel ) {
    Write-Error "Unable to locate Squirrel"
    Exit
}

Write-Output "Squirrel = $squirrel"

if ( -not $NoVersionUpdate ) {
    $gitversion = Get-GitVerionsCommand
    if ( $null -eq $gitversion ) {
        Write-Error "Unable to locate GitVersion"
        Exit
    }

    Write-Output "GitVersion = $gitversion"
    & $gitversion /updateAssemblyInfo SharedAssemblyInfo.cs
}

# ===========================================================================
$productName = "Sentinel"
$applicationName = "Sentinel"
$solutionFile = "Sentinel.sln"
$nuspecFile = "Sentinel.nuspec"

$buildConfiguration = "Release"

if ( $null -ne $Version -and -not $NoBuild ) {
    # Build the code in release mode
    & $msbuild /t:Clean /p:Configuration=$buildConfiguration $solutionFile
    & $msbuild /t:Build /p:Configuration=$buildConfiguration $solutionFile

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build!"
        Exit
    }
}

$basePath = "$applicationName\Bin\$buildConfiguration"
Write-Output "Base Path = $basePath"

$application = "$basePath\$applicationName.exe"
Write-Output "Application = $application"

if ( $null -ne $version ) {
    $version = (get-item $application).VersionInfo.FileVersion
    Write-Output (get-item $application).VersionInfo
}

# See whether there is a symver truncation of version
Write-Output "Determining version"
$symver = $version -replace '(\d+)\.(\d+)\.(\d+)\.(\d+)', '$1.$2.$3'
if ( $symver -ne $version ) {
    Write-Output "SymVer version being used: $($version) -> $($symver)"
    $version = $symver
}

Write-Output "Creating Nuget package to contain installation content"
Write-Output "Packaging up $application [$version]"
& $nuget pack -NoPackageAnalysis $nuspecFile -Version $version -BasePath $basePath
$package = "$productName.$version.nupkg"

if ( Test-Path $package ) {
    Write-Output "Processing nuget package: $package"
    Write-Output "Packed into $package"
    $releasesLocation = "Releases"

    # Squirrel doesn't wait, so cheat by pipelining it with | Write-Output
    # See https://github.com/Squirrel/Squirrel.Windows/issues/489
    Write-Output "Involking Squirrel"
    & $squirrel --releasify $package --no-msi --releaseDir=$releasesLocation | Write-Output

    if ( $LASTEXITCODE -eq 0) {
        $srcFile = Join-Path $releasesLocation "setup.exe"
        $destFile = Join-Path $releasesLocation "$($productName)-Setup-$version.exe"
        Write-Output "Renaming output file: src: $srcFile dest: $destFile"

        # Give the installer a sensible name
        Move-Item $srcFile $destFile -Force
    } else {
        Write-Error "Failed to write package"
    }
}
else {
    Write-Output "File $($package) not found"
    Write-Error "Problem creating package"
}
