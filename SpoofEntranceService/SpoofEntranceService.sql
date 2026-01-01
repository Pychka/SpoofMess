CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS UserEntry
(
    Id UUID PRIMARY KEY,
    PasswordHash VARCHAR(100) NOT NULL,
    UniqueName VARCHAR(100) UNIQUE NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS IX_UserEntry_UniqueName ON UserEntry(UniqueName);

CREATE TABLE IF NOT EXISTS SessionInfo
(
    Id UUID PRIMARY KEY,
    UserEntryId UUID NOT NULL,
    DeviceId VARCHAR(100) NOT NULL,
    DeviceName VARCHAR(255),
    Platform VARCHAR(50),
    UserAgent VARCHAR(500),
    IpAddress VARCHAR(45),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastActivityAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT FK_SessionInfo_UserEntryId 
        FOREIGN KEY (UserEntryId) 
        REFERENCES UserEntry(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_SessionInfo_Id ON SessionInfo(Id);
CREATE INDEX IF NOT EXISTS IX_SessionInfo_UserId_Active 
    ON SessionInfo(UserEntryId) 
    WHERE IsActive = TRUE AND IsDeleted = FALSE;

CREATE TABLE IF NOT EXISTS Token
(
    RefreshTokenHash VARCHAR(100) PRIMARY KEY,
    SessionInfoId UUID NOT NULL,
    ValidTo TIMESTAMP NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT FK_Token_SessionInfoId 
        FOREIGN KEY (SessionInfoId) 
        REFERENCES SessionInfo(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Token_SessionInfoId 
    ON Token(SessionInfoId) 
    WHERE IsDeleted = FALSE;
    
CREATE INDEX IF NOT EXISTS IX_Token_ValidTo 
    ON Token(RefreshTokenHash) 
    WHERE IsDeleted = FALSE;