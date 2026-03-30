create table "Category"
(
	"Id" smallint generated always as identity constraint "PK_Category_Id" primary key,
	"IsDeleted" boolean not null default false,
	"Name" varchar(20) not null constraint "UQ_Category_Name" unique
);

create table "Extension"
(
	"Id" smallint constraint "PK_Extension_Id" primary key,
	"CategoryId" smallint not null constraint "FK_Extension_CategoryId" references "Category"("Id"),
	"IsDeleted" boolean not null default false,
	"Name" varchar(20) not null constraint "UQ_Extension_Name" unique
);

create index "IX_Extension_Name" on "Extension" ("Name") where not "IsDeleted";

create table "FileObject"
(
	"Id" uuid constraint "PK_FileObject_Id" primary key,
	"L1" bytea,
	"L2" bytea,
	"L3" bytea not null,
	"ExtensionId" smallint not null constraint "FK_FileObject_ExtensionId" references "Extension"("Id"),
	"IsDeleted" boolean not null default false,
	"Path" text not null,
	"Size" bigint not null,
	"Metadata" jsonb,
	"LastModified" timestamp not null default CURRENT_TIMESTAMP,
	constraint "IX_FileObject_Check_L3" unique("L3", "Size", "ExtensionId"),
	check(("L1" is null and "L2" is null and "Size" <= 50000000) or ("L1" is not null and "L2" is not null and "Size" > 50000000)) --50mb
);

create index "IX_FileObject_Fast_Check_L1" on "FileObject"("L1", "Size", "ExtensionId");
create index "IX_FileObject_Fast_Check_L2" on "FileObject"("L2", "Size", "ExtensionId");

CREATE OR REPLACE FUNCTION "FindOrCreateExtension"(
	p_extensionId smallint,
	p_extensionName varchar(20),
	p_categoryName varchar(20)
)
RETURNS TABLE(
	"ExtensionId" smallint,
	"CategoryName" varchar(20)
) AS
$$
DECLARE
	v_categoryId smallint;
BEGIN
    INSERT INTO "Category" AS c ("Name")
    VALUES (p_categoryName)
    ON CONFLICT ("Name") DO UPDATE SET "Name" = EXCLUDED."Name"
    RETURNING c."Id" INTO v_categoryId;

    INSERT INTO "Extension" ("Id", "Name", "CategoryId")
    VALUES (p_extensionId, p_extensionName, v_categoryId)
    ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

    RETURN QUERY 
    SELECT 
        e."Id" AS "ExtensionId", 
        c."Name" AS "CategoryName"
    FROM "Extension" e
    JOIN "Category" c ON e."CategoryId" = c."Id"
    WHERE e."Id" = p_extensionId
	LIMIT 1;
END;
$$ language plpgsql;

create or replace function "FindOrCreateFile"(
	id uuid,
	l1 bytea, 
	l2 bytea, 
	l3 bytea, 
	extensionId smallint, 
	filePath text, 
	fileSize bigint,
	fileMetadata text)
returns uuid as
$$
declare
	inserted_id uuid;
begin
	INSERT INTO "FileObject"("Id", "L1", "L2", "L3", "ExtensionId", "Path", "Size", "Metadata")
    VALUES (id, l1, l2, l3, extensionId, filePath, fileSize, fileMetadata::jsonb)
    ON CONFLICT ("L3", "Size", "ExtensionId") DO NOTHING
    RETURNING "Id" INTO inserted_id;

    RETURN inserted_id; 
end;
$$ language plpgsql;