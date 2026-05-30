using ComplianceDashboard.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Data;

public class DashboardDbContext(DbContextOptions<DashboardDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<Operation> Operations => Set<Operation>();

    public DbSet<AppealCase> AppealCases => Set<AppealCase>();

    public DbSet<AppealAnswer> AppealAnswers => Set<AppealAnswer>();

    public DbSet<AppealDocument> AppealDocuments => Set<AppealDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);
        ConfigureOperations(builder);
        ConfigureAppealCases(builder);
        ConfigureAppealAnswers(builder);
        ConfigureAppealDocuments(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(user => user.Phone).HasColumnName("phone").HasMaxLength(32).IsRequired();
            entity.Property(user => user.AccountStatus)
                .HasColumnName("account_status")
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            entity.HasIndex(user => user.Phone).IsUnique();
        });

        builder.Entity<IdentityRole>(entity => entity.ToTable("roles"));
        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("user_roles"));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("user_claims"));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("user_logins"));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("role_claims"));
        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("user_tokens"));
    }

    private static void ConfigureOperations(ModelBuilder builder)
    {
        builder.Entity<Operation>(entity =>
        {
            entity.ToTable("operations");
            entity.HasKey(operation => operation.Id);

            entity.Property(operation => operation.Id).HasColumnName("id");
            entity.Property(operation => operation.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(operation => operation.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            entity.Property(operation => operation.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(operation => operation.RecipientName)
                .HasColumnName("recipient_name")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(operation => operation.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(operation => operation.BlockReasonCode)
                .HasColumnName("block_reason_code")
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(operation => operation.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(operation => operation.User)
                .WithMany(user => user.Operations)
                .HasForeignKey(operation => operation.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(operation => operation.UserId);
            entity.HasIndex(operation => operation.Status);
        });
    }

    private static void ConfigureAppealCases(ModelBuilder builder)
    {
        builder.Entity<AppealCase>(entity =>
        {
            entity.ToTable("appeal_cases");
            entity.HasKey(appealCase => appealCase.Id);

            entity.Property(appealCase => appealCase.Id).HasColumnName("id");
            entity.Property(appealCase => appealCase.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(appealCase => appealCase.OperationId).HasColumnName("operation_id");
            entity.Property(appealCase => appealCase.CaseType)
                .HasColumnName("case_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.SupportSummary)
                .HasColumnName("support_summary")
                .HasMaxLength(2000)
                .IsRequired();
            entity.Property(appealCase => appealCase.RouteTo)
                .HasColumnName("route_to")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(appealCase => appealCase.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(appealCase => appealCase.User)
                .WithMany(user => user.AppealCases)
                .HasForeignKey(appealCase => appealCase.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(appealCase => appealCase.Operation)
                .WithMany(operation => operation.AppealCases)
                .HasForeignKey(appealCase => appealCase.OperationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(appealCase => appealCase.UserId);
            entity.HasIndex(appealCase => appealCase.OperationId);
            entity.HasIndex(appealCase => appealCase.Status);
        });
    }

    private static void ConfigureAppealAnswers(ModelBuilder builder)
    {
        builder.Entity<AppealAnswer>(entity =>
        {
            entity.ToTable("appeal_answers");
            entity.HasKey(answer => answer.Id);

            entity.Property(answer => answer.Id).HasColumnName("id");
            entity.Property(answer => answer.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(answer => answer.QuestionKey)
                .HasColumnName("question_key")
                .HasMaxLength(128)
                .IsRequired();
            entity.Property(answer => answer.QuestionText)
                .HasColumnName("question_text")
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(answer => answer.Answer)
                .HasColumnName("answer")
                .HasMaxLength(4000)
                .IsRequired();

            entity.HasOne(answer => answer.Case)
                .WithMany(appealCase => appealCase.Answers)
                .HasForeignKey(answer => answer.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(answer => answer.CaseId);
        });
    }

    private static void ConfigureAppealDocuments(ModelBuilder builder)
    {
        builder.Entity<AppealDocument>(entity =>
        {
            entity.ToTable("appeal_documents");
            entity.HasKey(document => document.Id);

            entity.Property(document => document.Id).HasColumnName("id");
            entity.Property(document => document.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(document => document.DocumentType)
                .HasColumnName("document_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(document => document.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(document => document.MockUrl)
                .HasColumnName("mock_url")
                .HasMaxLength(2048)
                .IsRequired();

            entity.HasOne(document => document.Case)
                .WithMany(appealCase => appealCase.Documents)
                .HasForeignKey(document => document.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(document => document.CaseId);
        });
    }
}