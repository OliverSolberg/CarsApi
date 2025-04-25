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

var cars = app.MapGroup("/cars");
var people = app.MapGroup("/people");

cars.MapGet("/", async (CarDb db) =>
    await db.Cars.ToListAsync());
people.MapGet("/", async (PeopleDb db) =>
    await db.People.ToListAsync());

cars.MapGet("/{make}", async (string make, CarDb db) =>
    await db.Cars.Where(c => c.Make == make).ToListAsync());

cars.MapGet("/registered", async (CarDb db) =>
    await db.Cars.Where(t => t.IsRegistered).ToListAsync());

cars.MapGet("/unregistered", async (CarDb db) =>
    await db.Cars.Where(t => !t.IsRegistered).ToListAsync());

cars.MapGet("/{id}", async (int id, CarDb db) =>
    await db.Cars.FindAsync(id)
        is Car car
            ? Results.Ok(car)
            : Results.NotFound());
people.MapGet("/{id}", async (int id, PeopleDb db) =>
    await db.People.FindAsync(id)
        is Person person
            ? Results.Ok(person)
            : Results.NotFound());

cars.MapPost("/", async (Car car, CarDb db) =>
{
    db.Cars.Add(car);
    await db.SaveChangesAsync();

    return Results.Created($"/{car.Id}", car);
});
people.MapPost("/", async ( Person person, PeopleDb db) =>
{
    db.People.Add(person);
    await db.SaveChangesAsync();

    return Results.Created($"/{person.Id}", person);
});

cars.MapPut("/{id}", async (int id, Car inputCar, CarDb db) =>
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

people.MapPut("/{id}", async (int id, Person inputPerson, PeopleDb db) =>
{
    var person = await db.People.FindAsync(id);

    if(person is null) return Results.NotFound();

    person.Name = inputPerson.Name;
    person.Birthday = inputPerson.Birthday;
    person.Car = inputPerson.Car;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

cars.MapDelete("/{id}", async (int id, CarDb db) =>
{
    if (await db.Cars.FindAsync(id) is Car car)
    {
        db.Cars.Remove(car);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
    });
people.MapDelete("/{id}", async (int id, PeopleDb db) =>
{
    if(await db.People.FindAsync(id) is Person person)
    {
        db.People.Remove(person);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

    app.Run();