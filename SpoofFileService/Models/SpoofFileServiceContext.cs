using Microsoft.EntityFrameworkCore;

namespace SpoofFileService.Models;

public partial class SpoofFileServiceContext : DbContext
{
    public SpoofFileServiceContext()
    {
    }

    public SpoofFileServiceContext(DbContextOptions<SpoofFileServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Extension> Extensions { get; set; }

    public virtual DbSet<FileObject> FileObjects { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExtensionDto>().HasNoKey();
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Category_Id");

            entity.ToTable("Category");

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Extension>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Extension_Id");

            entity.ToTable("Extension");

            entity.HasIndex(e => e.Name, "IX_Extension_Name").HasFilter("(NOT \"IsDeleted\")");

            entity.HasIndex(e => e.Name, "UQ_Extension_Name").IsUnique();

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasMaxLength(20);

            entity.HasOne(d => d.Category).WithMany(p => p.Extensions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Extension_CategoryId");
        });

        modelBuilder.Entity<FileObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FileObject_Id");

            entity.ToTable("FileObject");

            entity.HasIndex(e => new { e.L3, e.Size, e.ExtensionId }, "IX_FileObject_Check_L3").IsUnique();

            entity.HasIndex(e => new { e.L1, e.Size, e.ExtensionId }, "IX_FileObject_Fast_Check_L1");

            entity.HasIndex(e => new { e.L2, e.Size, e.ExtensionId }, "IX_FileObject_Fast_Check_L2");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            entity.HasOne(d => d.Extension).WithMany(p => p.FileObjects)
                .HasForeignKey(d => d.ExtensionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileObject_ExtensionId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
