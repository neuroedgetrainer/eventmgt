
USE [EventMgt];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* =========================================================
   1. ROLES
   ========================================================= */

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        Name         NVARCHAR(50) NOT NULL,
        Description  NVARCHAR(250) NULL,
        IsActive     BIT NOT NULL
            CONSTRAINT DF_Roles_IsActive DEFAULT (1),
        CreatedDate  DATETIME2(0) NOT NULL
            CONSTRAINT DF_Roles_CreatedDate DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UX_Roles_Name UNIQUE (Name)
    );
END
GO

/* =========================================================
   2. USERS
   ========================================================= */

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        Name          NVARCHAR(150) NOT NULL,
        Email         NVARCHAR(320) NOT NULL,
        PasswordHash  NVARCHAR(500) NOT NULL,
        RoleId        INT NOT NULL,
        IsActive      BIT NOT NULL
            CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedDate   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate   DATETIME2(0) NULL,

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT FK_Users_Roles
            FOREIGN KEY (RoleId)
            REFERENCES dbo.Roles(Id)
            ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Users_Email'
      AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_Users_Email
        ON dbo.Users(Email);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_RoleId'
      AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_RoleId
        ON dbo.Users(RoleId);
END
GO

/* =========================================================
   3. CATEGORIES
   ========================================================= */

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        Name         NVARCHAR(100) NOT NULL,
        Description  NVARCHAR(500) NULL,
        IsActive     BIT NOT NULL
            CONSTRAINT DF_Categories_IsActive DEFAULT (1),
        CreatedDate  DATETIME2(0) NOT NULL
            CONSTRAINT DF_Categories_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate  DATETIME2(0) NULL,

        CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UX_Categories_Name UNIQUE (Name)
    );
END
GO

/* =========================================================
   4. VENUES
   ========================================================= */

