IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    CREATE TABLE [Events] (
        [EventId] int NOT NULL IDENTITY,
        [EventName] nvarchar(max) NOT NULL,
        [EventDate] datetime2 NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([EventId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    CREATE TABLE [Venues] (
        [VenueId] int NOT NULL IDENTITY,
        [VenueName] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [Capacity] int NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Venues] PRIMARY KEY ([VenueId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [BookingId] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [VenueId] int NOT NULL,
        [BookingDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
        CONSTRAINT [FK_Bookings_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Bookings_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([VenueId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_EventId] ON [Bookings] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bookings_VenueId_BookingDate] ON [Bookings] ([VenueId], [BookingDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507154501_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260507154501_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

