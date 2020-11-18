$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "$PSScriptRoot\cm-deploy" -RocksHost "https://cm.rockstest100.localhost" -SitecoreVersion "9.3" -SerializationMount "$PSScriptRoot\serialization-data"