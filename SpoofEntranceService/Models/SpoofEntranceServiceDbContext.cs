using Microsoft.EntityFrameworkCore;

namespace SpoofEntranceService.Models;

public partial class SpoofEntranceServiceDbContext : DbContext
{
    public SpoofEntranceServiceDbContext()
    {
    }

    public SpoofEntranceServiceDbContext(DbContextOptions<SpoofEntranceServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SessionInfo> SessionInfos { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<UserEntry> UserEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SessionInfo_Id");

            entity.ToTable("SessionInfo");

            entity.HasIndex(e => e.Id, "IX_SessionInfo_SessionId");

            entity.HasIndex(e => e.UserEntryId, "IX_SessionInfo_UserId_Active").HasFilter("([IsActive]=(1) AND [IsDeleted]=(0))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeviceId).HasMaxLength(100);
            entity.Property(e => e.DeviceName).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastActivityAt).HasColumnType("datetime");
            entity.Property(e => e.Platform).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.UserEntry).WithMany(p => p.SessionInfos)
                .HasForeignKey(d => d.UserEntryId)
                .HasConstraintName("FK_SessionInfo_UserEntryId");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenHash).HasName("PK_Token_Id");

            entity.ToTable("Token");

            entity.HasIndex(e => e.SessionInfoId, "IX_Token_SessionInfoId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.RefreshTokenHash, "IX_Token_ValidTo").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.RefreshTokenHash).HasMaxLength(100);
            entity.Property(e => e.ValidTo).HasColumnType("datetime");

            entity.HasOne(d => d.SessionInfo).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.SessionInfoId)
                .HasConstraintName("FK_Token_SessionInfoId");
        });

        modelBuilder.Entity<UserEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User_Id");

            entity.ToTable("UserEntry");

            entity.HasIndex(e => e.UniqueName, "UQ__UserEntr__6C972DEE78064811").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
            entity.Property(e => e.UniqueName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
