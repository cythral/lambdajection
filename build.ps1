#!/usr/bin/env pwsh
dotnet tool restore
$version = dotnet nbgv get-version -v NuGetPackageVersion

if ($args.Length -eq 1) 
{
    $os=$args[1]
    $releaseNotes="$(sed 's/,/%2c/g' .github/releases/v${version}.md)"
    dotnet build -t:build,pack --no-restore -m  -bl:obj/logs/build-${os}.binlog -p:PackageReleaseNotes="$releaseNotes"
} 
else 
{
    dotnet build
}

dotnet build examples /p:LambdajectionVersion=$version
