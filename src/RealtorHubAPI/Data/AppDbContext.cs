﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System.Data;
using RealtorHubAPI.Entities.Identity;
using RealtorHubAPI.Entities;

namespace RealtorHubAPI.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {
        }

        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        //public DbSet<Realtor> Realtors { get; set; }
        public DbSet<Land> Lands { get; set; }
        public DbSet<LandImage> LandImages { get; set; }
        public DbSet<LandVideo> LandVideos { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            
            //modelBuilder.HasSequence<int>("AccountNumberSeq", schema: "public")
            //    .StartsAt(2000753554)
            //    .IncrementsBy(1);
        }


        public async Task<bool> TrySaveChangesAsync()
        {
            try
            {
                await SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? string.Empty;
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}