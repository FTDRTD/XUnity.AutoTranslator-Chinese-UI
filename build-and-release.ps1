#Requires -Version 5.1
<#
.SYNOPSIS
    Automates the build, packaging, and GitHub release process for the XUnity.AutoTranslator project.
.DESCRIPTION
    This script performs the following actions:
    1. Cleans up previous build artifacts.
    2. Builds all 7 release configurations defined in the matrix.
    3. Correctly packages BepInEx versions with their specific directory structure.
    4. Creates ZIP archives for each build.
    5. Creates a new GitHub Release and uploads all generated ZIP files.
.PARAMETER Version
    The release version number (e.g., "5.5.0"). This parameter is required.
.EXAMPLE
    .\build-and-release.ps1 -Version 5.5.0
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true, HelpMessage = "The release version (e.g., 5.5.0)")]
    [string]$Version
)

# --- Script Configuration ---

# This matrix defines all the different versions to be built.
# It has been reformatted to be more readable and prevent copy-paste errors.
# FIX: Keys with hyphens like 'output-name' must be quoted.
$buildMatrix = @(
    @{
        project     = "XUnity.AutoTranslator.Plugin.BepInEx"
        target      = "net40"
        'output-name' = "XUnity.AutoTranslator-BepInEx"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.BepInEx-IL2CPP"
        target      = "net6.0"
        'output-name' = "XUnity.AutoTranslator-BepInEx-IL2CPP"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.Core"
        target      = "net35"
        'output-name' = "XUnity.AutoTranslator-Developer"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.Core"
        target      = "net6.0"
        'output-name' = "XUnity.AutoTranslator-Developer-IL2CPP"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.IPA"
        target      = "net35"
        'output-name' = "XUnity.AutoTranslator-IPA"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.MelonMod"
        target      = "net35"
        'output-name' = "XUnity.AutoTranslator-MelonMod"
        config      = "Release"
    },
    @{
        project     = "XUnity.AutoTranslator.Plugin.UnityInjector"
        target      = "net35"
        'output-name' = "XUnity.AutoTranslator-UnityInjector"
        config      = "Release"
    }
)

$SolutionFile = "XUnity.AutoTranslator.sln"
$DistDirectory = "dist"

# --- Script Execution ---

# Function to print a formatted header
function Write-SectionHeader {
    param ([string]$Title)
    Write-Host ""
    Write-Host "========================================================================" -ForegroundColor Green
    Write-Host "  $Title" -ForegroundColor Green
    Write-Host "========================================================================" -ForegroundColor Green
    Write-Host ""
}

try {
    # 0. Check for GitHub CLI
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI ('gh') not found. Please install it from https://cli.github.com/ and authenticate using 'gh auth login'."
    }

    # 1. Cleanup
    Write-SectionHeader "1. Cleaning previous build artifacts..."
    if (Test-Path $DistDirectory) {
        Remove-Item -Path $DistDirectory -Recurse -Force
        Write-Host "Removed existing '$DistDirectory' directory."
    }
    New-Item -Path $DistDirectory -ItemType Directory | Out-Null

    # 2. Restore Dependencies
    Write-SectionHeader "2. Restoring NuGet packages for the solution..."
    dotnet restore $SolutionFile
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

    # 3. Build All Projects
    Write-SectionHeader "3. Building all project configurations..."
    foreach ($build in $buildMatrix) {
        $projectName = $build.project
        $targetFramework = $build.target
        $config = $build.config
        Write-Host "Building $($build.'output-name') ($targetFramework)..."
        dotnet build "src\$projectName\$projectName.csproj" --configuration $config --framework $targetFramework --no-restore -p:Version=$Version
        if ($LASTEXITCODE -ne 0) { throw "Build failed for $projectName ($targetFramework)." }
    }

    # 4. Package All Builds
    Write-SectionHeader "4. Packaging all builds..."
    $allZipFiles = [System.Collections.Generic.List[string]]::new()
    foreach ($build in $buildMatrix) {
        $outputName = $build.'output-name'
        $projectName = $build.project
        $targetFramework = $build.target
        $config = $build.config
        $outputDir = Join-Path $DistDirectory $outputName
        
        Write-Host "Packaging $outputName..."
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

        # Copy main build outputs
        Copy-Item "src\$projectName\bin\$config\$targetFramework\*.dll" $outputDir -ErrorAction SilentlyContinue
        Copy-Item "src\$projectName\bin\$config\$targetFramework\*.pdb" $outputDir -ErrorAction SilentlyContinue
        Copy-Item "src\$projectName\bin\$config\$targetFramework\*.xml" $outputDir -ErrorAction SilentlyContinue

        # Special packaging for BepInEx versions
        if ($outputName -match "BepInEx") {
            Write-Host "  Applying BepInEx packaging rules for $outputName..."
            
            $bepInExDir = Join-Path $outputDir "BepInEx"
            $null = New-Item -ItemType Directory -Path (Join-Path $bepInExDir "core") -Force
            $null = New-Item -ItemType Directory -Path (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator") -Force
            $null = New-Item -ItemType Directory -Path (Join-Path $bepInExDir "plugins\XUnity.ResourceRedirector") -Force

            Copy-Item "src\XUnity.Common\bin\Release\$targetFramework\XUnity.Common.dll" (Join-Path $bepInExDir "core\")
            Copy-Item "src\XUnity.ResourceRedirector\bin\Release\$targetFramework\XUnity.ResourceRedirector.dll" (Join-Path $bepInExDir "plugins\XUnity.ResourceRedirector\")
            Copy-Item "src\XUnity.AutoTranslator.Plugin.Core\bin\Release\$targetFramework\XUnity.AutoTranslator.Plugin.Core.dll" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\")
            Copy-Item "src\XUnity.AutoTranslator.Plugin.ExtProtocol\bin\Release\net35\XUnity.AutoTranslator.Plugin.ExtProtocol.dll" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\")
            Copy-Item "src\$projectName\bin\$config\$targetFramework\$projectName.dll" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\")
            Copy-Item "src\$projectName\bin\$config\$targetFramework\$projectName.pdb" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\") -ErrorAction SilentlyContinue
            
            Copy-Item "libs\BepInEx 5.0\BepInEx.dll" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\")
            Copy-Item "libs\ExIni.dll" (Join-Path $bepInExDir "plugins\XUnity.AutoTranslator\")
        }

        # Create ZIP archive
        $zipName = "$outputName-$Version.zip"
        $zipPath = Join-Path $DistDirectory $zipName
        
        Compress-Archive -Path "$outputDir\*" -DestinationPath $zipPath -Force
        
        Write-Host "  Created ZIP package: $zipPath"
        $allZipFiles.Add($zipPath)
    }

    # 5. Create GitHub Release
    Write-SectionHeader "5. Creating GitHub Release v$Version..."
    
    if ($allZipFiles.Count -eq 0) {
        throw "No ZIP files found to upload."
    }

    # Use GitHub CLI to create the release and upload files
    gh release create "v$Version" --generate-notes --title "Release v$Version" $allZipFiles
    
    if ($LASTEXITCODE -ne 0) { throw "GitHub release creation failed." }

    Write-SectionHeader "SUCCESS! Release v$Version has been created successfully."

}
catch {
    Write-Host ""
    Write-Host "AN ERROR OCCURRED:" -ForegroundColor Red
    Write-Host $_.ToString() -ForegroundColor Red
    exit 1
}