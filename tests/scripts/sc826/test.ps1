$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "C:\inetpub\wwwroot\rocksTest826\Website" -RocksHost "http://rocksTest826.local" -SitecoreVersion "8.2" -DataFolder "..\data"