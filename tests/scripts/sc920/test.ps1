$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "c:\Inetpub\wwwroot\rocksTest920.local" -RocksHost "https://rocksTest920.local" -SitecoreVersion "9.2"