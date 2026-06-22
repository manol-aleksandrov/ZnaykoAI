using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZnaykoAI.Models;

namespace ZnaykoAI.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestSheet> TestSheets => Set<TestSheet>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<Answer> Answers => Set<Answer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestSheet>(entity =>
        {
            entity.HasKey(ts => ts.Id);

            entity.Property(ts => ts.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(ts => ts.Subject)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(ts => ts.SubTopic)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(ts => ts.UserId)
                .IsRequired();

            entity.HasIndex(ts => ts.UserId);

            entity.HasOne(ts => ts.User)
                .WithMany()
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(ts => ts.Questions)
                .WithOne(q => q.TestSheet)
                .HasForeignKey(q => q.TestSheetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);

            entity.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.AnswerText)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(a => new { a.QuestionId, a.SortOrder });
        });
    }
}
