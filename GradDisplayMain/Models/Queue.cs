using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GradDisplayMain.Models
{
    public class Queue
    {
        [Required]
        [Key]
        public string GraduateId { get; set; }

        public DateTime Created { get; set; }

    }

    public class QueueDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        public DbSet<Queue> Queue { get; set; }
    }

}
