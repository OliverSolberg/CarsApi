using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CarDb>(opt => opt.UseInMemoryDatabase("CarList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "CarAPI";
    config.Title = "CarAPI v1";
    config.Version = "v1";
});

var app = builder.Build();
if(app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "CarAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/cars", async (CarDb db) =>
    await db.Cars.ToListAsync());

app.MapGet("/cars/make", async (string make, CarDb db) =>
    await db.Cars.Where(c => c.Make == make).ToListAsync());

app.MapGet("/cars/registered", async (CarDb db) =>
    await db.Cars.Where(t => t.IsRegistered).ToListAsync());

app.MapGet("/cars/unregistered", async (CarDb db) =>
    await db.Cars.Where(t => !t.IsRegistered).ToListAsync());

app.MapGet("/cars/{id}", async (int id, CarDb db) =>
    await db.Cars.FindAsync(id)
        is Car car
            ? Results.Ok(car)
            : Results.NotFound());

app.MapPost("/cars", async (Car car, CarDb db) =>
{
    db.Cars.Add(car);
    await db.SaveChangesAsync();

    return Results.Created($"/cars/{car.Id}", car);
});

app.MapPut("/car/{id}", async (int id, Car inputCar, CarDb db) =>
{
    var car = await db.Cars.FindAsync(id);

    if(car is null) return Results.NotFound();

    car.IsRegistered = inputCar.IsRegistered;
    car.Make = inputCar.Make;
    car.Model = inputCar.Model;
    car.BuildYear = inputCar.BuildYear;
    car.Owner = inputCar.Owner;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/cars/{id}", async (int id, CarDb db) =>
{
    if (await db.Cars.FindAsync(id) is Car car)
    {
        db.Cars.Remove(car);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
    });

    app.Run();