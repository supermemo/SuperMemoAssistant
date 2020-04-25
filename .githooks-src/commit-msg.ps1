try {
    $prefixes = @(
        "- Added",
        "- Removed",
        "- Updated",
        "- Fixed",
        "- Misc"
    )

    # $args[0] should contain the path to a file with the user's commit message
    if (!(Test-Path $args[0])) {
        Write-Host "An error occured while verifying commit message: the commit message file '$args[0]' doesn't exist."
        exit 1
    }

    # Parse the file's commit message
    $errors = New-Object Collections.Generic.List[string]
    [int] $i = 1

    foreach ($line in [string[]] (Get-Content $args[0])) {
        $line = $line.Trim();

        if ($prefixes.Where({ $line.StartsWith($_) }).Count -eq 0) {
            $errors.Add("[line $i] Missing prefix in '$line'")
        }

        $i++
    }

    # If any error, display error message & fail
    if ($errors.Count -gt 0)
    {
        $red=""
        $nocolor=""

        $errMsg = @"
Errors:
$($errors -Join "`n")

Your commit lines should start with one of those prefixes:
'$($prefixes -Join "'; '")'

/!\ ERROR: Your commit message contained errors. Read above for more details.
"@
        Write-Host -ForegroundColor Red $errMsg

        exit 1
    }

    exit 0
}
catch {
    Write-Host "An error occured while verifying commit message: $_"
    exit -1
}