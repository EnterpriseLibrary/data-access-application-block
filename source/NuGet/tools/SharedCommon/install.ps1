param($installPath, $toolsPath, $package, $project)

Import-Module (Join-Path $toolsPath Utils.psm1)

$relativeInstallPath = Get-RelativePath ([System.Io.Path]::GetDirectoryName($dte.Solution.FullName)) $installPath

$folder = (Join-Path (Join-Path $relativeInstallPath "lib")  "NET45")

Add-ToolFolder $folder

