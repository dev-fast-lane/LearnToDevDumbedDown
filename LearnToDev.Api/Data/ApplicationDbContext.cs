using LearnToDev.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    //DI Dependency Injection
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    //Tables of all the model classes in the ER diagram
    public DbSet<Player> Players { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<StudyQuestion> StudyQuestions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobRequirement> JobRequirements { get; set; }
    public DbSet<PassedExam> PassedExams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Combined keys for the two join tables (N:M)
        modelBuilder.Entity<JobRequirement>().HasKey(x => new { x.JobId, x.SchoolId });
        modelBuilder.Entity<PassedExam>().HasKey(x => new { x.PlayerId, x.SchoolId });

        //One player per user
        modelBuilder.Entity<Player>().HasIndex(x => x.UserId).IsUnique();

        //Deleting a job makes its players unemployed instead of deleting them
        modelBuilder.Entity<Player>()
            .HasOne(x => x.CurrentJob)
            .WithMany(x => x.Players)
            .HasForeignKey(x => x.CurrentJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
