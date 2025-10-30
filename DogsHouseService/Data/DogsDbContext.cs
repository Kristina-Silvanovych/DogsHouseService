using Microsoft.EntityFrameworkCore;

namespace DogsHouseService.Data
{
    public class DogsDbContext : DbContext
    {
        public DogsDbContext(DbContextOptions<DogsDbContext> options) : base(options) { }

        public DbSet<Dog> Dogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Dog>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<Dog>().HasData(
                new Dog { Id = 1, Name = "Neo", Color = "red & amber", TailLength = 22, Weight = 32 },
                new Dog { Id = 2, Name = "Jessy", Color = "black & white", TailLength = 7, Weight = 14 }
            );
        }
    }
}
