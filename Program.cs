using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddDbContext<AppDb>(opt => opt.UseInMemoryDatabase("Lists"));
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

cars.MapGet("/", async (AppDb db) =>
    await db.Cars.ToListAsync());
people.MapGet("/", async (AppDb db) =>
    await db.People.Include(p => p.Cars).ToListAsync());

cars.MapGet("/brand/{make}", async (string make, AppDb db) =>
    await db.Cars.Where(c => c.Make == make).ToListAsync());

cars.MapGet("/brand", async (AppDb db) =>
    await db.Cars.OrderBy(c => c.Make).ToListAsync());

cars.MapGet("/registered", async (AppDb db) =>
    await db.Cars.Where(t => t.IsRegistered).ToListAsync());

cars.MapGet("/unregistered", async (AppDb db) =>
    await db.Cars.Where(t => !t.IsRegistered).ToListAsync());

cars.MapGet("/{id}", async (int id, AppDb db) =>
    await db.Cars.FindAsync(id)
        is Car car
            ? Results.Ok(car)
            : Results.NotFound());

people.MapGet("/{id}", async (int id, AppDb db) =>
{
    var person = await db.People
        .Include(p => p.Cars) // 👈 this is important
        .FirstOrDefaultAsync(p => p.Id == id);

    return person is not null 
        ? Results.Ok(person) 
        : Results.NotFound();
});


cars.MapPost("/", async (Car car, AppDb db) =>
{
    var owner = await db.People.FindAsync(car.PersonId);

    if(owner != null)
    {
        car.Owner = owner;
        if(owner.Cars == null)
        {
            owner.Cars = new List<Car>();
        }
        owner.Cars.Add(car);
    }
    else
    {
        return Results.NotFound();
    }
    

    db.Cars.Add(car);
    await db.SaveChangesAsync();
    return Results.Created($"/{car.Id}", car);
});
people.MapPost("/", async ( Person person, AppDb db) =>
{
    db.People.Add(person);
    await db.SaveChangesAsync();

    return Results.Created($"/{person.Id}", person);
});

cars.MapPut("/{id}", async (int id, Car inputCar, AppDb db) =>
{
    var car = await db.Cars.FindAsync(id);

    if(car is null) return Results.NotFound();

    var newOwner = await db.People.FindAsync(inputCar.PersonId);
    var currentOwner = await db.People.FindAsync(car.PersonId);
    if(newOwner != currentOwner)
    {
        if(currentOwner != null && currentOwner.Cars != null)
        {
            currentOwner.Cars.Remove(car);
        }

        if(newOwner != null)
        {
            if(newOwner.Cars == null)
            {
                newOwner.Cars = new List<Car>();
            }

            newOwner.Cars.Add(car);
        }
        car.Owner = inputCar.Owner;
    }
    car.IsRegistered = inputCar.IsRegistered;
    car.Make = inputCar.Make;
    car.Model = inputCar.Model;
    car.BuildYear = inputCar.BuildYear;
    car.PersonId = inputCar.PersonId;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

people.MapPut("/{id}", async (int id, Person inputPerson, AppDb db) =>
{
    var person = await db.People.FindAsync(id);

    if(person is null) return Results.NotFound();

    person.Name = inputPerson.Name;
    person.Birthday = inputPerson.Birthday;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

cars.MapDelete("/{id}", async (int id, AppDb db) =>
{
    if (await db.Cars.FindAsync(id) is Car car)
    {
        db.Cars.Remove(car);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
    });
people.MapDelete("/{id}", async (int id, AppDb db) =>
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