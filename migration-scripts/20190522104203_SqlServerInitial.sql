IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE TABLE [TenantConfiguration] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NULL,
        [Reference] uniqueidentifier NOT NULL,
        [Tenant] int NOT NULL,
        [TimeStamp] rowversion NULL,
        [Created] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsReadOnly] bit NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_TenantConfiguration] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE TABLE [FeatureValue] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NULL,
        [Reference] uniqueidentifier NOT NULL,
        [Tenant] int NOT NULL,
        [TimeStamp] rowversion NULL,
        [Created] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsReadOnly] bit NOT NULL,
        [ApplicationName] nvarchar(450) NULL,
        [Value] nvarchar(max) NULL,
        [InternalType] nvarchar(max) NULL,
        [TenantConfigurationId] int NOT NULL,
        CONSTRAINT [PK_FeatureValue] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FeatureValue_TenantConfiguration_TenantConfigurationId] FOREIGN KEY ([TenantConfigurationId]) REFERENCES [TenantConfiguration] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE INDEX [IX_FeatureValue_ApplicationName] ON [FeatureValue] ([ApplicationName]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE INDEX [IX_FeatureValue_Name] ON [FeatureValue] ([Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE INDEX [IX_FeatureValue_TenantConfigurationId] ON [FeatureValue] ([TenantConfigurationId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    CREATE INDEX [IX_TenantConfiguration_Name] ON [TenantConfiguration] ([Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190522104203_SqlServerInitial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190522104203_SqlServerInitial', N'2.2.4-servicing-10062');
END;

GO

