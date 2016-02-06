#Requires -RunAsAdministrator

$moduleAssembly = Join-Path $PSScriptRoot 'RouteHandlerHttpModule.dll'
$configs = @(
    (Join-Path ([Environment]::SystemDirectory) 'inetsrv\config\applicationHost.config'),
    (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'IISExpress\config\applicationhost.config')
)

function Install-RouteHandlerModule {
    $name = Get-AssemblyName $moduleAssembly

    Uninstall-Assembly $moduleAssembly
    Install-Assembly $moduleAssembly

    foreach ($c in $configs) {
        Install-IisConfigReference $name $c
    }
}

Export-ModuleMember 'Install-RouteHandlerModule'

function Uninstall-RouteHandlerModule {
    foreach ($c in $configs) {
        Uninstall-IisConfigReference $c
    }

    Uninstall-Assembly $moduleAssembly
}

Export-ModuleMember 'Uninstall-RouteHandlerModule'

function Get-AssemblyName([string]$Path) {
    $Path = Resolve-Path $Path
    [Reflection.AssemblyName]::GetAssemblyName($Path).FullName
}

function Install-Assembly([string]$Path) {
    # Based on code by David Brabant:
    # http://stackoverflow.com/a/19971942/27581

    $Path = Resolve-Path $Path

    Add-Type -AssemblyName System.EnterpriseServices

    [EnterpriseServices.Internal.Publish]$publish = New-Object EnterpriseServices.Internal.Publish

    Write-Verbose "Adding to GAC: $Path"
    $publish.GacInstall($Path)
}

function Uninstall-Assembly([string]$Path) {
    $Path = Resolve-Path $Path

    Add-Type -AssemblyName System.EnterpriseServices

    [EnterpriseServices.Internal.Publish]$publish = New-Object EnterpriseServices.Internal.Publish
    $publish.GacRemove($Path)
}

function Install-IisConfigReference([string]$AssemblyName, [string]$ConfigPath) {
    if (-not (Test-Path $ConfigPath)) {
        return
    }

    $cfg = Get-XmlContent $ConfigPath

    [xml.XmlElement]$location = $cfg.configuration.location | where { $_.path -eq '' }
    if ($location) {
        [xml.XmlElement]$modules = $location.'system.webServer'.modules
    }

    if (-not $modules) {
        [xml.XmlElement]$modules = $cfg.configuration.'system.webServer'.modules
    }

    if (-not $modules) {
        return
    }
     
    $add = $modules.add | where { $_.name -eq 'RouteHandlerModule' }
    if (-not $add) {
        $add = $cfg.CreateElement('add')
        $modules.AppendChild($add) | Out-Null
    }

    $add.SetAttribute('name', 'RouteHandlerModule')
    $add.SetAttribute('type', "RouteHandlerHttpModule.RouteHandlerModule, $AssemblyName")

    $cfg.Save($ConfigPath)
}

function Uninstall-IisConfigReference([string]$ConfigPath) {
    if (-not (Test-Path $ConfigPath)) {
        return
    }

    $cfg = Get-XmlContent $ConfigPath

    $location = $cfg.configuration.location | where { $_.path -eq '' }
    $modules = $location.'system.webServer'.modules
    $ref = $modules.add | where { $_.name -eq 'RouteHandlerModule' }

    if (-not $ref) {
        $modules = $cfg.configuration.'system.webServer'.modules
        $ref = $modules.add | where { $_.name -eq 'RouteHandlerModule' }
    }

    if ($ref) {
        $modules.RemoveChild($ref) | Out-Null
        $cfg.Save($ConfigPath)
    }
}

function Get-XmlContent {
    [OutputType([xml])]
    param(
        [parameter(Mandatory=$true)]
        [string]$Path
    )

    $xml = New-Object xml
    $xml.psbase.PreserveWhitespace = $true
    $xml.Load($Path)
    $xml
}
