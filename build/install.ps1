$exePath = "$env:USERPROFILE\SSCERuntime-ENU.exe"
(New-Object Net.WebClient).DownloadFile('https://download.microsoft.com/download/E/C/1/EC1B2340-67A0-4B87-85F0-74D987A27160/SSCERuntime-ENU.exe', $exePath)

Write-Host "Installing..."
cmd /c start /wait $exePath /Q
Write-Host "SQL Server Compact installed" -foregroundcolor Green

$zipPath = "$env:TEMP\SSCERuntime-ENU.exe"
Write-Host "Doanloading..."
(New-Object Net.WebClient).DownloadFile('https://download.microsoft.com/download/E/C/1/EC1B2340-67A0-4B87-85F0-74D987A27160/SSCERuntime-ENU.exe', $zipPath )

$destPath = "$env:TEMP\SSCERuntime"
Write-Host "Unpacking..."
7z x $zipPath -o"$destPath" | Out-Null

Write-Host "Installing..."
# according to install.txt It is mandatory to install both the 32-bit and the 64-bit 
# version of SQL Server Compact MSI files on a 64-bit Computer
cmd /c start /wait $destPath\SSCERuntime_x86-ENU.msi /quiet /norestart
cmd /c start /wait $destPath\SSCERuntime_x64-ENU.msi /quiet /norestart

Write-Host "SQL Server Compact installed" -foregroundcolor Green
