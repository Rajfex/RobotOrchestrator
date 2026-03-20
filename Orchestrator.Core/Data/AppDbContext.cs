using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Core.Models;

namespace Orchestrator.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Log> Logs { get; set; }
        public DbSet<Models.LogType> LogTypes { get; set; }
        public DbSet<Models.Robot> Robots { get; set; }
        public DbSet<Models.RobotStatus> RobotStatuses { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Models.TaskStatus> TaskStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogType>().HasData(
                new LogType { Id = 1, Name = "Info" },
                new LogType { Id = 2, Name = "Warning" },
                new LogType { Id = 3, Name = "Error" },
                new LogType { Id = 4, Name = "Debug" }
            );

            modelBuilder.Entity<RobotStatus>().HasData(
                new RobotStatus { Id = 1, Name = "Available" },
                new RobotStatus { Id = 2, Name = "Busy" },
                new RobotStatus { Id = 3, Name = "Offline" },
                new RobotStatus { Id = 4, Name = "Maintenance" }
            );

            modelBuilder.Entity<Models.TaskStatus>().HasData(
                new Models.TaskStatus { Id = 1, Name = "New" },
                new Models.TaskStatus { Id = 2, Name = "In Progress" },
                new Models.TaskStatus { Id = 3, Name = "Completed" },
                new Models.TaskStatus { Id = 4, Name = "Failed" },
                new Models.TaskStatus { Id = 5, Name = "Canceled" }
            );
        }

    }
}
