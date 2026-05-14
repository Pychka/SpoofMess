using CommunicationLibrary.Communication;
using Microsoft.EntityFrameworkCore;

namespace SpoofSettingsService.Models;

public partial class SpoofSettingsServiceContext : DbContext
{
    public SpoofSettingsServiceContext()
    {
    }

    public SpoofSettingsServiceContext(DbContextOptions<SpoofSettingsServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<ChatAvatar> ChatAvatars { get; set; }

    public virtual DbSet<ChatProperty> ChatProperties { get; set; }

    public virtual DbSet<ChatRole> ChatRoles { get; set; }

    public virtual DbSet<ChatRoleRule> ChatRoleRules { get; set; }

    public virtual DbSet<ChatType> ChatTypes { get; set; }

    public virtual DbSet<ChatTypeChatProperty> ChatTypeChatProperties { get; set; }

    public virtual DbSet<ChatUser> ChatUsers { get; set; }

    public virtual DbSet<ChatUserChatRole> ChatUserChatRoles { get; set; }

    public virtual DbSet<ChatUserOutbox> ChatUserOutboxes { get; set; }

    public virtual DbSet<ChatUserRule> ChatUserRules { get; set; }

    public virtual DbSet<FileMetadatum> FileMetadata { get; set; }

    public virtual DbSet<GlobalPermission> GlobalPermissions { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RoleRank> RoleRanks { get; set; }

    public virtual DbSet<Sticker> Stickers { get; set; }

    public virtual DbSet<StickerPack> StickerPacks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAvatar> UserAvatars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.HasPostgresEnum<OutboxStatus>(name: "OutboxStatus");

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Chat_Id");

            entity.ToTable("Chat");

            entity.HasIndex(e => e.UniqueName, "Chat_ChatUniqueName_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UniqueName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ChatType).WithMany(p => p.Chats)
                .HasForeignKey(d => d.ChatTypeId)
                .HasConstraintName("FK_Chat_ChatTypeId");

            entity.HasOne(d => d.Owner).WithMany(p => p.Chats)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Chat_OwnerId");
        });

        modelBuilder.Entity<ChatAvatar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatAvatar_Id");

            entity.ToTable("ChatAvatar");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatAvatars)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_ChatAvatar_ChatId");

