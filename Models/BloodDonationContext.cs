using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Blood_Donations_Project.Models;

public partial class BloodDonationContext : DbContext
{
    public BloodDonationContext()
    {
    }

    public BloodDonationContext(DbContextOptions<BloodDonationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BloodRequest> BloodRequests { get; set; }

    public virtual DbSet<BloodType> BloodTypes { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Donor> Donors { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<DonationRequest> DonationRequests { get; set; }

    public DbSet<PasswordReset> PasswordReset { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BloodRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BloodReq__3214EC074C86DB03");

            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<BloodType>(entity =>
        {
            entity.HasKey(e => e.BloodTypeId).HasName("PK__BloodTyp__B489BA63CA208358");

            //entity.Property(e => e.BloodTypeId).ValueGeneratedNever();
            entity.Property(e => e.TypeName).HasMaxLength(5);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC07F18D4E4C");

            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.DonationApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_Donations_approvedby");

            entity.HasOne(d => d.User).WithMany(p => p.DonationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_Users");
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donors__3214EC073ED99504");

            //entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.BloodType).WithMany(p => p.Donors)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK_Donors_BloodTypes");

            entity.HasOne(d => d.User).WithMany(p => p.Donors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Donors_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AC7A9A6E0");

            //entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CE54B5781");

            //entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.MobileNo).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__3B75D760");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A35807D8263");

            entity.ToTable("UserRole");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRole_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRole_Users");
        });

        modelBuilder.Entity<DonationRequest>(entity =>
        {
            entity.ToTable("DonationRequests");

            entity.HasKey(e => e.Id).HasName("PK_DonationRequests");

            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonationRequests_Users");

            entity.HasOne(d => d.ApprovedByNavigation)
                .WithMany() 
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_DonationRequests_ApprovedBy");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
