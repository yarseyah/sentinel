<#
.SYNOPSIS
    New standalone build generator
.DESCRIPTION
    Makes a standalone installation inside a Zip file for the current code
.EXAMPLE
    New-Standalone
.EXAMPLE
    New-Standalone --NoCompile --NoVersionUpdate
.INPUTS
    None
.OUTPUTS
    Zips should be generated in the Releases folder
.NOTES

.COMPONENT
    NCCIS Checker
#>
[CmdletBinding()]
param (
    [Alias("b")]
    [Switch]
    $NoBuild = $false,

    [Alias("v")]
    [Switch]
    $NoVersionUpdate = $false
)

function Get-MsBuildCommand() {

    Write-Host "Get-MsBuildCommand()"

    $findCommand = Get-Command msbuild -ErrorAction SilentlyContinue
    if ( $null -eq $findCommand ) {
        Write-Host "Get-MsBuildCommand() - returning 'msbuild' due to Get-Command finding something"
        return "msbuild"
    }

    # Since VS2017, there is a bundled command called vswhere
    # - vswhere is included with the installer as of Visual Studio 2017 version 15.2 and later,
    #   and can be found at the following location:
    #   %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe

    Write-Host "Get-MsBuildCommand() - using vswhere to find"
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    Write-Host "Get-MsBuildCommand() - looking for vswhere at $vsWhere"

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

    Write-Host "Get-MsBuildCommand() - giving up and returning $null"
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

$buildConfiguration = "Release"

if ( -not $NoBuild ) {
    # Build the code in release mode
    & $msbuild /t:Clean /p:Configuration=$buildConfiguration $solutionFile
    & $msbuild /t:Build /p:Configuration=$buildConfiguration /property:DefineConstants="TRACE%3BSTANDALONE_BUILD" $solutionFile

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build!"
        Exit
    }
}

$basePath = "$applicationName\Bin\$buildConfiguration"
Write-Host "Base Path = $basePath"

$application = "$basePath\$applicationName.exe"
Write-Host "Application = $application"

$version = (get-item $application).VersionInfo.FileVersion
Write-Host "Version = $((get-item $application).VersionInfo)"

# See whether there is a symver truncation of version
Write-Host "Determining version"
$symver = $version -replace '(\d+)\.(\d+)\.(\d+)\.(\d+)', '$1.$2.$3'
if ( $symver -ne $version ) {
    Write-Host "SymVer version being used: $($version) -> $($symver)"
    $version = $symver
}

$releasesLocation = "Releases"
$zipName = "$productName.$version.zip"
$zipPath = Join-Path $releasesLocation $zipName 
Write-Host "Creating ZIP $zipPath from $basePath"

Get-ChildItem -Path "$basePath\*.dll", "$basePath\*.exe" | Compress-Archive -DestinationPath $zipPath -Force