IF OBJECT_ID(N'dbo.Venues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venues
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(200) NOT NULL,
        Address     NVARCHAR(500) NULL,
        City        NVARCHAR(100) NULL,
        State       NVARCHAR(100) NULL,
        Country     NVARCHAR(100) NULL,
        PostalCode  NVARCHAR(20) NULL,
        Capacity    INT NULL,
        IsActive    BIT NOT NULL
            CONSTRAINT DF_Venues_IsActive DEFAULT (1),
        CreatedDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Venues_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2(0) NULL,

        CONSTRAINT PK_Venues PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT CK_Venues_Capacity
            CHECK (Capacity IS NULL OR Capacity > 0)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Venues_City'
      AND object_id = OBJECT_ID(N'dbo.Venues')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Venues_City
        ON dbo.Venues(City);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Venues_IsActive'
      AND object_id = OBJECT_ID(N'dbo.Venues')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Venues_IsActive
        ON dbo.Venues(IsActive);
END
GO

/* =========================================================
   5. EVENTS
   ========================================================= */

IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Events
    (
        Id                    INT IDENTITY(1,1) NOT NULL,
        Title                 NVARCHAR(200) NOT NULL,
        Description           NVARCHAR(MAX) NULL,
        StartDate             DATETIME2(0) NOT NULL,
        EndDate               DATETIME2(0) NOT NULL,
        CategoryId            INT NOT NULL,
        VenueId               INT NULL,
        OrganizerId           INT NOT NULL,
        MaxParticipants       INT NOT NULL,

        Status                NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Events_Status DEFAULT (N'Draft'),

        RegistrationStartDate DATETIME2(0) NULL,
        RegistrationEndDate   DATETIME2(0) NULL,
        EventImageUrl         NVARCHAR(1000) NULL,

        EventMode             TINYINT NOT NULL
            CONSTRAINT DF_Events_EventMode DEFAULT (1),

        OnlineMeetingUrl      NVARCHAR(1000) NULL,

        IsActive              BIT NOT NULL
            CONSTRAINT DF_Events_IsActive DEFAULT (1),

        IsCancelled           BIT NOT NULL
            CONSTRAINT DF_Events_IsCancelled DEFAULT (0),

        CancellationReason    NVARCHAR(500) NULL,

        CreatedDate           DATETIME2(0) NOT NULL
            CONSTRAINT DF_Events_CreatedDate DEFAULT (SYSUTCDATETIME()),

        UpdatedDate           DATETIME2(0) NULL,

        CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT FK_Events_Categories
            FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(Id)
            ON DELETE NO ACTION,

        CONSTRAINT FK_Events_Venues
            FOREIGN KEY (VenueId)
            REFERENCES dbo.Venues(Id)
            ON DELETE NO ACTION,

        CONSTRAINT FK_Events_Organizer
            FOREIGN KEY (OrganizerId)
            REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION,

        CONSTRAINT CK_Events_DateRange
            CHECK (EndDate > StartDate),

        CONSTRAINT CK_Events_MaxParticipants
            CHECK (MaxParticipants > 0),

        CONSTRAINT CK_Events_Status
            CHECK
            (
                Status IN
                (
                    N'Draft',
                    N'Published',
                    N'Completed',
                    N'Cancelled'
                )
            ),

        CONSTRAINT CK_Events_RegistrationDates
            CHECK
            (
                RegistrationStartDate IS NULL
                OR RegistrationEndDate IS NULL
                OR RegistrationEndDate >= RegistrationStartDate
            ),

        CONSTRAINT CK_Events_RegistrationWithinEvent
            CHECK
            (
                RegistrationStartDate IS NULL
                OR RegistrationStartDate <= StartDate
            ),

        CONSTRAINT CK_Events_EventMode
            CHECK (EventMode IN (1, 2, 3)),

        CONSTRAINT CK_Events_ModeRequirements
            CHECK
            (
                (EventMode = 1
                    AND VenueId IS NOT NULL
                    AND OnlineMeetingUrl IS NULL)

                OR

                (EventMode = 2
                    AND VenueId IS NULL
                    AND OnlineMeetingUrl IS NOT NULL)

                OR

                (EventMode = 3
                    AND VenueId IS NOT NULL
                    AND OnlineMeetingUrl IS NOT NULL)
            ),

        CONSTRAINT CK_Events_Cancellation
            CHECK
            (
                IsCancelled = 0
                OR Status = N'Cancelled'
            )
    );
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_CategoryId'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_CategoryId
        ON dbo.Events(CategoryId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_VenueId'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_VenueId
        ON dbo.Events(VenueId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_OrganizerId'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_OrganizerId
        ON dbo.Events(OrganizerId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_StartDate'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_StartDate
        ON dbo.Events(StartDate);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_Status'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_Status
        ON dbo.Events(Status);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Events_IsActive_Status_StartDate'
      AND object_id = OBJECT_ID(N'dbo.Events')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Events_IsActive_Status_StartDate
        ON dbo.Events(IsActive, Status, StartDate);
END
GO

/* =========================================================
   6. EVENT REGISTRATIONS
   ========================================================= */

IF OBJECT_ID(N'dbo.EventRegistrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventRegistrations
    (
        Id               INT IDENTITY(1,1) NOT NULL,
        EventId          INT NOT NULL,
        UserId           INT NOT NULL,

        RegistrationDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_EventRegistrations_RegistrationDate
            DEFAULT (SYSUTCDATETIME()),

        Status           NVARCHAR(20) NOT NULL
            CONSTRAINT DF_EventRegistrations_Status
            DEFAULT (N'Registered'),

        CancelledDate    DATETIME2(0) NULL,

        CreatedDate      DATETIME2(0) NOT NULL
            CONSTRAINT DF_EventRegistrations_CreatedDate
            DEFAULT (SYSUTCDATETIME()),

        UpdatedDate      DATETIME2(0) NULL,

        CONSTRAINT PK_EventRegistrations
            PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT FK_EventRegistrations_Events
            FOREIGN KEY (EventId)
            REFERENCES dbo.Events(Id)
            ON DELETE NO ACTION,

        CONSTRAINT FK_EventRegistrations_Users
            FOREIGN KEY (UserId)
            REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION,

        CONSTRAINT CK_EventRegistrations_Status
            CHECK (Status IN (N'Registered', N'Cancelled')),

        CONSTRAINT CK_EventRegistrations_Cancellation
            CHECK
            (
                (Status = N'Registered' AND CancelledDate IS NULL)
                OR
                (Status = N'Cancelled' AND CancelledDate IS NOT NULL)
            )
    );
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_EventRegistrations_EventId_UserId'
      AND object_id = OBJECT_ID(N'dbo.EventRegistrations')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_EventRegistrations_EventId_UserId
        ON dbo.EventRegistrations(EventId, UserId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_EventRegistrations_EventId'
      AND object_id = OBJECT_ID(N'dbo.EventRegistrations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventRegistrations_EventId
        ON dbo.EventRegistrations(EventId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_EventRegistrations_UserId'
      AND object_id = OBJECT_ID(N'dbo.EventRegistrations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventRegistrations_UserId
        ON dbo.EventRegistrations(UserId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_EventRegistrations_UserId_Status'
      AND object_id = OBJECT_ID(N'dbo.EventRegistrations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventRegistrations_UserId_Status
        ON dbo.EventRegistrations(UserId, Status);
END
GO

/* =========================================================
   7. SEED DATA
   ========================================================= */

BEGIN TRANSACTION;

INSERT INTO dbo.Roles (Name, Description)
SELECT N'Admin', N'Full system administration access'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Roles WHERE Name = N'Admin'
);

INSERT INTO dbo.Roles (Name, Description)
SELECT N'Organizer', N'Creates and manages owned events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Roles WHERE Name = N'Organizer'
);

INSERT INTO dbo.Roles (Name, Description)
SELECT N'Participant', N'Can browse events and manage own registrations'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Roles WHERE Name = N'Participant'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Technology', N'Technology and software related events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Technology'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Business', N'Business, entrepreneurship and leadership events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Business'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Education', N'Education and academic events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Education'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Entertainment', N'Entertainment and cultural events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Entertainment'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Sports', N'Sports and fitness events'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Sports'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Workshop', N'Hands-on workshops and practical learning'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Workshop'
);

INSERT INTO dbo.Categories (Name, Description)
SELECT N'Conference', N'Professional conferences and summits'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Categories WHERE Name = N'Conference'
);

/* Venues */

INSERT INTO dbo.Venues
    (Name, Address, City, State, Country, PostalCode, Capacity)
SELECT
    N'Pune Convention Centre',
    N'Baner Road',
    N'Pune',
    N'Maharashtra',
    N'India',
    N'411045',
    500
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Venues
    WHERE Name = N'Pune Convention Centre'
);

INSERT INTO dbo.Venues
    (Name, Address, City, State, Country, PostalCode, Capacity)
SELECT
    N'Wagholi Community Hall',
    N'Kesnand Road',
    N'Pune',
    N'Maharashtra',
    N'India',
    N'412207',
    250
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Venues
    WHERE Name = N'Wagholi Community Hall'
);

INSERT INTO dbo.Venues
    (Name, Address, City, State, Country, PostalCode, Capacity)
SELECT
    N'Innovation Hub',
    N'Kharadi',
    N'Pune',
    N'Maharashtra',
    N'India',
    N'411014',
    150
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Venues
    WHERE Name = N'Innovation Hub'
);

/* Users deliberately use placeholder hashes.
   Replace through ASP.NET Core PasswordHasher<User>. */

DECLARE @AdminRoleId INT =
    (SELECT Id FROM dbo.Roles WHERE Name = N'Admin');

DECLARE @OrganizerRoleId INT =
    (SELECT Id FROM dbo.Roles WHERE Name = N'Organizer');

DECLARE @ParticipantRoleId INT =
    (SELECT Id FROM dbo.Roles WHERE Name = N'Participant');

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'System Administrator',
    N'admin@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @AdminRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'admin@example.com'
);

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'Rahul Sharma',
    N'organizer1@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @OrganizerRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'organizer1@example.com'
);

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'Priya Patil',
    N'organizer2@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @OrganizerRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'organizer2@example.com'
);

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'Amit Joshi',
    N'participant1@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @ParticipantRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'participant1@example.com'
);

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'Neha Kulkarni',
    N'participant2@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @ParticipantRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'participant2@example.com'
);

