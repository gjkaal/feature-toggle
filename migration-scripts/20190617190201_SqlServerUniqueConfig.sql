IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190617190201_SqlServerUniqueConfig')
BEGIN
    DROP INDEX [IX_TenantConfiguration_Name] ON [TenantConfiguration];
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190617190201_SqlServerUniqueConfig')
BEGIN
    CREATE UNIQUE INDEX [IX_TenantConfiguration_Name] ON [TenantConfiguration] ([Name]) WHERE [Name] IS NOT NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190617190201_SqlServerUniqueConfig')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190617190201_SqlServerUniqueConfig', N'2.2.4-servicing-10062');
END;

GO

