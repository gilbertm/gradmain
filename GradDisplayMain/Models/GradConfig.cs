using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradDisplayMain.Models
{
    public class GradConfig
    {
        
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Value {get; set; }
    }

    public class GradConfigDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public GradConfigDbContext(DbContextOptions<GradConfigDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        public virtual DbSet<GradConfig> GradConfig { get; set; }
    }
}
