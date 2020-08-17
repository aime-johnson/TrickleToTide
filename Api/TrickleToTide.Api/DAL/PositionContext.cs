using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrickleToTide.Api.DAL
{
    class PositionContext : DbContext
    {
        public PositionContext(DbContextOptions<PositionContext> options)
            : base(options)
        {
        }

        public DbSet<Position> Positions { get; set; }
        public DbSet<PositionHistory> History { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("ttt");
            modelBuilder.Entity<Position>().ToTable("Position");
            modelBuilder.Entity<Position>().HasMany(f => f.History).WithOne(f => f.Position).HasForeignKey(f => f.PositionId);
            modelBuilder.Entity<Position>().Property(f => f.Id).ValueGeneratedNever();
            modelBuilder.Entity<PositionHistory>().ToTable("PositionHistory");

            base.OnModelCreating(modelBuilder);
        }
    }
}
