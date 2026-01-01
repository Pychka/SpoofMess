CREATE EXTENSION "uuid-ossp";
CREATE TABLE "UserEntry"
(
    "Id" UUID PRIMARY KEY,
    "PasswordHash" VARCHAR(100) NOT NULL,
    "UniqueName" VARCHAR(100) UNIQUE NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX "IX_UserEntry_UniqueName" ON "UserEntry"("UniqueName");

CREATE TABLE "SessionInfo"
(
    "Id" UUID PRIMARY KEY,
    "UserEntryId" UUID references "UserEntry"("Id") NOT NULL,
    "DeviceId" VARCHAR(100) NOT NULL,
    "DeviceName" VARCHAR(255) default null,
    "Platform" VARCHAR(50) default null,
    "UserAgent" VARCHAR(500) default null,
    "IpAddress" VARCHAR(45) default null,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastActivityAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX "IX_SessionInfo_Id" ON "SessionInfo"("Id");
CREATE INDEX "IX_SessionInfo_UserId_Active" 
    ON "SessionInfo"("UserEntryId") 
    WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE;

CREATE TABLE "Token"
(
    "RefreshTokenHash" VARCHAR(100) PRIMARY KEY,
    "SessionInfoId" UUID references "SessionInfo"("Id") NOT NULL,
    "ValidTo" TIMESTAMPTZ NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX "IX_Token_SessionInfoId" 
    ON "Token"("SessionInfoId") 
    WHERE "IsDeleted" = FALSE;
    
CREATE INDEX "IX_Token_ValidTo" 
    ON "Token"("RefreshTokenHash") 
    WHERE "IsDeleted" = FALSE;