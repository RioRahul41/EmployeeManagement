using CodexaManagement.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Data.DbContext
{
    public class ApplicationDbContext :
        IdentityDbContext<IdentityApplication>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Project> Projects { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
   .HasOne(e => e.IdentityUser)
   .WithOne()
   .HasForeignKey<Employee>(e => e.IdentityUserId)
   .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Employee>()
      .Property(e => e.Salary)
      .HasPrecision(18, 2);
            modelBuilder.Entity<Employee>()
    .Property(e => e.EmpIsActive)
    .HasDefaultValue(false);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(request => request.Employee)
                .WithMany()
                .HasForeignKey(request => request.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasIndex(request => new { request.EmployeeId, request.FromDate, request.ToDate });
        }
    }
}

