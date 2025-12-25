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

    public virtual DbSet<ChatType> ChatTypes { get; set; }

    public virtual DbSet<ChatUser> ChatUsers { get; set; }

    public virtual DbSet<Extension> Extensions { get; set; }

    public virtual DbSet<FileMetadatum> FileMetadata { get; set; }

    public virtual DbSet<RoleType> RoleTypes { get; set; }

    public virtual DbSet<Sticker> Stickers { get; set; }

    public virtual DbSet<StickerPack> StickerPacks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAvatar> UserAvatars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Chat_Id");

            entity.ToTable("Chat");

            entity.HasIndex(e => e.UniqueName, "UQ__Chat__6C972DEE8CDAD8C9").IsUnique();

            entity.Property(e => e.ChatName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UniqueName).HasMaxLength(100);

            entity.HasOne(d => d.ChatType).WithMany(p => p.Chats)
                .HasForeignKey(d => d.ChatTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_ChatTypeId");

            entity.HasOne(d => d.Owner).WithMany(p => p.Chats)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("FK_Chat_OwnerId");
        });

        modelBuilder.Entity<ChatAvatar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatAvatar_Id");

            entity.ToTable("ChatAvatar");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatAvatars)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_ChatAvatar_ChatId");

            entity.HasOne(d => d.File).WithMany(p => p.ChatAvatars)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_ChatAvatar_FileId");
        });

        modelBuilder.Entity<ChatType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatType_Id");

            entity.ToTable("ChatType");

            entity.HasIndex(e => e.Title, "UQ__ChatType__2CB664DC40EFFCA0").IsUnique();

            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<ChatUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatUser_Id");

            entity.ToTable("ChatUser");

            entity.HasIndex(e => e.ChatId, "IX_ChatUser_ChatId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.UserId, e.ChatId }, "IX_ChatUser_UserChat").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.ChatId, e.UserId }, "UX_ChatUser_ChatId_UserId")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatUser_ChatId");

            entity.HasOne(d => d.RoleType).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.RoleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatUser_RoleTypeId");

            entity.HasOne(d => d.User).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatUser_UserId");
        });

        modelBuilder.Entity<Extension>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Extension_Id");

            entity.ToTable("Extension");

            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<FileMetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FileMetadata_Id");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Bucket).HasMaxLength(50);
            entity.Property(e => e.ObjectKey).HasMaxLength(500);

            entity.HasOne(d => d.Extension).WithMany(p => p.FileMetadata)
                .HasForeignKey(d => d.ExtensionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PK_FileMetadata_ExtensionId");
        });

        modelBuilder.Entity<RoleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RoleType_Id");

            entity.ToTable("RoleType");

            entity.HasIndex(e => e.Title, "UQ__RoleType__2CB664DC6F5C8DC3").IsUnique();

            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<Sticker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sticker_Id");

            entity.ToTable("Sticker");

            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.File).WithMany(p => p.Stickers)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_Sticker_FileId");

            entity.HasOne(d => d.StickerPack).WithMany(p => p.Stickers)
                .HasForeignKey(d => d.StickerPackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sticker_StickerPackId");
        });

        modelBuilder.Entity<StickerPack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_StickerPack_Id");

            entity.ToTable("StickerPack");

            entity.HasIndex(e => e.AuthorId, "IX_StickerPack_AuthorId").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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

            entity.Property(e => e.ForwardMessage).HasDefaultValue(true);
            entity.Property(e => e.InviteMe).HasDefaultValue(true);
            entity.Property(e => e.MonthsBeforeDelete).HasDefaultValue(6L);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SearchMe).HasDefaultValue(true);
            entity.Property(e => e.ShowMe).HasDefaultValue(true);
            entity.Property(e => e.WasOnline)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<UserAvatar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UserAvatar_Id");

            entity.ToTable("UserAvatar");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

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
