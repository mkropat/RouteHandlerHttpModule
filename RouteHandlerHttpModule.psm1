#requires –runasadministrator

$ErrorActionPreference = 'Stop'
$VerbosePreference = 'Continue'

$moduleAssembly = Join-Path $PSScriptRoot 'RouteHandlerHttpModule.dll'

function Install-RouteHandlerModule {
    $name = Get-AssemblyName $moduleAssembly

    Install-Assembly $moduleAssembly

    Install-IisConfigReference $name
    Install-UserIisExpressConfigReference $name
}

Export-ModuleMember 'Install-RouteHandlerModule'

function Uninstall-RouteHandlerModule {
    Uninstall-UserIisExpressConfigReference

    Uninstall-Assembly $moduleAssembly
 }

Export-ModuleMember 'Uninstall-RouteHandlerModule'

function Get-AssemblyName([string] $Path) {
    $Path = Resolve-Path $Path
    [Reflection.AssemblyName]::GetAssemblyName($Path).FullName
}

function Install-Assembly([string] $Path) {
    # Based on code by David Brabant:
    # http://stackoverflow.com/a/19971942/27581

    $Path = Resolve-Path $Path

    Add-Type -AssemblyName System.EnterpriseServices

    [EnterpriseServices.Internal.Publish] $publish = New-Object EnterpriseServices.Internal.Publish

    Write-Verbose "Adding to GAC: $Path"
    $publish.GacInstall($Path)
}

function Uninstall-Assembly([string] $Path) {
    $Path = Resolve-Path $Path

    Add-Type -AssemblyName System.EnterpriseServices

    [EnterpriseServices.Internal.Publish] $publish = New-Object EnterpriseServices.Internal.Publish
    $publish.GacRemove($Path)
}

function Install-IisConfigReference([string]$AssemblyName) {
    $path = Join-Path ([Environment]::SystemDirectory) 'inetsrv\config\applicationHost.config'
    if (-not (Test-Path $path)) {
        return
    }

    [xml]$cfg = Get-XmlContent $path

    [xml.XmlElement]$modules = $cfg.configuration.'system.webServer'.modules
    if (-not $modules) {
        return
    }
     
    Add-ModuleReference $cfg $modules $AssemblyName

    $cfg.Save($path)
}

function Install-UserIisExpressConfigReference([string]$AssemblyName) {
    $path = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'IISExpress\config\applicationhost.config'
    if (-not (Test-Path $path)) {
        return
    }

    [xml]$cfg = Get-XmlContent $path

    [xml.XmlElement]$location = $cfg.configuration.location | where { $_.path -eq '' }
    if (-not $location) {
        return
    }

    [xml.XmlElement]$modules = $location.'system.webServer'.modules
    if (-not $modules) {
        return
    }
     
    Add-ModuleReference $cfg $modules $AssemblyName

    $cfg.Save($path)
}

function Uninstall-UserIisExpressConfigReference {
    $path = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'IISExpress\config\applicationhost.config'
    [xml]$cfg = Get-XmlContent $path
    if (-not (Test-Path $path)) {
        return
    }

    [xml.XmlElement]$location = $cfg.configuration.location | where { $_.path -eq '' }
    if (-not $location) {
        return
    }

    [xml.XmlElement]$modules = $location.'system.webServer'.modules
    if (-not $modules) {
        return
    }

    $ref = $modules.add | where { $_.name -eq 'RouteHandlerModule' }
    if ($ref) {
        $modules.RemoveChild($ref) | Out-Null
        $cfg.Save($path)
    }
}

function Add-ModuleReference([xml]$Document, [xml.XmlElement]$Modules, [string]$AssemblyName) {
    $add = $Modules.add | where { $_.name -eq 'RouteHandlerModule' }
    if (-not $add) {
        $add = $Document.CreateElement('add')
        $Modules.AppendChild($add) | Out-Null
    }

    $add.SetAttribute('name', 'RouteHandlerModule')
    $add.SetAttribute('type', "RouteHandlerHttpModule.RouteHandlerModule, $AssemblyName")
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