INSERT INTO dbo.Users
    (Name, Email, PasswordHash, RoleId)
SELECT
    N'Vikas Mehta',
    N'participant3@example.com',
    N'REPLACE_WITH_ASPNET_CORE_PASSWORD_HASH',
    @ParticipantRoleId
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Users
    WHERE Email = N'participant3@example.com'
);

/* Resolve IDs */

DECLARE @TechCategoryId INT =
    (SELECT Id FROM dbo.Categories WHERE Name = N'Technology');

DECLARE @BusinessCategoryId INT =
    (SELECT Id FROM dbo.Categories WHERE Name = N'Business');

DECLARE @EducationCategoryId INT =
    (SELECT Id FROM dbo.Categories WHERE Name = N'Education');

DECLARE @SportsCategoryId INT =
    (SELECT Id FROM dbo.Categories WHERE Name = N'Sports');

DECLARE @WorkshopCategoryId INT =
    (SELECT Id FROM dbo.Categories WHERE Name = N'Workshop');

DECLARE @PuneConventionVenueId INT =
    (SELECT Id FROM dbo.Venues WHERE Name = N'Pune Convention Centre');

DECLARE @WagholiHallVenueId INT =
    (SELECT Id FROM dbo.Venues WHERE Name = N'Wagholi Community Hall');

