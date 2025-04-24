using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CarDb>(opt => opt.UseInMemoryDatabase("CarList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/cars", async (CarDb db) =>
    await db.Cars.ToListAsync());