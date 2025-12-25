CREATE DATABASE SpoofEntranceService;
GO
USE SpoofEntranceService;

CREATE LOGIN EntranceUser WITH PASSWORD = 'Str0ngP@ssw0rd2024!';
CREATE USER EntranceUser FOR LOGIN EntranceUser;
ALTER ROLE db_datareader ADD MEMBER EntranceUser;
ALTER ROLE db_datawriter ADD MEMBER EntranceUser;
GO

CREATE TABLE UserEntry
(
	Id UNIQUEIDENTIFIER CONSTRAINT PK_User_Id PRIMARY KEY,
	PasswordHash NVARCHAR(100) NOT NULL,
	UniqueName NVARCHAR(100) UNIQUE NOT NULL,
	IsDeleted BIT NOT NULL DEFAULT 0,
);
CREATE INDEX IX_SessionInfo_UserId_Active ON UserEntry(UniqueName);

CREATE TABLE SessionInfo
(
	Id UNIQUEIDENTIFIER CONSTRAINT PK_SessionInfo_Id PRIMARY KEY,
	UserEntryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SessionInfo_UserEntryId FOREIGN KEY REFERENCES UserEntry(Id) ON DELETE CASCADE,
    
    DeviceId NVARCHAR(100) NOT NULL,
    DeviceName NVARCHAR(255) NULL,
    [Platform] NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    IpAddress NVARCHAR(45) NULL,
    
    CreatedAt DATETIME NOT NULL,
    LastActivityAt DATETIME NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
	IsDeleted BIT NOT NULL DEFAULT 0,
);
GO
CREATE INDEX IX_SessionInfo_SessionId ON SessionInfo(Id);
CREATE INDEX IX_SessionInfo_UserId_Active ON SessionInfo(UserEntryId) WHERE IsActive = 1 AND IsDeleted = 0;

CREATE TABLE Token
(
	RefreshTokenHash NVARCHAR(100) CONSTRAINT PK_Token_Id PRIMARY KEY,
	SessionInfoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Token_SessionInfoId FOREIGN KEY REFERENCES SessionInfo(Id) ON DELETE CASCADE,
	ValidTo DATETIME NOT NULL,
	IsDeleted BIT NOT NULL DEFAULT 0,
);
CREATE INDEX IX_Token_SessionInfoId ON Token(SessionInfoId) WHERE IsDeleted = 0;
CREATE INDEX IX_Token_ValidTo ON Token(RefreshTokenHash) WHERE IsDeleted = 0;