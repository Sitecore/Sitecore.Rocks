Push-Location $PSScriptRoot\..\..\..

try {
    Import-Module .\tests\scripts\unicorn\Unicorn.psm1
    $secret = ([xml](Get-Content -Raw .\tests\code\instance\App_Config\Include\RocksTestData.config)).configuration.sitecore.unicorn.authenticationProvider.SharedSecret

    # For reserializing test data
    Sync-Unicorn -ControlPanelUrl "https://rockstest911.local/unicorn.aspx" -SharedSecret $secret -Verb 'Reserialize'
    Remove-Item -r -Force .\tests\serialization\*
    Copy-Item -r -Force C:\inetpub\wwwroot\rocksTest911.local\App_Data\unicorn\* .\tests\serialization
} finally {
    Pop-Location
}
