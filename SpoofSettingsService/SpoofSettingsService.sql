create table "User"
(
	"Id" uuid constraint "PK_User_Id" primary key,
	"WasOnline" timestamptz not null default CURRENT_TIMESTAMP,
	"Name" varchar(100) not null,
	"Login" varchar(100) not null unique,
	"MonthsBeforeDelete" int not null default 6,
	"SearchMe" boolean not null default true,
	"ShowMe" boolean not null default true,
	"ForwardMessage" boolean not null default true,
	"InviteMe" boolean not null default true,
	"IsDeleted" boolean not null default false,
    "LastModified" timestamptz not null default CURRENT_TIMESTAMP
);

create table "ChatType"
(
    "Id" serial constraint "PK_ChatType_Id" primary key,
    "Name" varchar(50) not null unique,
	"IsDeleted" boolean not null default false
);

create table "ChatProperty"
(
    "Id" smallserial constraint "PK_ChatProperty_Id" primary key,
    "Name" varchar(50) not null,
    "Description" varchar(100),
	"IsDeleted" boolean not null default false
);

create table "ChatTypeChatProperty"
(
    "ChatTypeId" int not null constraint "FK_ChatTypeChatProperty_ChatTypeId" references "ChatType"("Id") on delete cascade,
    "ChatPropertyId" smallint not null constraint "FK_ChatTypeChatProperty_ChatPropertyId" references "ChatProperty"("Id") on delete cascade,
	"IsDeleted" boolean not null default false,
    constraint "PK_ChatTypeChatProperty_Id" primary key("ChatTypeId", "ChatPropertyId")
);

create table "GlobalPermission"
(
    "Id" serial constraint "PK_GlobalPermission_Id" primary key,
    "Name" varchar(50) not null unique,
    "Description" varchar(100),
	"IsDeleted" boolean not null default false
);

