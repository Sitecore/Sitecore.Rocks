. $PSScriptRoot\settings.ps1

# Copy our config into the install root
Copy-Item "$PSScriptRoot\$InstallConfig" $InstallConfigPath

Push-Location $SCInstallRoot
Install-SitecoreConfiguration @singleDeveloperParams *>&1 | Tee-Object log.install.txt
Pop-Location