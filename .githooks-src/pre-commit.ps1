try {
    # https://github.com/Microsoft/vswhere/wiki/Find-MSBuild
    function Find-MSBuild( [string] $vswhere ) {
        return & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
    }

    # https://github.com/microsoft/vswhere/wiki/Find-VSTest
    function Find-VSTest( [string] $vswhere ) {
        $path = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Workload.ManagedDesktop Microsoft.VisualStudio.Workload.Web -requiresAny -property installationPath
        return Join-Path $path 'Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe'
    }
    
    # Recursively search for all .csproj files under the src folder
    function Find-Projects {
        return Get-ChildItem -Path src -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue -Force
    }

    # Run tests on the given projects
    function Run-Tests ( [string] $vstest, [string[]] $projects ) {
        #

        & $vstest $projectDlls "/InIsolation" "/Logger:trx"
    }

    # Get MSBuild
    # $vswhere = "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    # $vstest = Find-VSTest $vswhere
    # $msbuild = Find-MSBuild $vswhere

    # Find all the projects
    # $projects = Find-Projects

    # TODO: Build projects
    # 

    # Run tests
    # $testResults = Run-Tests $vstest $projects

    exit 0
}
catch {
    Write-Host "An error occured while verifying commit: $_"
    exit -1
}