create table "Chat"
(
    "Id" uuid constraint "PK_Chat_Id" primary key default uuidv7(),
    "ChatTypeId" int not null constraint "FK_Chat_ChatTypeId" references "ChatType"("Id") on delete cascade,
	"OwnerId" uuid constraint "FK_Chat_OwnerId" references "User"("Id") on delete set null,
    "UniqueName" varchar(100) unique not null,
    "Name" varchar(100) not null,
    "CreatedAt" timestamptz not null default CURRENT_TIMESTAMP,
    "LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create table "RoleRank"
(
    "Id" bigserial constraint "PK_RoleRank_Id" primary key,
    "ChatId" uuid not null constraint "FK_RoleRank_ChatId" references "Chat"("Id") on delete cascade,
	"Level" smallint not null,
	"Name" varchar(50) not null,
	check("Level" between 0 and 255),
	unique("ChatId", "Name")
);

create table "UserGlobalPermission"
(
    "GlobalPermissionId" int not null constraint "FK_UserGlobalPermission_ChatId" references "GlobalPermission"("Id") on delete cascade,
    "UserId" uuid not null constraint "FK_UserGlobalPermission_UserId" references "User"("Id") on delete cascade,
    constraint "PK_UserGlobalPermission_Id" primary key("GlobalPermissionId", "UserId")
);

create table "ChatRole"
(
    "Id" bigserial constraint "PK_ChatRole_Id" primary key,
    "ChatId" uuid not null constraint "FK_ChatRole_ChatId" references "Chat"("Id") on delete cascade,
	"RoleRankId" bigint not null constraint "FK_ChatRole_RoleRankId" references "RoleRank"("Id") on delete cascade,
    "Name" varchar(50) not null,
	"IsDeleted" boolean not null default false,
	constraint "UQ_ChatRole_Chat_Name" unique ("ChatId", "Name")
);

alter table "Chat" add "BaseRoleId" bigint constraint "FK_Chat_BaseRoleId" references "ChatRole"("Id") on delete set null;

create table "Permission"
(
    "Id" smallint constraint "PK_Permission_Id" primary key,
    "Name" varchar(50) not null,
    "Description" varchar(100),
	"IsDeleted" boolean not null default false
);

create table "ChatRoleRules"
(
    "ChatRoleId" bigint not null constraint "FK_ChatRoleRules_ChatRoleId" references "ChatRole"("Id") on delete cascade,
    "PermissionId" smallint not null constraint "FK_ChatRoleRules_PermissionId" references "Permission"("Id") on delete cascade,
	"IsPermission" boolean not null default true,
    constraint "PK_ChatRoleRules_Id" primary key("ChatRoleId", "PermissionId")
);

create table "FileMetadata" (
    "Id" uuid constraint "PK_FileMetadata_Id" primary key,
    "Size" bigint not null,
	"Category" varchar(20) not null,
	"Metadata" jsonb,
	"IsDeleted" boolean not null default false
);

create TABLE "ChatUser"
(
    "ChatId" uuid not null constraint "FK_ChatUser_ChatId" references "Chat"("Id") on delete cascade,
    "UserId" uuid not null constraint "FK_ChatUser_UserId" references "User"("Id") on delete cascade,
	"JoinedAt" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false,
    constraint "PK_ChatUser_Id" primary key("ChatId", "UserId")
);
create unique index "UX_ChatUser_ChatId_UserId" on "ChatUser"("ChatId", "UserId") where "IsDeleted" = false;
create index "IX_ChatUser_UserChat" on "ChatUser"("UserId", "ChatId") where "IsDeleted" = false;
create index "IX_ChatUser_ChatId" on "ChatUser"("ChatId") where "IsDeleted" = false;

create table "ChatUserChatRole"
(
    "ChatId" uuid not null,
    "UserId" uuid not null,
    "ChatRoleId" bigint not null constraint "FK_ChatUserChatRole_ChatRoleId" references "ChatRole"("Id"),
	"IsDeleted" boolean not null default false,
	"TimeSet" timestamptz not null default CURRENT_TIMESTAMP,
    constraint "FK_ChatUserChatRole_ChatUserId" foreign key("ChatId", "UserId") references "ChatUser"("ChatId", "UserId") on delete cascade,
    constraint "PK_ChatUserChatRole_Id" primary key("ChatId", "UserId", "ChatRoleId")
);

create table "ChatUserRules"
(
    "ChatId" uuid not null,
    "UserId" uuid not null,
	"IsPermission" boolean not null default true,
    "PermissionId" smallint not null constraint "FK_ChatUserPermission_PermissionId" references "Permission"("Id"),
    constraint "FK_ChatUserPermission_ChatUserId" foreign key("ChatId", "UserId") references "ChatUser"("ChatId", "UserId") on delete cascade,
    constraint "PK_ChatUserPermission_Id" primary key("ChatId", "UserId", "PermissionId")
);

create table "StickerPack"
(
	"Id" bigserial constraint "PK_StickerPack_Id" primary key,
	"AuthorId" uuid not null constraint "FK_StickerPack_AuthorId" references "User"("Id") on delete cascade,
	"PreviewId" uuid not null constraint "FK_StickerPack_PreviewId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text not null,
	"Title" varchar(100),
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create index "IX_StickerPack_AuthorId" on "StickerPack"("AuthorId") where "IsDeleted" = false;

create table "Sticker"
(
	"Id" uuid constraint "PK_Sticker_Id" primary key default uuidv7(),
	"StickerPackId" bigint not null constraint "FK_Sticker_StickerPackId" references "StickerPack"("Id") on delete cascade,
	"FileId" uuid not null constraint "FK_Sticker_FileId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text not null,
	"Title" varchar(50) not null,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create table "UserAvatar"
(
	"Id" uuid constraint "PK_UserAvatar_Id" primary key default uuidv7(),
	"UserId" uuid not null constraint "FK_UserAvatar_UserId" references "User"("Id") on delete cascade,
	"FileId" uuid not null constraint "FK_UserAvatar_FileId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text not null,
	"IsActive" boolean not null default true,
	"IsDeleted" boolean not null default false,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP
);

create table "ChatAvatar"
(
	"Id" uuid constraint "PK_ChatAvatar_Id" primary key default uuidv7(),
	"ChatId" uuid not null constraint "FK_ChatAvatar_ChatId" references "Chat"("Id") on delete cascade,
	"FileId" uuid not null constraint "FK_ChatAvatar_FileId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text not null,
	"IsActive" boolean not null default true,
	"IsDeleted" boolean not null default false,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP
);

create type "OutboxStatus" as enum 
(
    'Delete',
	'Create',
	'Update',
	'Rollback'
);

create table "ChatUserOutbox"
(
	"Id" uuid constraint "PK_ChatUserOutbox_Id" primary key default uuidv7(),
    "ChatId" uuid not null,
    "UserId" uuid not null,
	"IsSynced" boolean not null default false,
	"LastTryDate" timestamptz not null default CURRENT_TIMESTAMP,
	"CreatedAt" timestamptz not null default CURRENT_TIMESTAMP,
	"Data" jsonb not null,
	"Status" "OutboxStatus" not null,
	constraint "FK_ChatUserOutbox_ChatUserId" foreign key("ChatId", "UserId") references "ChatUser"("ChatId", "UserId") on delete cascade
);

insert into "ChatType"("Id", "Name")
values 
(0, 'Private'),
(1, 'Group'),
(2, 'Channel');

insert into "Permission"("Id", "Name")
values
--1xx: content and messages
(100, 'SendTexts'),
(101, 'SendAudios'),
(102, 'SendVideos'),
(103, 'SendFiles'),
(104, 'SendEmoji'),
(105, 'SendSticker'),
(106, 'SendVoiceMessage'),
(107, 'SendVideoMessage'),
(150, 'ShareMessage'),
(151, 'DeleteMessage'),
(152, 'EditMessage'),
--2xx: moderation and roles
(200, 'CreateRole'),
(201, 'SetRole'),
(202, 'DepriveRole'),
(240, 'ChangeRules'),
(260, 'Muting'),
(261, 'Restricting'),
--3xx: settings and members moderation
(300, 'Inviting'),
(301, 'Kicking'),
(302, 'Banning'),
(303, 'Sharing'),
(330, 'ChangeSettings'),
(331, 'ChangeAvatar'),
(332, 'DeleteAvatar');

create or replace function "LogChatUserToOutbox"()
returns trigger as $$
declare
	roleId bigint;
begin
	select "BaseRoleId" into roleId from "Chat" where "Id" = new."ChatId" limit 1;
    insert into "ChatUserOutbox"("ChatId", "UserId", "Status", "Data")
    values (
        new."ChatId",
        new."UserId",
        'Create',
        jsonb_build_object(
	        'ChatId', new."ChatId",
	        'UserId', new."UserId",
	        'Rules', (SELECT jsonb_agg(row_to_json(t)) 
                FROM get_user_permission(
					new."UserId",
					new."ChatId",
					array[
						100,
						101,
						102,
						103,
						104,
						105,
						106,
						107,
						150,
						151,
						152
					]::smallint[]
					) t
			)
    	)
    );
	if roleId is not null then
		insert into "ChatUserChatRole"("ChatId", "UserId", "ChatRoleId")
		values
		(new."ChatId", new."UserId", roleId);
	end if;
    return new;
end;
$$ language plpgsql;
create trigger "Trg_ChatUser_Outbox"
after insert on "ChatUser"
for each row execute function "LogChatUserToOutbox"();

create or replace function "LogChatUserChatRoleToOutbox"()
returns trigger as $$
begin
    insert into "ChatUserOutbox"("ChatId", "UserId", "Status", "Data")
    values (
        new."ChatId",
        new."UserId",
        'Update',
        jsonb_build_object(
	        'ChatId', new."ChatId",
	        'UserId', new."UserId",
	        'Rules', (SELECT jsonb_agg(row_to_json(t)) 
                FROM get_user_permission(
					new."UserId", 
					new."ChatId",
					array[
						100,
						101,
						102,
						103,
						104,
						105,
						106,
						107,
						150,
						151,
						152
					]::smallint[]
					) t
			)
    	)
    );
    return new;
end;
$$ language plpgsql;
create trigger "Trg_ChatUserChatRole_Outbox"
after insert on "ChatUserChatRole"
for each row execute function "LogChatUserChatRoleToOutbox"();

create or replace function "LogChatRoleRulesToOutbox"()
returns trigger as $$
begin
    insert into "ChatUserOutbox"("ChatId", "UserId", "Status", "Data")
	select 
		crr."ChatId", 
		cucr."UserId",
		'Update',
		jsonb_build_object(
	        'ChatId', crr."ChatId",
	        'UserId', cucr."UserId",
	        'Rules', (SELECT jsonb_agg(row_to_json(t)) 
	            FROM get_user_permission(
					cucr."UserId",
					crr."ChatId",
					array[
						100,
						101,
						102,
						103,
						104,
						105,
						106,
						107,
						150,
						151,
						152
					]::smallint[]
					) t
			)) from
	"ChatRole" crr 
	join "ChatUserChatRole" cucr on cucr."ChatId" = crr."ChatId" and cucr."ChatRoleId" = crr."Id"
	where crr."Id" = 1;
    return new;
end;
$$ language plpgsql;
create trigger "Trg_ChatRoleRules_Outbox"
after update on "ChatRoleRules"
for each row execute function "LogChatRoleRulesToOutbox"();

create or replace function "CreateRolesAfterChat"()
returns trigger as
$$
declare
	topRankId bigint;
	middleRankId bigint;
	leastRankId bigint;
	ownerRoleId bigint;
	adminRoleId bigint;
	memberRoleId bigint;
begin
	if new."IsDeleted" THEN
		return new;
	end if;
	
	insert into "RoleRank"("ChatId", "Level", "Name")
	values (new."Id", 0, 'Top')
	returning "Id" into topRankId; 

	insert into "RoleRank"("ChatId", "Level", "Name")
	values (new."Id", 2, 'Middle')
	returning "Id" into middleRankId; 

	insert into "RoleRank"("ChatId", "Level", "Name")
	values (new."Id", 5, 'Least')
	returning "Id" into leastRankId; 

	insert into "ChatRole"("ChatId", "Name", "RoleRankId")
	values (new."Id", 'Owner', topRankId)
	returning "Id" into ownerRoleId;

	insert into "ChatRole"("ChatId", "Name", "RoleRankId")
	values (new."Id", 'Admin', middleRankId)
	returning "Id" into adminRoleId;

	insert into "ChatRole"("ChatId", "Name", "RoleRankId")
	values (new."Id", 'Member', leastRankId)
	returning "Id" into memberRoleId;

	insert into "ChatRoleRules"("ChatRoleId", "PermissionId", "IsPermission")
	select memberRoleId, "Id", true 
	from "Permission" 
	where "Id" between 100 and 199;

	insert into "ChatRoleRules"("ChatRoleId", "PermissionId", "IsPermission")
	select adminRoleId, "Id", true 
	from "Permission" 
	where "Id" between 100 and 199
	or "Id" in (260, 261, 300, 301, 302, 303);

	insert into "ChatRoleRules"("ChatRoleId", "PermissionId", "IsPermission")
	select ownerRoleId, "Id", true from "Permission";

	insert into "ChatUser"("ChatId", "UserId")
	values
	(new."Id", new."OwnerId");

	insert into "ChatUserChatRole"("ChatId", "UserId", "ChatRoleId")
	values
	(new."Id", new."OwnerId", ownerRoleId);
	
	update "Chat" set "BaseRoleId" = memberRoleId where "Id" = new."Id";
	
	return new;
end;
$$ language plpgsql;


create trigger "Trg_Chat_After_Insert" after insert on "Chat"
for each row
execute function "CreateRolesAfterChat"();

create or replace function check_user_permission(
	p_user_id uuid,
	p_chat_id uuid,
	p_permission_id smallint
)
returns int as
$$
declare
isPermission bool;
begin
	select "IsPermission" into isPermission
	from "ChatUserRules" 
	where "ChatId" = p_chat_id
		and "UserId" = p_user_id 
		and "PermissionId" = p_permission_id;

	if found then
		return
			case when isPermission
				then 1
				else 2
			end;
	end if;

	select crr."IsPermission" into isPermission
	from "ChatUserChatRole" cucr
	join "ChatRole" cr on cr."Id" = cucr."ChatRoleId" 
	join "RoleRank" rr on rr."Id" = cr."RoleRankId"
	join "ChatRoleRules" crr on crr."ChatRoleId" = cucr."ChatRoleId"
		and crr."PermissionId" = p_permission_id
	where cucr."ChatId" = p_chat_id
		and cucr."UserId" = p_user_id
	order by rr."Level" asc
	limit 1;

	if found then
		return case when isPermission then 1 else 2 end;
	end if;
	return 0;
end;
$$ language plpgsql;

CREATE TYPE permission_result AS 
( 
	"RuleId" smallint, 
	"IsAllowed" boolean 
); 

create or replace function get_user_permission
(
	p_user_id uuid, 
	p_chat_id uuid, 
	p_mask smallint[] 
) RETURNS SETOF permission_result
as $$
begin
return query select distinct on ( rules."PermissionId" ) rules."PermissionId" AS "RuleId", rules."IsPermission" AS "IsAllowed"
	from (
		select cur."PermissionId", cur."IsPermission", 1 as priority from "ChatUser" 
		ch join "ChatUserRules" cur on cur."ChatId" = ch."ChatId"
			and cur."UserId" = ch."UserId" 
		where ch."ChatId" = p_chat_id 
			and ch."UserId" = p_user_id 
			and (
				p_mask is null
				or cardinality(p_mask) = 0
				or cur."PermissionId" = any(p_mask)
			)
		union all 
		select crr."PermissionId", crr."IsPermission", 2 as priority 
		from "ChatUserChatRole" cucr 
		join "ChatRole" cr on cucr."ChatRoleId" = cr."Id"
		join "ChatRoleRules" crr on crr."ChatRoleId" = cr."Id"
		where cucr."ChatId" = p_chat_id
			and cucr."UserId" = p_user_id
			and (
				p_mask is null
				or cardinality(p_mask) = 0
				or crr."PermissionId" = any(p_mask)
			)
	) as rules
	order by rules."PermissionId",
		rules.priority,
		rules."IsPermission";
end; 
$$ language plpgsql;

CREATE OR REPLACE FUNCTION "OnceActiveChatAvatar"()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "ChatAvatar" 
    SET "IsActive" = FALSE 
    WHERE "ChatId" = NEW."ChatId" 
      AND "Id" != NEW."Id" 
      AND "IsActive" = TRUE;
    RETURN NEW;
END;
$$ LANGUAGE PLPGSQL;


CREATE TRIGGER "Trg_ChatAvatar_AfterInsert"
AFTER INSERT ON "ChatAvatar"
FOR EACH ROW EXECUTE FUNCTION "OnceActiveChatAvatar"();


CREATE OR REPLACE FUNCTION "OnceActiveUserAvatar"()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "UserAvatar" 
    SET "IsActive" = FALSE 
    WHERE "UserId" = NEW."UserId" 
      AND "Id" != NEW."Id" 
      AND "IsActive" = TRUE;
    RETURN NEW;
END;
$$ LANGUAGE PLPGSQL;


CREATE TRIGGER "Trg_UserAvatar_AfterInsert"
AFTER INSERT ON "UserAvatar"
FOR EACH ROW EXECUTE FUNCTION "OnceActiveUserAvatar"();
