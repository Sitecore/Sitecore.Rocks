. $PSScriptRoot\settings.ps1

# Copy our config into the install root
Copy-Item "$PSScriptRoot\$InstallConfig" $InstallConfigPath

Push-Location $SCInstallRoot
Uninstall-SitecoreConfiguration @singleDeveloperParams *>&1 | Tee-Object log.uninstall.txt
Pop-Location