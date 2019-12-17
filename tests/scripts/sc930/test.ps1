$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "C:\inetpub\wwwroot\rocksTest930.local" -RocksHost "https://rockstest930.local" -SitecoreVersion "9.3"