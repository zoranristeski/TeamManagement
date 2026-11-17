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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Auth0Id] nvarchar(100) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [Role] int NOT NULL,
        [IsActive] bit NOT NULL,
        [ProfileImageUrl] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [EmergencyContactName] nvarchar(200) NULL,
        [EmergencyContactPhone] nvarchar(20) NULL,
        [MedicalConditions] nvarchar(1000) NULL,
        [JoinDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [CurrentBelt] int NOT NULL,
        [LastBeltPromotion] datetime2 NULL,
        [NextGradingEligible] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Students_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [Trainers] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Rank] int NOT NULL,
        [Specialization] nvarchar(200) NULL,
        [StartDate] datetime2 NOT NULL,
        [Biography] nvarchar(2000) NULL,
        [CanApproveStudents] bit NOT NULL,
        [CanPromoteBelts] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Trainers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Trainers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [BeltPromotions] (
        [Id] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [PromotedById] uniqueidentifier NOT NULL,
        [FromBelt] int NOT NULL,
        [ToBelt] int NOT NULL,
        [PromotionDate] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [Requirements] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BeltPromotions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BeltPromotions_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BeltPromotions_Trainers_PromotedById] FOREIGN KEY ([PromotedById]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [Events] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Type] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Location] nvarchar(300) NULL,
        [MaxParticipants] int NOT NULL,
        [Cost] decimal(10,2) NULL,
        [IsActive] bit NOT NULL,
        [RegistrationDeadline] datetime2 NOT NULL,
        [OrganizerId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Events_Trainers_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [TrainingSessions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [TrainerId] uniqueidentifier NOT NULL,
        [MaxParticipants] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TrainingSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TrainingSessions_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [EventRegistrations] (
        [Id] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [RegistrationDate] datetime2 NOT NULL,
        [IsConfirmed] bit NOT NULL,
        [IsAttended] bit NOT NULL,
        [Notes] nvarchar(500) NULL,
        [AmountPaid] decimal(10,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EventRegistrations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EventRegistrations_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EventRegistrations_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE TABLE [Attendances] (
        [Id] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [TrainingSessionId] uniqueidentifier NOT NULL,
        [AttendanceDate] datetime2 NOT NULL,
        [IsPresent] bit NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attendances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Attendances_TrainingSessions_TrainingSessionId] FOREIGN KEY ([TrainingSessionId]) REFERENCES [TrainingSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Attendances_StudentId_TrainingSessionId] ON [Attendances] ([StudentId], [TrainingSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Attendances_TrainingSessionId] ON [Attendances] ([TrainingSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BeltPromotions_PromotedById] ON [BeltPromotions] ([PromotedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BeltPromotions_StudentId] ON [BeltPromotions] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventRegistrations_EventId] ON [EventRegistrations] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EventRegistrations_StudentId_EventId] ON [EventRegistrations] ([StudentId], [EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_OrganizerId] ON [Events] ([OrganizerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Trainers_UserId] ON [Trainers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainingSessions_TrainerId] ON [TrainingSessions] ([TrainerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Auth0Id] ON [Users] ([Auth0Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117014023_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117014023_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