            entity.HasOne(d => d.File).WithMany(p => p.ChatAvatars)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_ChatAvatar_FileId");
        });
        modelBuilder.Entity<ChatProperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatProperty_Id");

            entity.ToTable("ChatProperty");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ChatRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatRole_Id");

            entity.ToTable("ChatRole");

            entity.HasIndex(e => new { e.ChatId, e.Name }, "UQ_ChatRole_Chat_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatRoles)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_ChatRole_ChatId");

            entity.HasOne(d => d.RoleRank).WithMany(p => p.ChatRoles)
                .HasForeignKey(d => d.RoleRankId)
                .HasConstraintName("FK_ChatRole_RoleRankId");
        });

        modelBuilder.Entity<ChatRoleRule>(entity =>
        {
            entity.HasKey(e => new { e.Key1, e.Key2 }).HasName("PK_ChatRoleRules_Id");

            entity.Property(e => e.IsPermission).HasDefaultValue(true);

            entity.HasOne(d => d.ChatRole).WithMany(p => p.ChatRoleRules)
                .HasForeignKey(d => d.Key1)
                .HasConstraintName("FK_ChatRoleRules_ChatRoleId");

            entity.HasOne(d => d.Permission).WithMany(p => p.ChatRoleRules)
                .HasForeignKey(d => d.Key2)
                .HasConstraintName("FK_ChatRoleRules_PermissionId");
        });

        modelBuilder.Entity<ChatType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatType_Id");

            entity.ToTable("ChatType");

            entity.HasIndex(e => e.Name, "ChatType_Name_key").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ChatTypeChatProperty>(entity =>
        {
            entity.HasKey(e => new { e.ChatTypeId, e.ChatPropertyId }).HasName("PK_ChatTypeChatProperty_Id");

            entity.ToTable("ChatTypeChatProperty");

            entity.HasOne(d => d.ChatProperty).WithMany(p => p.ChatTypeChatProperties)
                .HasForeignKey(d => d.ChatPropertyId)
                .HasConstraintName("FK_ChatTypeChatProperty_ChatPropertyId");

            entity.HasOne(d => d.ChatType).WithMany(p => p.ChatTypeChatProperties)
                .HasForeignKey(d => d.ChatTypeId)
                .HasConstraintName("FK_ChatTypeChatProperty_ChatTypeId");
        });

        modelBuilder.Entity<ChatUser>(entity =>
        {
            entity.HasKey(e => new { e.Key1, e.Key2 }).HasName("PK_ChatUser_Id");

            entity.ToTable("ChatUser");

            entity.HasIndex(e => e.Key1, "IX_ChatUser_ChatId").HasFilter("(\"IsDeleted\" = false)");

            entity.HasIndex(e => new { e.Key2, e.Key1 }, "IX_ChatUser_UserChat").HasFilter("(\"IsDeleted\" = false)");

            entity.HasIndex(e => new { e.Key1, e.Key2 }, "UX_ChatUser_ChatId_UserId")
                .IsUnique()
                .HasFilter("(\"IsDeleted\" = false)");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.Key1)
                .HasConstraintName("FK_ChatUser_ChatId");

            entity.HasOne(d => d.User).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.Key2)
                .HasConstraintName("FK_ChatUser_UserId");
        });

        modelBuilder.Entity<ChatUserChatRole>(entity =>
        {
            entity.HasKey(e => new { e.Key1, e.Key2, e.ChatRoleId }).HasName("PK_ChatUserChatRole_Id");

            entity.ToTable("ChatUserChatRole");

            entity.Property(e => e.TimeSet).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ChatRole).WithMany(p => p.ChatUserChatRoles)
                .HasForeignKey(d => d.ChatRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatUserChatRole_ChatRoleId");

            entity.HasOne(d => d.ChatUser).WithMany(p => p.ChatUserChatRoles)
                .HasForeignKey(d => new { d.Key1, d.Key2 })
                .HasConstraintName("FK_ChatUserChatRole_ChatUserId");
        });

        modelBuilder.Entity<ChatUserOutbox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatUserOutbox_Id");

            entity.ToTable("ChatUserOutbox");

            entity.Property(e => e.Status)
                .HasColumnType("OutboxStatus");

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Data).HasColumnType("jsonb");
            entity.Property(e => e.LastTryDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ChatUser).WithMany(p => p.ChatUserOutboxes)
                .HasForeignKey(d => new { d.ChatId, d.UserId })
                .HasConstraintName("FK_ChatUserOutbox_ChatUserId");
        });

        modelBuilder.Entity<ChatUserRule>(entity =>
        {
            entity.HasKey(e => new { e.Key1, e.Key2, e.PermissionId }).HasName("PK_ChatUserPermission_Id");

            entity.Property(e => e.IsPermission).HasDefaultValue(true);

            entity.HasOne(d => d.Permission).WithMany(p => p.ChatUserRules)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatUserPermission_PermissionId");

            entity.HasOne(d => d.ChatUser).WithMany(p => p.ChatUserRules)
                .HasForeignKey(d => new { d.Key1, d.Key2 })
                .HasConstraintName("FK_ChatUserPermission_ChatUserId");
        });

        modelBuilder.Entity<FileMetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FileMetadata_Id");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(20);
        });

        modelBuilder.Entity<GlobalPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_GlobalPermission_Id");

            entity.ToTable("GlobalPermission");

            entity.HasIndex(e => e.Name, "GlobalPermission_Name_key").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Users).WithMany(p => p.GlobalPermissions)
                .UsingEntity<Dictionary<string, object>>(
                    "UserGlobalPermission",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserGlobalPermission_UserId"),
                    l => l.HasOne<GlobalPermission>().WithMany()
                        .HasForeignKey("GlobalPermissionId")
                        .HasConstraintName("FK_UserGlobalPermission_ChatId"),
                    j =>
                    {
                        j.HasKey("GlobalPermissionId", "UserId").HasName("PK_UserGlobalPermission_Id");
                        j.ToTable("UserGlobalPermission");
                    });
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Permission_Id");

            entity.ToTable("Permission");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<RoleRank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RoleRank_Id");

            entity.ToTable("RoleRank");

            entity.HasIndex(e => new { e.ChatId, e.Name }, "RoleRank_ChatId_Name_key").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Chat).WithMany(p => p.RoleRanks)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_RoleRank_ChatId");
        });

        modelBuilder.Entity<Sticker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sticker_Id");

            entity.ToTable("Sticker");

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.File).WithMany(p => p.Stickers)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_Sticker_FileId");

            entity.HasOne(d => d.StickerPack).WithMany(p => p.Stickers)
                .HasForeignKey(d => d.StickerPackId)
                .HasConstraintName("FK_Sticker_StickerPackId");
        });

        modelBuilder.Entity<StickerPack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_StickerPack_Id");

            entity.ToTable("StickerPack");

            entity.HasIndex(e => e.AuthorId, "IX_StickerPack_AuthorId").HasFilter("(\"IsDeleted\" = false)");

            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Author).WithMany(p => p.StickerPacks)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_StickerPack_AuthorId");

            entity.HasOne(d => d.Preview).WithMany(p => p.StickerPacks)
                .HasForeignKey(d => d.PreviewId)
                .HasConstraintName("FK_StickerPack_PreviewId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User_Id");

            entity.ToTable("User");

            entity.HasIndex(e => e.Login, "User_Login_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ForwardMessage).HasDefaultValue(true);
            entity.Property(e => e.InviteMe).HasDefaultValue(true);
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.MonthsBeforeDelete).HasDefaultValue(6);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SearchMe).HasDefaultValue(true);
            entity.Property(e => e.ShowMe).HasDefaultValue(true);
            entity.Property(e => e.WasOnline).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserAvatar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UserAvatar_Id");

            entity.ToTable("UserAvatar");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.File).WithMany(p => p.UserAvatars)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_UserAvatar_FileId");

            entity.HasOne(d => d.User).WithMany(p => p.UserAvatars)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserAvatar_UserId");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}