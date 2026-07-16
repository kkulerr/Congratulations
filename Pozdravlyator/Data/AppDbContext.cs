using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Pozdravlyator.Models;

namespace Pozdravlyator.Data
{
    /// <summary>
    /// Контекст базы данных на основе SQLite.
    /// Файл базы данных создаётся рядом с исполняемым файлом приложения.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<BirthdayPerson> People => Set<BirthdayPerson>();

        private readonly string _dbPath;

        public AppDbContext()
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            _dbPath = Path.Combine(folder, "birthdays.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BirthdayPerson>(entity =>
            {
                entity.ToTable("People");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.BirthDate).IsRequired();
                entity.Property(p => p.PhoneNumber).HasMaxLength(50);
                entity.Property(p => p.Notes).HasMaxLength(500);
            });
        }
    }
}
