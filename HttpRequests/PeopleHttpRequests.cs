using Microsoft.EntityFrameworkCore;
public static class PeopleEndpoints
{
    public static void MapPeopleEndpoints(this WebApplication app)
    {
        var people = app.MapGroup("/people");

        people.MapGet("/", async (AppDb db) =>
            await db.People.Include(p => p.Cars).ToListAsync());

        people.MapGet("/{id}", async (int id, AppDb db) =>
        {
            var person = await db.People
                .Include(p => p.Cars) 
                .FirstOrDefaultAsync(p => p.Id == id);

            return person is not null 
                ? Results.Ok(person) 
                : Results.NotFound();
        });
        people.MapPost("/", async ( Person person, AppDb db) =>
        {
            db.People.Add(person);
            await db.SaveChangesAsync();

            return Results.Created($"/{person.Id}", person);
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
        people.MapPut("/{id}", async (int id, Person inputPerson, AppDb db) =>
        {
            var person = await db.People.FindAsync(id);

            if(person is null) return Results.NotFound();

            person.Name = inputPerson.Name;
            person.Birthday = inputPerson.Birthday;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}