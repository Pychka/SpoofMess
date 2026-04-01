using Microsoft.EntityFrameworkCore;

namespace SpoofMessageService.Models;

public partial class SpoofMessageServiceContext : DbContext
{
    public SpoofMessageServiceContext()
    {
    }

    public SpoofMessageServiceContext(DbContextOptions<SpoofMessageServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<ChatUser> ChatUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<FileMetadatum> FileMetadata { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<ViewMessage> ViewMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Attachment_Id");

            entity.ToTable("Attachment");

            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.FileMetadata).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.FileMetadataId)
                .HasConstraintName("FK_Attachment_FileMetadataId");

            entity.HasOne(d => d.Message).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_Attachment_MessageId");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Chat_Id");

            entity.ToTable("Chat");

            entity.HasIndex(e => e.UniqueName, "Chat_UniqueName_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UniqueName).HasMaxLength(100);

            entity.HasOne(d => d.Avatar).WithMany(p => p.Chats)
                .HasForeignKey(d => d.AvatarId)
                .HasConstraintName("FK_Chat_AvatarId");
        });

        modelBuilder.Entity<ChatUser>(entity =>
        {
            entity.HasKey(e => new { e.Key1, e.Key2 }).HasName("PK_ChatUser_Id");

            entity.ToTable("ChatUser");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.Key1)
                .HasConstraintName("FK_ChatUser_ChatId");

            entity.HasOne(d => d.User).WithMany(p => p.ChatUsers)
                .HasForeignKey(d => d.Key2)
                .HasConstraintName("FK_ChatUser_UserId");
        });

        modelBuilder.Entity<FileMetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FileMetadata_Id");

            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(20);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Message_Id");

            entity.ToTable("Message");

            entity.HasIndex(e => e.UserId, "IX_Message_AuthorId");

            entity.HasIndex(e => e.ChatId, "IX_Message_ChatId");

            entity.HasIndex(e => new { e.ChatId, e.SentAt }, "IX_Message_LastMessages");

            entity.HasIndex(e => e.SentAt, "IX_Message_SentAt");

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Text).HasDefaultValueSql("''::text");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_Message_ChatId");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Message_UserId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User_Id");

            entity.ToTable("User");

            entity.HasIndex(e => e.Login, "User_Login_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuidv7()");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Avatar).WithMany(p => p.Users)
                .HasForeignKey(d => d.AvatarId)
                .HasConstraintName("FK_User_AvatarId");
        });

        modelBuilder.Entity<ViewMessage>(entity =>
        {
            entity.HasKey(e => new { e.Key2, e.Key1 }).HasName("PK_ViewMessage_Id");

            entity.ToTable("ViewMessage");

            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ViewTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Message).WithMany(p => p.ViewMessages)
                .HasForeignKey(d => d.Key2)
                .HasConstraintName("FK_ViewMessage_MessageId");

            entity.HasOne(d => d.User).WithMany(p => p.ViewMessages)
                .HasForeignKey(d => d.Key1)
                .HasConstraintName("FK_ViewMessage_UserId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
