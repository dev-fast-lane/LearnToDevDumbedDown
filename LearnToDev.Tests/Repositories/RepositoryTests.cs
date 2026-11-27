using LearnToDev.Api.Data;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Tests.Repositories;

//Runs against a real SQLite database in memory, so Include, keys and foreign keys are tested too
public class RepositoryTests : IDisposable
{
    private const string UserId = "user-1";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public RepositoryTests()
    {
        //The database in memory only lives as long as the connection is open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _context.Users.Add(new IdentityUser { Id = UserId, UserName = "en" });
        _context.Schools.AddRange(
            new School { Id = 1, Name = "Frontend" },
            new School { Id = 2, Name = "Backend" });
        _context.StudyQuestions.AddRange(
            NewQuestion(1, "Hva står HTML for?"),
            NewQuestion(1, "Hvilken tag lager en lenke?"),
            NewQuestion(2, "Hva gjør await i C#?"));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static StudyQuestion NewQuestion(int schoolId, string text)
    {
        return new StudyQuestion
        {
            SchoolId = schoolId,
            Text = text,
            Explanation = "Forklaring",
            AnswerOptions = new List<AnswerOption>
            {
                new() { Text = "Riktig", IsCorrect = true },
                new() { Text = "Feil" }
            }
        };
    }

    [Fact]
    public async Task GetAllAsync_IncludesSchoolAndAnswerOptions()
    {
        var repository = new StudyQuestionRepository(_context);

        var result = await repository.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.All(result, q => Assert.Equal(2, q.AnswerOptions.Count));
        Assert.Equal("Backend", result[0].School.Name);
    }

    [Fact]
    public async Task SearchAsync_NoFilters_ReturnsAllQuestions()
    {
        var repository = new StudyQuestionRepository(_context);

        var result = await repository.SearchAsync(null, null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_Text_IgnoresCase()
    {
        var repository = new StudyQuestionRepository(_context);

        var result = await repository.SearchAsync("  html ", null);

        var question = Assert.Single(result);
        Assert.Equal("Hva står HTML for?", question.Text);
    }

    [Fact]
    public async Task SearchAsync_School_ReturnsOnlyThatSchool()
    {
        var repository = new StudyQuestionRepository(_context);

        var result = await repository.SearchAsync(null, 1);

        Assert.Equal(2, result.Count);
        Assert.All(result, q => Assert.Equal(1, q.SchoolId));
    }

    [Fact]
    public async Task SearchAsync_TextAndSchool_BothMustMatch()
    {
        var repository = new StudyQuestionRepository(_context);

        //"hva" is in one Frontend and one Backend question, so only the school filter can pick the right one
        var result = await repository.SearchAsync("hva", 2);

        var question = Assert.Single(result);
        Assert.Equal("Hva gjør await i C#?", question.Text);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyList()
    {
        var repository = new StudyQuestionRepository(_context);

        var result = await repository.SearchAsync("finnes ikke", null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrCreateAsync_CalledTwice_CreatesOnePlayer()
    {
        var repository = new PlayerRepository(_context);

        var first = await repository.GetOrCreateAsync(UserId);
        var second = await repository.GetOrCreateAsync(UserId);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await _context.Players.CountAsync());
    }

    [Fact]
    public async Task AddPassedExamAsync_SameSchoolTwice_SavesOneRow()
    {
        var repository = new PlayerRepository(_context);
        var player = await repository.GetOrCreateAsync(UserId);

        await repository.AddPassedExamAsync(player.Id, 1);
        await repository.AddPassedExamAsync(player.Id, 1);

        Assert.Equal(1, await _context.PassedExams.CountAsync());
    }
}
