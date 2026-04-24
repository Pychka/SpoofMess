create table "FileMetadata" (
    "Id" uuid constraint "PK_FileMetadata_Id" primary key,
    "Size" bigint not null,
	"Category" varchar(20) not null,
	"Metadata" jsonb,
	"IsDeleted" boolean not null default false
);

create table "User"
(
	"Id" uuid constraint "PK_User_Id" primary key default uuidv7(),
	"AvatarId" uuid constraint "FK_Attachment_AvatarId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text,
	"Login" varchar(100) unique not null,
	"Name" varchar(100) not null,
	"IsConnected" boolean not null default false,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create table "Chat"
(
	"Id" uuid constraint "PK_Chat_Id" primary key default uuidv7(),
	"AvatarId" uuid constraint "FK_Attachment_AvatarId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text,
	"UniqueName" varchar(100) unique not null,
	"Name" varchar(100),
	"IsPublic" boolean not null default false,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create TABLE "ChatUser"
(
    "ChatId" uuid not null constraint "FK_ChatUser_ChatId" references "Chat"("Id") on delete cascade,
    "UserId" uuid not null constraint "FK_ChatUser_UserId" references "User"("Id") on delete cascade,
	"Rules" bigint default 0 not null,
	"JoinedAt" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false,
    constraint "PK_ChatUser_Id" primary key("ChatId", "UserId")
);

create table "Message"
(
	"Id" uuid constraint "PK_Message_Id" primary key default uuidv7(),
	"Text" text not null default '',
	"ChatId" uuid not null constraint "FK_Message_ChatId" references "Chat"("Id") on delete cascade,
	"UserId" uuid not null constraint "FK_Message_UserId" references "User"("Id") on delete cascade,
	"CountViews" int not null default 0,
	"SentAt" timestamptz not null default CURRENT_TIMESTAMP,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create index "IX_Message_LastMessages" on "Message"("ChatId", "SentAt") include ("Text");
create index "IX_Message_ChatId" on "Message"("ChatId");
create index "IX_Message_AuthorId" on "Message"("UserId");
create index "IX_Message_SentAt" on "Message"("SentAt");

create table "Attachment"
(
	"Id" uuid constraint "PK_Attachment_Id" primary key default uuidv7(),
	"MessageId"	uuid not null constraint "FK_Attachment_MessageId" references "Message"("Id") on delete cascade, 
	"FileMetadataId" uuid not null constraint "FK_Attachment_FileMetadataId" references "FileMetadata"("Id") on delete cascade,
	"OriginalFileName" text not null,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false
);

create table "ViewMessage"
(
	"UserId" uuid not null constraint "FK_ViewMessage_UserId" references "User"("Id") on delete cascade,
	"MessageId"	uuid not null constraint "FK_ViewMessage_MessageId" references "Message"("Id") on delete cascade, 
	"ViewTime" timestamptz not null default CURRENT_TIMESTAMP,
	"LastModified" timestamptz not null default CURRENT_TIMESTAMP,
	"IsDeleted" boolean not null default false,
	constraint "PK_ViewMessage_Id" primary key("MessageId", "UserId")
);

create index "IX_ViewMessage_MessageId" on "ViewMessage"("MessageId");
create index "IX_ViewMessage_UserId" on "ViewMessage"("UserId");

create or replace function "Fnc_ViewMessage_Count"()
returns trigger as
$$
begin
	update "Message" set "CountViews" = "CountViews" + 1 where "Id" = new."MessageId";
	
	return new;
end;
$$ language plpgsql;

create trigger "Trg_ViewMessage_After_Insert" after insert on "ViewMessage"
for each row
execute function "Fnc_ViewMessage_Count"();