DECLARE @InnovationHubVenueId INT =
    (SELECT Id FROM dbo.Venues WHERE Name = N'Innovation Hub');

DECLARE @Organizer1Id INT =
    (SELECT Id FROM dbo.Users WHERE Email = N'organizer1@example.com');

DECLARE @Organizer2Id INT =
    (SELECT Id FROM dbo.Users WHERE Email = N'organizer2@example.com');

DECLARE @Participant1Id INT =
    (SELECT Id FROM dbo.Users WHERE Email = N'participant1@example.com');

DECLARE @Participant2Id INT =
    (SELECT Id FROM dbo.Users WHERE Email = N'participant2@example.com');

DECLARE @Participant3Id INT =
    (SELECT Id FROM dbo.Users WHERE Email = N'participant3@example.com');

/* Events */

INSERT INTO dbo.Events
(
    Title, Description, StartDate, EndDate,
    CategoryId, VenueId, OrganizerId, MaxParticipants,
    Status, RegistrationStartDate, RegistrationEndDate,
    EventMode, OnlineMeetingUrl
)
SELECT
    N'ASP.NET Core Web API Masterclass',
    N'Hands-on event covering production-ready ASP.NET Core Web API development.',
    '2026-10-10T10:00:00',
    '2026-10-10T17:00:00',
    @TechCategoryId,
    @PuneConventionVenueId,
    @Organizer1Id,
    300,
    N'Published',
    '2026-09-01T00:00:00',
    '2026-10-09T23:59:59',
    1,
    NULL
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Events
    WHERE Title = N'ASP.NET Core Web API Masterclass'
);

INSERT INTO dbo.Events
(
    Title, Description, StartDate, EndDate,
    CategoryId, VenueId, OrganizerId, MaxParticipants,
    Status, RegistrationStartDate, RegistrationEndDate,
    EventMode, OnlineMeetingUrl
)
SELECT
    N'Entrepreneurship & Startup Summit',
    N'Business networking and startup strategy summit.',
    '2026-10-17T09:30:00',
    '2026-10-17T18:00:00',
    @BusinessCategoryId,
    @PuneConventionVenueId,
    @Organizer2Id,
    400,
    N'Published',
    '2026-09-05T00:00:00',
    '2026-10-16T23:59:59',
    1,
    NULL
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Events
    WHERE Title = N'Entrepreneurship & Startup Summit'
);

INSERT INTO dbo.Events
(
    Title, Description, StartDate, EndDate,
    CategoryId, VenueId, OrganizerId, MaxParticipants,
    Status, RegistrationStartDate, RegistrationEndDate,
    EventMode, OnlineMeetingUrl
)
SELECT
    N'AI & Machine Learning Workshop',
    N'Practical introduction to AI and machine learning concepts.',
    '2026-11-07T10:00:00',
    '2026-11-07T16:00:00',
    @WorkshopCategoryId,
    @InnovationHubVenueId,
    @Organizer1Id,
    120,
    N'Published',
    '2026-09-10T00:00:00',
    '2026-11-06T23:59:59',
    1,
    NULL
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Events
    WHERE Title = N'AI & Machine Learning Workshop'
);

