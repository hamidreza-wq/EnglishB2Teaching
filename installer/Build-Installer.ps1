param(
    [string]$Dotnet = "$env:LOCALAPPDATA\EnglishB2Academy\dotnet\dotnet.exe",
    [string]$WixDirectory = "$env:LOCALAPPDATA\EnglishB2Academy\tools\wix314"
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
[xml]$project = Get-Content (Join-Path $root 'src\EnglishB2Academy\EnglishB2Academy.csproj') -Raw
$version = [string]$project.Project.PropertyGroup.Version
if (!$version) { throw 'Application version is missing.' }
$publish = Join-Path $root 'artifacts\app'
$output = Join-Path $root 'artifacts\installer'
New-Item -ItemType Directory -Path $publish, $output -Force | Out-Null
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& $Dotnet publish (Join-Path $root 'src\EnglishB2Academy\EnglishB2Academy.csproj') `
    -c Release -r win-x64 --self-contained true -p:Platform=x64 -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Application publish failed.' }

foreach ($name in @('THIRD_PARTY_NOTICES.md','OPEN_SOURCE_LICENCES.md','PRIVACY_POLICY.md',
    'CONTENT_PROVENANCE_REGISTER.md','COPYRIGHT_AND_ATTRIBUTION.md','RESOURCE_PERMISSIONS_REPORT.md',
    'SOFTWARE_BILL_OF_MATERIALS.md','CONTENT_LICENSING_POLICY.md')) {
    Copy-Item -LiteralPath (Join-Path $root $name) -Destination $publish -Force
}
Copy-Item -LiteralPath (Join-Path $root 'docs\USER_GUIDE.md') -Destination $publish -Force
Copy-Item -LiteralPath (Join-Path $root 'docs\licences') -Destination $publish -Recurse -Force
$runtimeNotices = Join-Path (Split-Path $Dotnet -Parent) 'ThirdPartyNotices.txt'
if (!(Test-Path $runtimeNotices)) { throw 'Runtime third-party notices not found next to the SDK.' }
Copy-Item -LiteralPath $runtimeNotices -Destination (Join-Path $publish 'DOTNET-ThirdPartyNotices.txt') -Force
Copy-Item -LiteralPath (Join-Path (Split-Path $Dotnet -Parent) 'LICENSE.txt') `
    -Destination (Join-Path $publish 'DOTNET-LICENSE.txt') -Force

$xml = [System.Xml.XmlDocument]::new()
$ns = 'http://schemas.microsoft.com/wix/2006/wi'
$wix = $xml.CreateElement('Wix', $ns); $null = $xml.AppendChild($wix)
$fragment = $xml.CreateElement('Fragment', $ns); $null = $wix.AppendChild($fragment)
$directoryRef = $xml.CreateElement('DirectoryRef', $ns); $directoryRef.SetAttribute('Id','INSTALLFOLDER')
$null = $fragment.AppendChild($directoryRef)
$group = $xml.CreateElement('ComponentGroup', $ns); $group.SetAttribute('Id','PublishedFiles')
$null = $fragment.AppendChild($group)
$directories = @{ '' = $directoryRef }
$counter = 0
foreach ($file in Get-ChildItem -LiteralPath $publish -File -Recurse | Sort-Object FullName) {
    if ($file.Extension -eq '.pdb') { continue }
    $relative = [System.IO.Path]::GetRelativePath($publish, $file.FullName)
    $parent = [System.IO.Path]::GetDirectoryName($relative)
    if (!$directories.ContainsKey($parent)) {
        $segments = $parent.Split('\')
        $path = ''
        foreach ($segment in $segments) {
            $ancestor = $path
            $path = if ($path) { "$path\$segment" } else { $segment }
            if ($directories.ContainsKey($path)) { continue }
            $directory = $xml.CreateElement('Directory', $ns)
            $directory.SetAttribute('Id', "Dir$($directories.Count)")
            $directory.SetAttribute('Name', $segment)
            $null = $directories[$ancestor].AppendChild($directory)
            $directories[$path] = $directory
        }
    }
    $hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData(
        [System.Text.Encoding]::UTF8.GetBytes($relative.ToLowerInvariant()))).Substring(0,32)
    $id = "File$hash"
    $component = $xml.CreateElement('Component', $ns)
    $component.SetAttribute('Id', "Cmp$hash")
    $component.SetAttribute('Guid', [Guid]::ParseExact($hash,'N').ToString('D'))
    $component.SetAttribute('Win64', 'yes')
    $payload = $xml.CreateElement('File', $ns)
    $payload.SetAttribute('Id', $id); $payload.SetAttribute('Source', $file.FullName)
    $payload.SetAttribute('Name', $file.Name)
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName)
    if ($versionInfo.FileVersion -and !$versionInfo.Language) { $payload.SetAttribute('DefaultLanguage','0') }
    $null = $component.AppendChild($payload)
    $key = $xml.CreateElement('RegistryValue', $ns)
    $key.SetAttribute('Root', 'HKCU'); $key.SetAttribute('Key', 'Software\EnglishB2Academy\Installer\Files')
    $key.SetAttribute('Name', $hash); $key.SetAttribute('Type','integer')
    $key.SetAttribute('Value','1'); $key.SetAttribute('KeyPath','yes')
    $null = $component.AppendChild($key)
    $null = $directories[$parent].AppendChild($component)
    $reference = $xml.CreateElement('ComponentRef', $ns); $reference.SetAttribute('Id', "Cmp$hash")
    $null = $group.AppendChild($reference)
    $counter++
}
$cleanup = $xml.CreateElement('Component', $ns)
$cleanup.SetAttribute('Id','RemoveInstallDirectories'); $cleanup.SetAttribute('Guid','*'); $cleanup.SetAttribute('Win64','yes')
$key = $xml.CreateElement('RegistryValue', $ns)
$key.SetAttribute('Root','HKCU'); $key.SetAttribute('Key','Software\EnglishB2Academy\Installer')
$key.SetAttribute('Name','Directories'); $key.SetAttribute('Type','integer')
$key.SetAttribute('Value','1'); $key.SetAttribute('KeyPath','yes'); $null = $cleanup.AppendChild($key)
$removePrograms = $xml.CreateElement('RemoveFolder', $ns)
$removePrograms.SetAttribute('Id','RemoveEmptyProgramsFolder'); $removePrograms.SetAttribute('Directory','ProgramsFolder')
$removePrograms.SetAttribute('On','uninstall'); $null = $cleanup.AppendChild($removePrograms)
foreach ($entry in $directories.GetEnumerator()) {
    $remove = $xml.CreateElement('RemoveFolder', $ns)
    $directoryId = if ($entry.Key -eq '') { 'INSTALLFOLDER' } else { $entry.Value.GetAttribute('Id') }
    $remove.SetAttribute('Id',"Remove$directoryId"); $remove.SetAttribute('Directory',$directoryId)
    $remove.SetAttribute('On','uninstall'); $null = $cleanup.AppendChild($remove)
}
$null = $directoryRef.AppendChild($cleanup)
$reference = $xml.CreateElement('ComponentRef', $ns); $reference.SetAttribute('Id','RemoveInstallDirectories')
$null = $group.AppendChild($reference)
$generated = Join-Path $output 'PublishedFiles.wxs'
$xml.Save($generated)
& (Join-Path $WixDirectory 'candle.exe') -nologo -arch x64 "-dAppVersion=$version" -out "$output\" `
    (Join-Path $PSScriptRoot 'Product.wxs') $generated
if ($LASTEXITCODE -ne 0) { throw 'MSI compilation failed.' }
$msi = Join-Path $output "EnglishB2Academy-$version-x64.msi"
& (Join-Path $WixDirectory 'light.exe') -nologo -sice:ICE91 -out $msi `
    (Join-Path $output 'Product.wixobj') (Join-Path $output 'PublishedFiles.wixobj')
if ($LASTEXITCODE -ne 0) { throw 'MSI linking or validation failed.' }
Write-Host "Packaged $counter files: $msi"
Get-FileHash -LiteralPath $msi -Algorithm SHA256
