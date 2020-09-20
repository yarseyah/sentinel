<#
.SYNOPSIS
    New Installer generator
.DESCRIPTION
    Makes a new installer for the current code
.EXAMPLE
    New-Installer
.EXAMPLE
    New-Installer --NoBuild --Version 1.2.3
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
    [string] $Version = [string]::Empty
)

function Get-MsBuildCommand() {

    Write-Verbose "Looking for MSBuild"

    $findCommand = Get-Command msbuild -ErrorAction SilentlyContinue
    if ( $null -eq $findCommand ) {
        # returning 'msbuild' due to Get-Command finding something
        return "msbuild"
    }

    # Since VS2017, there is a bundled command called vswhere
    # - vswhere is included with the installer as of Visual Studio 2017 version 15.2 and later,
    #   and can be found at the following location:
    #   %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe

    Write-Verbose "Using vswhere to find msbuild"
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    if ( $null -eq (Get-Command $vsWhere -ErrorAction SilentlyContinue) ) {
        Write-Error "Unable to find 'vswhere'"
    }
    else {
        $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

        if ( Test-Path $msbuild ) {
            Write-Host "Get-MsBuildCommand() - found at $msbuild"
            return $msbuild
        }
        else {
            Write-Error "Should be located at $msbuild but Test-Path failed"
        }
    }

    # giving up and returning $null
    return $null;
}

function Get-NugetCommand() {
    Write-Verbose "Looking for nuget"
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
    Write-Verbose "Looking for Squirrel.exe"
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
    Write-Verbose "Looking for GitVersion"
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

if ( -not $NoBuild ) {
    $msbuild = Get-MsBuildCommand
    if ( $null -eq $msbuild ) {
        Write-Error "Unable to locate MSBuild"
        Exit 1
    }

    Write-Host "MSBuild = $msbuild"
}

$nuget = Get-NugetCommand
if ( $null -eq $nuget ) {
    Write-Error "Unable to locate Nuget"
    Exit 1
}

Write-Host "Nuget = $nuget"

$squirrel = Get-SquirrelCommand

if ( -not $NoBuild ) {
    # Restore any packages
    & $nuget restore

    # If clean building, Squirrel might not be found until a nuget restore is done
    if ( $null -eq $squirrel) {
        $squirrel = Get-SquirrelCommand
    }
}

if ( $null -eq $squirrel ) {
    Write-Error "Unable to locate Squirrel"
    Exit 1
}

Write-Host "Squirrel = $squirrel"

if ( [string]::IsNullOrWhiteSpace($Version) ) {
    $gitversion = Get-GitVerionsCommand
    if ( $null -eq $gitversion ) {
        Write-Error "Unable to locate GitVersion"
        Exit 1
    }

    Write-Host "GitVersion = $gitversion"
    & $gitversion /updateAssemblyInfo SharedAssemblyInfo.cs
} else {
    Write-Host "Using supplied version of $version"
}

# ===========================================================================
$productName = "Sentinel"
$applicationName = "Sentinel"
$solutionFile = "Sentinel.sln"
$nuspecFile = "Sentinel.nuspec"

$buildConfiguration = "Release"

if ( -not $NoBuild ) {
    # Build the code in release mode
    & $msbuild /t:Clean /p:Configuration=$buildConfiguration $solutionFile
    & $msbuild /t:Build /p:Configuration=$buildConfiguration $solutionFile

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build!"
        Exit 1
    }
}

$basePath = "$applicationName\Bin\$buildConfiguration"
Write-Host "Base Path = $basePath"

$application = "$basePath\$applicationName.exe"
Write-Host "Application = $application"

if ( [string]::IsNullOrWhiteSpace($version) ) {
    $version = (get-item $application).VersionInfo.FileVersion
    Write-Host (get-item $application).VersionInfo

    # See whether there is a symver truncation of version
    Write-Host "Determining version"
    $symver = $version -replace '(\d+)\.(\d+)\.(\d+)\.(\d+)', '$1.$2.$3'
    if ( $symver -ne $version ) {
        Write-Host "SymVer version being used: $($version) -> $($symver)"
        $version = $symver
    }
}

Write-Host "Creating Nuget package to contain installation content"
Write-Host "Packaging up $application [$version]"
& $nuget pack -NoPackageAnalysis $nuspecFile -Version $version -BasePath $basePath
$package = "$productName.$version.nupkg"

if ( Test-Path $package ) {
    Write-Host "Processing nuget package: $package"
    Write-Host "Packed into $package"
    $releasesLocation = "Releases"

    # Squirrel doesn't wait, so cheat by pipelining it with | Write-Host
    # See https://github.com/Squirrel/Squirrel.Windows/issues/489
    Write-Host "Involking Squirrel"
    & $squirrel --releasify $package --no-msi --releaseDir=$releasesLocation | Write-Host

    if ( $LASTEXITCODE -eq 0) {
        $srcFile = Join-Path $releasesLocation "setup.exe"
        $destFile = Join-Path $releasesLocation "$($productName)-Setup-$version.exe"
        Write-Host "Renaming output file: src: $srcFile dest: $destFile"

        # Give the installer a sensible name
        Move-Item $srcFile $destFile -Force
    } else {
        Write-Error "Failed to write package"
    }
}
else {
    Write-Host "File $($package) not found"
    Write-Error "Problem creating package"
}

Write-Host "`$LastExitCode = $LASTEXITCODE"
Exit -1
