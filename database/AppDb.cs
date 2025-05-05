using Microsoft.EntityFrameworkCore;

class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options)
        : base(options) {}

    public DbSet<Person> People => Set<Person>();
    public DbSet<Car> Cars => Set<Car>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasOne(c => c.Owner)
            .WithMany(p => p.Cars)
            .HasForeignKey(c => c.PersonId);
            
        base.OnModelCreating(modelBuilder);
    }
}
