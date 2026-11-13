using System.Text.Json;
using LearnToDev.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Data;

// Fills the database with game content the first time the app starts
public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger, string seedFolder)
    {
        // 1. Do nothing if the database already has content
        if (await db.Schools.AnyAsync())
        {
            logger.LogInformation("Database already has content, skipping seed.");
            return;
        }

        // 2. Schools, questions and answer options from the JSON files (one file per school)
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        foreach (var file in Directory.GetFiles(seedFolder, "*.json"))
        {
            try
            {
                await using var stream = File.OpenRead(file);
                var school = await JsonSerializer.DeserializeAsync<School>(stream, options);

                if (school == null)
                {
                    logger.LogWarning("Seed file {File} was empty and was skipped", file);
                    continue;
                }

                db.Schools.Add(school);
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                logger.LogError(ex, "Could not read seed file {File}.", file);
            }
        }
        await db.SaveChangesAsync();

        // 3. Jobs and the schools they require
        var frontend = await db.Schools.SingleAsync(s => s.Name == "Frontend");
        var backend = await db.Schools.SingleAsync(s => s.Name == "Backend");
        var database = await db.Schools.SingleAsync(s => s.Name == "Database");

        db.Jobs.AddRange(
            NewJob("IT Help", "Hjelper kolleger med datamaskiner. Krever ingen eksamen."),
            NewJob("Frontend-utvikler", "Lager nettsider og brukergrensesnitt.", frontend),
            NewJob("Backend-utvikler", "Lager API-er og logikk på serveren.", backend),
            NewJob("Databaseutvikler", "Designer tabeller og skriver SQL.", database),
            NewJob("Fullstack-utvikler", "Jobber med hele løsningen, fra database til nettleser.", frontend, backend, database)
        );
        await db.SaveChangesAsync();

        logger.LogInformation("Database seeded with game content.");
    }

    private static Job NewJob(string name, string description, params School[] requiredSchools)
    {
        var job = new Job { Name = name, Description = description };
        foreach (var school in requiredSchools)
        {
            job.JobRequirements.Add(new JobRequirement { School = school });
        }
        return job;
    }
}