INSERT INTO dbo.Events
(
    Title, Description, StartDate, EndDate,
    CategoryId, VenueId, OrganizerId, MaxParticipants,
    Status, RegistrationStartDate, RegistrationEndDate,
    EventMode, OnlineMeetingUrl
)
SELECT
    N'Full Stack Development Live Session',
    N'Online full stack development learning session.',
    '2026-11-21T11:00:00',
    '2026-11-21T14:00:00',
    @EducationCategoryId,
    NULL,
    @Organizer2Id,
    500,
    N'Published',
    '2026-09-15T00:00:00',
    '2026-11-20T23:59:59',
    2,
    N'https://example.com/online-meeting'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Events
    WHERE Title = N'Full Stack Development Live Session'
);

INSERT INTO dbo.Events
(
    Title, Description, StartDate, EndDate,
    CategoryId, VenueId, OrganizerId, MaxParticipants,
    Status, RegistrationStartDate, RegistrationEndDate,
    EventMode, OnlineMeetingUrl
)
SELECT
    N'Community Sports Day',
    N'Community sports and fitness event.',
    '2026-12-05T08:00:00',
    '2026-12-05T17:00:00',
    @SportsCategoryId,
    @WagholiHallVenueId,
    @Organizer1Id,
    200,
    N'Published',
    '2026-10-01T00:00:00',
    '2026-12-04T23:59:59',
    1,
    NULL
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Events
    WHERE Title = N'Community Sports Day'
);

/* Registrations */

DECLARE @Event1Id INT =
    (SELECT Id FROM dbo.Events
     WHERE Title = N'ASP.NET Core Web API Masterclass');

DECLARE @Event2Id INT =
    (SELECT Id FROM dbo.Events
     WHERE Title = N'Entrepreneurship & Startup Summit');

DECLARE @Event3Id INT =
    (SELECT Id FROM dbo.Events
     WHERE Title = N'AI & Machine Learning Workshop');

DECLARE @Event4Id INT =
    (SELECT Id FROM dbo.Events
     WHERE Title = N'Full Stack Development Live Session');

INSERT INTO dbo.EventRegistrations
    (EventId, UserId, RegistrationDate, Status)
SELECT
    @Event1Id,
    @Participant1Id,
    '2026-09-02T10:00:00',
    N'Registered'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EventRegistrations
    WHERE EventId = @Event1Id
      AND UserId = @Participant1Id
);

INSERT INTO dbo.EventRegistrations
    (EventId, UserId, RegistrationDate, Status)
SELECT
    @Event1Id,
    @Participant2Id,
    '2026-09-02T11:00:00',
    N'Registered'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EventRegistrations
    WHERE EventId = @Event1Id
      AND UserId = @Participant2Id
);

INSERT INTO dbo.EventRegistrations
    (EventId, UserId, RegistrationDate, Status)
SELECT
    @Event2Id,
    @Participant1Id,
    '2026-09-03T10:00:00',
    N'Registered'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EventRegistrations
    WHERE EventId = @Event2Id
      AND UserId = @Participant1Id
);

INSERT INTO dbo.EventRegistrations
    (EventId, UserId, RegistrationDate, Status)
SELECT
    @Event3Id,
    @Participant2Id,
    '2026-09-03T11:00:00',
    N'Registered'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EventRegistrations
    WHERE EventId = @Event3Id
      AND UserId = @Participant2Id
);

INSERT INTO dbo.EventRegistrations
    (EventId, UserId, RegistrationDate, Status)
SELECT
    @Event4Id,
    @Participant3Id,
    '2026-09-03T12:00:00',
    N'Registered'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.EventRegistrations
    WHERE EventId = @Event4Id
      AND UserId = @Participant3Id
);

COMMIT TRANSACTION;
GO



/* =========================================================
   VERIFICATION
   ========================================================= */

SELECT N'Roles' AS TableName, COUNT(*) AS RRowCount
FROM dbo.Roles

UNION ALL

SELECT N'Users', COUNT(*)
FROM dbo.Users

UNION ALL

SELECT N'Categories', COUNT(*)
FROM dbo.Categories

UNION ALL

SELECT N'Venues', COUNT(*)
FROM dbo.Venues

UNION ALL

SELECT N'Events', COUNT(*)
FROM dbo.Events

UNION ALL

SELECT N'EventRegistrations', COUNT(*)
FROM dbo.EventRegistrations;
GO