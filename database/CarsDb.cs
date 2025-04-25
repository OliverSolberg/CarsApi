using Microsoft.EntityFrameworkCore;

class CarDb : DbContext
{
    public CarDb(DbContextOptions<CarDb> options)
        : base(options) {}

    public DbSet<Car> Cars => Set<Car>();
}