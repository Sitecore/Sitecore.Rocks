@("cm-deploy", "mssql-data", "serialization-data", "solr-data") | % {
    Get-ChildItem -Path $_ -Exclude ".gitkeep" -Recurse | Remove-Item -Force -Recurse -Verbose
}