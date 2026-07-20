using Microsoft.EntityFrameworkCore;
using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Poll
        modelBuilder.Entity<Poll>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Poll>()
            .Property(p => p.Status)
            .HasDefaultValue("open");

        // Question
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Poll)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        // Option
        modelBuilder.Entity<Option>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Vote
        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Question)
            .WithMany(q => q.Votes)
            .HasForeignKey(v => v.QuestionId)
            .OnDelete(DeleteBehavior.Restrict); // tránh cascade conflict

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Option)
            .WithMany()
            .HasForeignKey(v => v.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Chặn vote trùng: 1 voter chỉ vote 1 lần / question
        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.QuestionId, v.VoterToken })
            .IsUnique();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasDefaultValue("User");

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql(
                Database.IsNpgsql() ? "now() at time zone 'utc'" : "GETUTCDATE()");
    }
}