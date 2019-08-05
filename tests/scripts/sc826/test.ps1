$ErrorActionPreference = "Stop"
Import-Module ..\Invoke-Test.psm1

Invoke-Test -RocksLocation "C:\Sitecore\sc826\Website" -RocksHost "http://sc826.local" -SitecoreVersion "8.2" -DataFolder "..\data"