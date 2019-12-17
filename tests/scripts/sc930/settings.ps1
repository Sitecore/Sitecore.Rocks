# The SIF config for our test instance
$InstallConfig = "rocks-sc930.json"
# The Prefix that will be used on SOLR, Website and Database instances.
$Prefix = "rocksTest930"
# The Password for the Sitecore Admin User. This will be regenerated if left on the default.
$SitecoreAdminPassword = "b"
# The root folder with the license file and WDP files.
$SCInstallRoot = "C:\Sitecore\Sitecore 9.3.0 rev. 003498 (WDP XM1 packages)"
# The URL of the Solr Server
$SolrUrl = "https://solr:8811/solr"
# The Folder that Solr has been installed in.
$SolrRoot = "C:\\solr\\solr-8.1.1"
# The Name of the Solr Service.
$SolrService = "Solr-8.1.1"
# The DNS name or IP of the SQL Instance.
$SqlServer = ".\sqlexpress2017"
# A SQL user with sysadmin privileges.
$SqlAdminUser = "sa"
# The password for $SQLAdminUser.
$SqlAdminPassword = "sitecore"

# User overrides before we calculate values
if (Test-Path $PSScriptRoot\settings.user.ps1) {
    . $PSScriptRoot\settings.user.ps1
}

# The execution path of the SIF config
$InstallConfigPath = "$SCInstallRoot\$InstallConfig"
# The Path to the license file
$LicenseFile = "$SCInstallRoot\license.xml"
# The name for the Sitecore Content Delivery server.
$SitecoreContentManagementSitename = "$Prefix.local"
# Identity Server site name
$IdentityServerSiteName = "$Prefix.identityserver.local"
# The path to the Sitecore Content Management Package to Deploy
$SiteCoreContentManagementPackage = (Get-ChildItem "$SCInstallRoot\Sitecore 9* rev. * (XM) (OnPrem)_cm.scwdp.zip").FullName
# The path to the Identity Server Package to Deploy.
$IdentityServerPackage = (Get-ChildItem "$SCInstallRoot\Sitecore.IdentityServer * rev. * (OnPrem)_identityserver.scwdp.zip").FullName
# The Identity Server password recovery URL, this should be the URL of the CM Instance
$PasswordRecoveryUrl = "http://$SitecoreContentManagementSitename"
# The URL of the Identity Authority
$SitecoreIdentityAuthority = "https://$IdentityServerSiteName"
# The random string key used for establishing connection with IdentityService. This will be regenerated if left on the default.
$ClientSecret = "SIF-Default"
# Pipe-separated list of instances (URIs) that are allowed to login via Sitecore Identity.
$allowedCorsOrigins = "https://$SitecoreContentManagementSitename"

# Install XM1 via combined partials file.
$singleDeveloperParams = @{
    Path = $InstallConfigPath
    SqlServer = $SqlServer
    SqlAdminUser = $SqlAdminUser
    SqlAdminPassword = $SqlAdminPassword
    SitecoreAdminPassword = $SitecoreAdminPassword
    SolrUrl = $SolrUrl
    SolrRoot = $SolrRoot
    SolrService = $SolrService
    Prefix = $Prefix
    IdentityServerCertificateName = $IdentityServerSiteName
    IdentityServerSiteName = $IdentityServerSiteName
    LicenseFile = $LicenseFile
    SiteCoreContentManagementPackage = $SiteCoreContentManagementPackage
    IdentityServerPackage = $IdentityServerPackage
    SitecoreContentManagementSitename = $SitecoreContentManagementSitename
    PasswordRecoveryUrl = $PasswordRecoveryUrl
    SitecoreIdentityAuthority = $SitecoreIdentityAuthority
    ClientSecret = $ClientSecret
    AllowedCorsOrigins = $AllowedCorsOrigins
}