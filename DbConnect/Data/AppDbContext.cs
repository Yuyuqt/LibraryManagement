using System;
using System.Collections.Generic;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbConnect.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Borrowing> Borrowings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

    public virtual DbSet<WishlistItem> WishlistItems { get; set; }
    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Books__3214EC07AE1974C8");

            entity.HasIndex(e => e.Isbn, "UQ__Books__447D36EA79CF66CE").IsUnique();

            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.AvailableCopies).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TotalCopies).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp");

            entity.HasMany(d => d.Categories).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_BookCategories_Categories"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .HasConstraintName("FK_BookCategories_Books"),
                    j =>
                    {
                        j.HasKey("BookId", "CategoryId").HasName("PK__BookCate__9C7051A74E7D221D");
                        j.ToTable("BookCategories");
                    });
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BorrowDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.DueDate).HasColumnType("timestamp");
            entity.Property(e => e.FineAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReturnDate).HasColumnType("timestamp");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Book).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrowings_Books");

            entity.HasOne(d => d.User).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrowings_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC0751B5797D");

            entity.HasIndex(e => e.Name, "UQ__Categori__737584F622B16D3A").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Membersh__3214EC07A6F5A55F");

            entity.HasIndex(e => e.Type, "IX_Memberships_Type").IsUnique();

            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RewardId).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.ActionLink).HasMaxLength(500);
            entity.Property(e => e.ActionText).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("Info");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FcmToken).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
            entity.Property(e => e.StudentId).HasMaxLength(20);
            entity.Property(e => e.SuspensionEndDate).HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp");
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)").HasDefaultValue(0);
        });


        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExpiryDate).HasColumnType("timestamp");
            entity.Property(e => e.ExternalRedemptionId).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartDate).HasColumnType("timestamp");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Membership).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.MembershipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSubscriptions_Memberships");

            entity.HasOne(d => d.User).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSubscriptions_Users");
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasIndex(e => e.BookId, "IX_WishlistItems_BookId");

            entity.HasIndex(e => e.UserId, "IX_WishlistItems_UserId");

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Book).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_WishlistItems_Books");

            entity.HasOne(d => d.User).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_WishlistItems_Users");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("now()");

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
