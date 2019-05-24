# Deploy Unicorn and Serialization Config

& MSBuild ..\data\RocksTestData.sln /p:DeployOnBuild=true /p:PublishProfile=sc911


Import-Module ..\unicorn\Unicorn.psm1
$secret = ([xml](Get-Content -Raw ..\data\App_Config\Include\RocksTestData.config)).configuration.sitecore.unicorn.authenticationProvider.SharedSecret

# Copy serialized data to instance and sync
# Remove-Item -r -ForceC:\inetpub\wwwroot\rocksTest911.local\App_Data\unicorn\*
#Copy-Item -r -Force ..\data\serialized\* C:\inetpub\wwwroot\rocksTest911.local\App_Data\unicorn\
#Sync-Unicorn -ControlPanelUrl "https://rockstest911.local/unicorn.aspx" -SharedSecret $secret

# For reserializing test data
Sync-Unicorn -ControlPanelUrl "https://rockstest911.local/unicorn.aspx" -SharedSecret $secret -Verb 'Reserialize'
Remove-Item -r -Force ..\data\serialized\*
Copy-Item -r -Force C:\inetpub\wwwroot\rocksTest911.local\App_Data\unicorn\* ..\data\serialized