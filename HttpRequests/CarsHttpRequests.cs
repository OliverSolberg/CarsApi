using Microsoft.EntityFrameworkCore;
public static class CarsEndpoints
{
    public static void MapCarsEndpoints(this WebApplication app)
    {
        var cars = app.MapGroup("/cars");

        cars.MapGet("/", async (AppDb db) =>
            await db.Cars.ToListAsync());
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
    }
}