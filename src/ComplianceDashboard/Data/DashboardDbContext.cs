using ComplianceDashboard.Entities;
using ComplianceDashboard.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Data;

public class DashboardDbContext(DbContextOptions<DashboardDbContext> options) : DbContext(options)
{
    private static readonly DateTimeOffset SeedCreatedAt = new(2026, 5, 30, 10, 30, 0, TimeSpan.Zero);

    public DbSet<User> Users => Set<User>();

    public DbSet<Operation> Operations => Set<Operation>();

    public DbSet<AppealCase> AppealCases => Set<AppealCase>();

    public DbSet<AppealAnswer> AppealAnswers => Set<AppealAnswer>();

    public DbSet<AppealDocument> AppealDocuments => Set<AppealDocument>();

    public DbSet<SupportDecision> SupportDecisions => Set<SupportDecision>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ConfigureUsers(builder);
        ConfigureOperations(builder);
        ConfigureAppealCases(builder);
        ConfigureAppealAnswers(builder);
        ConfigureAppealDocuments(builder);
        ConfigureSupportDecisions(builder);
        SeedDemoData(builder);
    }

    private static void ConfigureUsers(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(user => user.Phone).HasColumnName("phone").HasMaxLength(32).IsRequired();
            entity.Property(user => user.AccountStatus)
                .HasColumnName("account_status")
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            entity.Property(user => user.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(user => user.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasIndex(user => user.Phone).IsUnique();
        });
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
            entity.Property(operation => operation.RecipientAccount)
                .HasColumnName("recipient_account")
                .HasMaxLength(64);
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
            entity.Property(operation => operation.UpdatedAt)
                .HasColumnName("updated_at")
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
                .HasMaxLength(4000);
            entity.Property(appealCase => appealCase.ClientMessage)
                .HasColumnName("client_message")
                .HasMaxLength(2000);
            entity.Property(appealCase => appealCase.MissingInfoJson)
                .HasColumnName("missing_info_json")
                .HasColumnType("jsonb");
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
            entity.HasIndex(appealCase => appealCase.RouteTo);
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
            entity.Property(answer => answer.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(answer => answer.Case)
                .WithMany(appealCase => appealCase.Answers)
                .HasForeignKey(answer => answer.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(answer => answer.CaseId);
            entity.HasIndex(answer => new { answer.CaseId, answer.QuestionKey }).IsUnique();
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
                .HasMaxLength(2048);
            entity.Property(document => document.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(document => document.Case)
                .WithMany(appealCase => appealCase.Documents)
                .HasForeignKey(document => document.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(document => document.CaseId);
        });
    }

    private static void ConfigureSupportDecisions(ModelBuilder builder)
    {
        builder.Entity<SupportDecision>(entity =>
        {
            entity.ToTable("support_decisions");
            entity.HasKey(decision => decision.Id);

            entity.Property(decision => decision.Id).HasColumnName("id");
            entity.Property(decision => decision.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(decision => decision.Decision)
                .HasColumnName("decision")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(decision => decision.Comment)
                .HasColumnName("comment")
                .HasMaxLength(2000);
            entity.Property(decision => decision.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(decision => decision.Case)
                .WithMany(appealCase => appealCase.Decisions)
                .HasForeignKey(decision => decision.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(decision => decision.CaseId);
        });
    }

    private static void SeedDemoData(ModelBuilder builder)
    {
        builder.Entity<User>().HasData(
            new
            {
                Id = "user_1",
                FullName = "Andrey K.",
                Phone = "+7 777 000 00 00",
                AccountStatus = AccountStatus.LIMITED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = "user_2",
                FullName = "Client Account Appeal",
                Phone = "+7 777 000 00 02",
                AccountStatus = AccountStatus.LIMITED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = "user_3",
                FullName = "Client Operation Confirmation",
                Phone = "+7 777 000 00 03",
                AccountStatus = AccountStatus.ACTIVE,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });

        builder.Entity<Operation>().HasData(
            new
            {
                Id = "op_1",
                UserId = "user_1",
                Amount = 250000m,
                Currency = Currency.KZT,
                RecipientName = "Alisher M.",
                RecipientAccount = "KZ00 **** **** 1234",
                Status = OperationStatus.PENDING_CONFIRMATION,
                BlockReasonCode = BlockReasonCode.CLIENT_CONFIRMATION_REQUIRED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = "op_3",
                UserId = "user_3",
                Amount = 45000m,
                Currency = Currency.KZT,
                RecipientName = "Service Company",
                RecipientAccount = "KZ00 **** **** 9876",
                Status = OperationStatus.PENDING_CONFIRMATION,
                BlockReasonCode = BlockReasonCode.CLIENT_CONFIRMATION_REQUIRED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });

        builder.Entity<AppealCase>().HasData(
            new
            {
                Id = "case_2",
                UserId = "user_2",
                OperationId = (string?)null,
                CaseType = AppealCaseType.ACCOUNT_BLOCK_APPEAL,
                Status = AppealCaseStatus.NEED_MORE_INFO,
                SupportSummary =
                    "Client reported account restriction after several incoming transfers. Supporting documents are not attached yet.",
                ClientMessage = (string?)null,
                MissingInfoJson =
                    "[\"Source of funds confirmation is required\", \"Purpose of incoming transfers is required\"]",
                RouteTo = RouteTo.COMPLIANCE,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = "case_3",
                UserId = "user_3",
                OperationId = "op_3",
                CaseType = AppealCaseType.OPERATION_CONFIRMATION,
                Status = AppealCaseStatus.SUBMITTED,
                SupportSummary =
                    "Client confirmed service payment. Recipient is a company/service. Payment check is attached.",
                ClientMessage = (string?)null,
                MissingInfoJson = "[]",
                RouteTo = RouteTo.SUPPORT,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });
    }
}