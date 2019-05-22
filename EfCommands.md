# feature-toggle
Service for feature toggles and user specific app settings


~~~~
dotnet ef migrations add SqlServerInitialCreate --context FeatureServicesContext

remove last migration:
dotnet ef migrations remove --context FeatureServicesContext

Update the database:
dotnet ef database update

~~~~

## Version History
__20-05-2019__
> - Created model