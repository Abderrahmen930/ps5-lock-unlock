param(
    [ValidateSet('Payload', 'Folder')]
    [string]$Output = 'Payload'
)

$ErrorActionPreference = 'Stop'

if (-not $env:SHARPPROSPERO_ROOT) {
    throw 'Set SHARPPROSPERO_ROOT to the SharpProspero SDK repository before building.'
}

$project = Join-Path $PSScriptRoot 'src/SharpProsperoCusaLocker/SharpProsperoCusaLocker.csproj'
$configuration = 'Release'

if ($Output -eq 'Folder') {
    dotnet publish $project -c $configuration -p:SharpProsperoOutput=Folder
} else {
    dotnet publish $project -c $configuration -p:SharpProsperoOutput=Payload
}
