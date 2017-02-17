using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GradDisplayMain.Models
{
    public class Teleprompt
    {
        [Required]
        [Key]
        public string GraduateId { get; set; }

        public DateTime Created { get; set; }

    }

    public class TelepromptDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public TelepromptDbContext(DbContextOptions<TelepromptDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        public DbSet<Teleprompt> Teleprompt { get; set; }
    }

}
