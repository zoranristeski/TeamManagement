/*
    Permanently deletes application data from the SportsTeamManagement database.
    EF migration history is preserved, so the schema remains available.

    Set @ExpectedDatabaseName to the exact development database name before running.
    Run this script only against a local/development database after taking a backup.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ExpectedDatabaseName sysname = N'REPLACE_WITH_DEV_DATABASE_NAME';

IF @ExpectedDatabaseName = N'REPLACE_WITH_DEV_DATABASE_NAME'
    THROW 50001, 'Set @ExpectedDatabaseName to the exact development database name before running this script.', 1;

IF DB_NAME() <> @ExpectedDatabaseName
    THROW 50002, 'The connected database does not match @ExpectedDatabaseName. No data was deleted.', 1;

DECLARE @Tables TABLE
(
    SchemaName sysname NOT NULL,
    TableName sysname NOT NULL,
    PRIMARY KEY (SchemaName, TableName)
);

INSERT INTO @Tables (SchemaName, TableName)
VALUES
    (N'dbo', N'Attendances'),
    (N'dbo', N'AuditLogs'),
    (N'dbo', N'BeltPromotions'),
    (N'dbo', N'Clubs'),
    (N'dbo', N'ClubMemberships'),
    (N'dbo', N'EnrollmentRequests'),
    (N'dbo', N'Events'),
    (N'dbo', N'EventRegistrations'),
    (N'dbo', N'ExternalLogins'),
    (N'dbo', N'Guardians'),
    (N'dbo', N'GuardianStudents'),
    (N'dbo', N'Memberships'),
    (N'dbo', N'MembershipPlans'),
    (N'dbo', N'ParentStudentRelationships'),
    (N'dbo', N'PaymentRecords'),
    (N'dbo', N'RefreshTokens'),
    (N'dbo', N'RoleAssignmentHistories'),
    (N'dbo', N'Students'),
    (N'dbo', N'Trainers'),
    (N'dbo', N'TrainingGroups'),
    (N'dbo', N'TrainingGroupStudents'),
    (N'dbo', N'TrainingGroupTrainers'),
    (N'dbo', N'TrainingSessions'),
    (N'dbo', N'Users');

IF EXISTS
(
    SELECT 1
    FROM @Tables
    WHERE OBJECT_ID(QUOTENAME(SchemaName) + N'.' + QUOTENAME(TableName), N'U') IS NULL
)
    THROW 50003, 'One or more expected application tables are missing. Check migrations before running this script.', 1;

DECLARE @SchemaName sysname;
DECLARE @TableName sysname;
DECLARE @Sql nvarchar(max);
DECLARE @DeletedRows int;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE disable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT SchemaName, TableName FROM @Tables;

    OPEN disable_cursor;
    FETCH NEXT FROM disable_cursor INTO @SchemaName, @TableName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
            + N' NOCHECK CONSTRAINT ALL;';
        EXEC sys.sp_executesql @Sql;
        FETCH NEXT FROM disable_cursor INTO @SchemaName, @TableName;
    END;
    CLOSE disable_cursor;
    DEALLOCATE disable_cursor;

    DECLARE delete_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT SchemaName, TableName FROM @Tables;

    OPEN delete_cursor;
    FETCH NEXT FROM delete_cursor INTO @SchemaName, @TableName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'DELETE FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
            + N'; SET @RowsDeleted = @@ROWCOUNT;';
        EXEC sys.sp_executesql @Sql, N'@RowsDeleted int OUTPUT', @RowsDeleted = @DeletedRows OUTPUT;
        PRINT CONCAT(N'Deleted ', @DeletedRows, N' row(s) from ', QUOTENAME(@SchemaName), N'.', QUOTENAME(@TableName), N'.');
        FETCH NEXT FROM delete_cursor INTO @SchemaName, @TableName;
    END;
    CLOSE delete_cursor;
    DEALLOCATE delete_cursor;

    DECLARE enable_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT SchemaName, TableName FROM @Tables;

    OPEN enable_cursor;
    FETCH NEXT FROM enable_cursor INTO @SchemaName, @TableName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
            + N' WITH CHECK CHECK CONSTRAINT ALL;';
        EXEC sys.sp_executesql @Sql;
        FETCH NEXT FROM enable_cursor INTO @SchemaName, @TableName;
    END;
    CLOSE enable_cursor;
    DEALLOCATE enable_cursor;

    COMMIT TRANSACTION;
    PRINT N'Application data cleared. EF migration history and table schemas were preserved.';
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'disable_cursor') >= 0
        CLOSE disable_cursor;
    IF CURSOR_STATUS('local', 'disable_cursor') > -3
        DEALLOCATE disable_cursor;
    IF CURSOR_STATUS('local', 'delete_cursor') >= 0
        CLOSE delete_cursor;
    IF CURSOR_STATUS('local', 'delete_cursor') > -3
        DEALLOCATE delete_cursor;
    IF CURSOR_STATUS('local', 'enable_cursor') >= 0
        CLOSE enable_cursor;
    IF CURSOR_STATUS('local', 'enable_cursor') > -3
        DEALLOCATE enable_cursor;

    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
