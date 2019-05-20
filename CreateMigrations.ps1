
$migrations = dotnet ef migrations list -p ./FeatureServices.Storage/FeatureServices.Storage.csproj -c FeatureServicesContext
$previous = '0'
echo "Creating migrations for FeatureServices / FeatureServicesContext"
foreach ($migration in $migrations)
{
    echo "Creating migration : $migration";
    dotnet ef migrations script $previous $migration -p ./FeatureServices.Storage/FeatureServices.Storage.csproj -c FeatureServicesContext -i -o ".\migration-scripts\$migration.sql"
    $previous = $migration
}