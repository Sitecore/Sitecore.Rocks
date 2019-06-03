$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "c:\Inetpub\wwwroot\rocksTest911.local" -RocksHost "https://rocksTest911.local"