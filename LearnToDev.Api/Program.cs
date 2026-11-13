using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LearnToDev.Api.Data;
using LearnToDev.Api.Repositories;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services;
using LearnToDev.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Fant ikke connection string 'DefaultConnection'.");

//Registering ApplicationDbContext so that it can be injected using SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

//Repositories: Scoped gives one per request, same lifetime as ApplicationDbContext
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IStudyQuestionRepository, StudyQuestionRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

//Services: the game rules. Scoped because they use the repositories above
builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Et API skal svare med statuskode, ikke sende brukeren videre til en innloggingsside
builder.Services.ConfigureApplicationCookie(options =>
{
    //JavaScript får ikke lese cookien, så et XSS-angrep ikke kan stjele innloggingen
    options.Cookie.HttpOnly = true;
    //Nettleseren sender ikke cookien med forespørsler fra andre nettsider
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    // Fill the database with game content the first time the app starts
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
    var seedFolder = Path.Combine(AppContext.BaseDirectory, "Data", "Seed");
    await DbInitializer.SeedAsync(db, logger, seedFolder);
    // Create the roles and the test users if they do not exist
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var identityLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
    await IdentitySeeder.SeedAsync(roleManager, userManager, identityLogger);
}

// Configure the HTTP request pipeline.
// Uventede feil og feilkoder uten innhold blir svart med ProblemDetails (JSON)
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LearnToDev API"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
